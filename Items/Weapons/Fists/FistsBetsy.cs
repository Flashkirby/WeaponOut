using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsBetsy : ModItem
    {
        public static int altEffect = 0;
        public static int projectileID = 0;

        public static short customGlowMask = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hidden Dragon");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 100% increased melee damage and knockback\n" +
                "Combo causes damage to echo to nearby enemies");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            projectileID = mod.ProjectileType<Projectiles.SpiritDragon>();
            customGlowMask = WeaponOut.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 375;
            item.useAnimation = 32; // 30%-50% reduction
            item.knockBack = 11f;
            item.tileBoost = 12; // Combo Power

            item.value = Item.sellPrice(0, 3, 0, 0);
            item.rare = 8;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.DD2_PhantomPhoenixShot;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.glowMask = customGlowMask;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 26;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 8f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const int altHitboxSize = 50;
        const float altDashSpeed = 18f;
        const float altDashThresh = 14f;
        const float altJumpVelo = 18f;

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0)
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    int move = player.GetModPlayer<ModPlayerFists>().specialMove;
                    Projectile.NewProjectile(player.Center, player.velocity * 0.25f, projectileID, 0, 0, Main.myPlayer, move);
                }
            }

            if (player.velocity.Y != 0)
            {
                player.velocity.Y -= (player.gravity * player.gravDir) / 2;
            }

            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height, 228);
                d.noGravity = true;
                d.velocity *= 4f;
                if (i > 3)
                {
                    d.scale /= 2;
                    d.fadeIn = 1f;
                }
                else
                {
                    d.velocity -= player.velocity * 2;
                }
            }
        }

        // Dash
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            if (AltStats(player))
            {
                damage = (int)(damage * 2f);
                knockBack *= 2f;
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (Main.rand.Next(2) == 0)
            {
                target.AddBuff(BuffID.OnFire, 600);
            }
            if(mpf.IsComboActiveItemOnHit)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.whoAmI == target.whoAmI || 
                        npc.friendly || npc.FindBuffIndex(BuffID.BetsysCurse) < 0) continue;
                    if (
                        npc.Center.X <= player.Center.X + Buffs.BetsyRing.debuffDist &&
                        npc.Center.X >= player.Center.X - Buffs.BetsyRing.debuffDist &&
                        npc.Center.Y <= player.Center.Y + Buffs.BetsyRing.debuffDist &&
                        npc.Center.Y >= player.Center.Y - Buffs.BetsyRing.debuffDist)
                    {
                        // Echo damage
                        npc.StrikeNPC(damage / 4, knockBack / 2, player.direction, false);
                    }
                }
            }
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                if (Main.time % 30 == 0)
                {
                    player.AddBuff(mod.BuffType<Buffs.BetsyRing>(), 60, false);
                }
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 12, fistHitboxSize);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            Vector2 pos = r.TopLeft();
            for (int i = 0; i < 4; i++)
            {
                d = Main.dust[Dust.NewDust(pos, r.Width, r.Height, 228,
                    0, 0, 100, default(Color), 0.5f)];
                d.velocity = velocity * 2f;
                d.noGravity = true;
                pos -= pVelo / 5; // trail better at high speeds
            }
        }

        #region Hardmode Dash Base
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if (player.dashDelay == 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                    SetDash(altDashSpeed, altDashThresh, 0.992f, 0.96f, false, altEffect);
                return true;
            }
            return false;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 16f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, altDashSpeed);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
