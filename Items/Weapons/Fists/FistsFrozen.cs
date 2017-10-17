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
    public class FistsFrozen : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;
        public static int projectileID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Frostflame Fist");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 75% increased melee damage and knockback\n" +
                "Combo leaves a chilly slipstream that amplifies frostburn");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            projectileID = mod.ProjectileType<Projectiles.SpiritWindstream>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 360; //656dps vs 20def
            item.useAnimation = 32; // 30%-50% reduction
            item.knockBack = 11f;
            item.tileBoost = 12; // Combo Power

            item.value = Item.sellPrice(0, 15, 0, 0);
            item.rare = 8;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.DD2_PhantomPhoenixShot.WithPitchVariance(1.3f);
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 26;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 8f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const int altHitboxSize = 32;
        const float altDashSpeed = 17f;
        const float altDashThresh = 13f;
        const float altJumpVelo = 16f;

        private static int dashCounter = 0;
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if (player.dashDelay == 0)
                {
                    dashCounter = 2;
                }
                if(dashCounter <= 0)
                {
                    dashCounter = 4;
                    if (mpf.IsComboActive)
                    {
                        Projectile.NewProjectile(player.Center - player.velocity * 2, player.velocity * 0.2f, projectileID, 20, 5f, player.whoAmI);
                    }
                }
                dashCounter--;
            }

            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height,
                    135, 0, 0, 100, default(Color), 3f);
                d.velocity = d.velocity * 0.5f + player.velocity * -0.4f;
                d.noGravity = true;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                d = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.Smoke, 0, 0, 100, default(Color), 0.4f);
                d.fadeIn = 0.7f;
                d.velocity = d.velocity * 0.1f + player.velocity * -0.2f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
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
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if(mpf.IsComboActiveItemOnHit)
                {
                    damage = (int)(damage * 2f);
                    knockBack *= 2f;
                }
                else
                {
                    damage = (int)(damage * 1.75f);
                    knockBack *= 1.75f;
                }
            }
        }

        // Debuff
        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            target.AddBuff(BuffID.Frostburn, 480);
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            target.AddBuff(BuffID.Frostburn, 480);
        }

        // Idle Effect
        public override void HoldItem(Player player)
        {
            if (Main.time % 3 == 0)
            {
                Rectangle r = ModPlayerFists.GetPlayerOnHandRectangle(player, 10);

                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 135,
                    (float)(player.direction * 2), 0f, 100, default(Color), 0.5f)];
                d.position += player.position - player.oldPosition;
                d.velocity = Utils.RandomVector2(Main.rand, -0.5f, 0.5f) + player.velocity * 0.5f - new Vector2(0, player.gravDir);
                d.noGravity = true;
                d.fadeIn = 0.85f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cHandOn, player);
            }
        }
        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 12, 26);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            Vector2 pos = r.TopLeft();
            for (int i = 0; i < 4; i++)
            {
                d = Main.dust[Dust.NewDust(pos, r.Width, r.Height, 135,
                    velocity.X, velocity.Y, 100, default(Color), 1.2f + player.itemAnimation * 0.05f)];
                d.velocity *= 2f;
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
                    SetDash(altDashSpeed, altDashThresh, 0.997f, 0.98f, false, altEffect);
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
