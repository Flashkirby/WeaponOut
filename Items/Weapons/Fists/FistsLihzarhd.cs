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
    public class FistsLihzarhd : ModItem
    {
        public static int altEffect = 0;
        public static int projectileID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strongarm");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash for a projectile deflecting punch\n" +
                "Combo causes successful strikes to generate powerful shockwaves");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            projectileID = mod.ProjectileType<Projectiles.SpiritQuake>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 260;
            item.useAnimation = 20; // 30%-50% reduction
            item.knockBack = 12f;
            item.tileBoost = 8; // Combo Power

            item.shootSpeed = 16f;

            item.value = Item.sellPrice(0, 3, 0, 0);
            item.rare = 7;

            item.UseSound = SoundID.DD2_SonicBoomBladeSlash;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 26;
        const float fistDashSpeed = 9f;
        const float fistDashThresh = 6f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const int altHitboxSize = 32;
        const float altDashSpeed = 16f;
        const float altDashThresh = 12f;
        const float altJumpVelo = 16.8f;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.GolemFist, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0) { }

            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Smoke, -player.velocity.X / 5, -player.velocity.Y / 5, 50 + 20 * i);
                d.position -= d.velocity * 10f;
                d.fadeIn = 1.2f;
            }

            player.GetModPlayer<PlayerFX>().reflectingProjectilesForce = true;
            player.GetModPlayer<PlayerFX>().reflectingProjectilesParryStyle = true;
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
                damage = (int)(damage * 1.75f);
                knockBack *= 1.75f;
            }
        }

        // Dash && Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                Vector2 shockVelo = (target.Center - player.Center).SafeNormalize(default(Vector2)) * item.shootSpeed;
                Vector2 rotVelo;

                rotVelo = shockVelo.RotatedBy(0.4f);
                Projectile.NewProjectile(target.Center + rotVelo, rotVelo, projectileID, damage / 2, knockBack, Main.myPlayer, 0f);
                rotVelo = shockVelo.RotatedBy(-0.4f);
                Projectile.NewProjectile(target.Center + rotVelo, rotVelo, projectileID, damage / 2, knockBack, Main.myPlayer, 1f); //No sound from this one
            }
            if (AltStats(player))
            {
                if (target.velocity.Equals(default(Vector2))) return;
                target.velocity += player.velocity / 2;
                target.netUpdate = true;
            }
        }

        // Combo
        bool hitGround;
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                // On landing with divekicks
                if (mpf.specialMove == 2 && player.velocity.Y == 0 && !hitGround)
                {
                    Main.PlaySound(SoundID.DD2_MonkStaffGroundImpact, player.position);
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Vector2 displace = new Vector2(32, 32 * player.gravDir);
                        Vector2 velo = new Vector2(player.direction * item.shootSpeed, player.gravDir * -3f);
                        Projectile.NewProjectile(player.Center + displace, velo, projectileID, (int)(item.damage * player.meleeDamage / 2), 8f, Main.myPlayer, 1f);
                        velo.X *= -1;
                        Projectile.NewProjectile(player.Center + displace, velo, projectileID, (int)(item.damage * player.meleeDamage / 2), 8f, Main.myPlayer, 1f);
                    }
                }
                hitGround = player.velocity.Y == 0;
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
