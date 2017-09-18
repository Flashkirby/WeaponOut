using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn)]
    public class FistsMartian : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;
        public static int projectileID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Booster Fist");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 120% increased melee damage\n" +
                "Combo grants an follow-up punch\n" + 
                "'Rocket... PUUUUUNCH!'");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            projectileID = mod.ProjectileType<Projectiles.SpiritMartianFist>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 300;
            item.useAnimation = 26; // 30%-50% reduction
            item.knockBack = 9.5f;
            item.tileBoost = 6; // Combo Power

            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 8;

            item.UseSound = SoundID.Item92;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 28;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 8f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const int altHitboxSize = 40;
        const float altDashSpeed = 20f;
        const float altDashThresh = 12f;
        const float altJumpVelo = 18f;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.InfluxWaver, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            if (player.velocity.Y != 0 && !player.controlDown)
            {
                player.velocity.Y -= (player.gravity * player.gravDir) / 2;
            }

            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height, 229,
                    player.velocity.X * -0.2f, player.velocity.Y * -0.2f);
                d.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(player.Center, 229);
                d.position += player.velocity * 3f;
                d.velocity = 0.2f * player.velocity.RotatedBy(1f + 0.1f * i);
                d.position += d.velocity * 2f;
                d.noGravity = true;

                d = Dust.NewDustPerfect(player.Center, 229);
                d.position += player.velocity * 3f;
                d.velocity = 0.2f * player.velocity.RotatedBy(-(1f + 0.1f * i));
                d.position += d.velocity * 2f;
                d.noGravity = true;
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
                damage = (int)(damage * 2.2f);
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                Vector2 launchVelo = new Vector2(8).RotatedByRandom(Math.PI * 2) - player.velocity / 2f;
                Projectile.NewProjectile(player.Center + launchVelo * 6, launchVelo, projectileID, 150, 1f, Main.myPlayer, target.whoAmI);
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 12, fistHitboxSize);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * 2f + pVelo * 0.5f;
            Dust d;

            Vector2 pos = r.TopLeft();
            for (int i = 0; i < 3; i++)
            {
                d = Main.dust[Dust.NewDust(pos, r.Width, r.Height, DustID.t_Martian,
                    0, 0, 0, default(Color), 0.75f)];
                d.velocity = velocity;
                d.noGravity = true;
                pos -= d.velocity * 4 + pVelo / 5; // trail better at high speeds
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
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 1f, 16f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 1f, altDashSpeed);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
