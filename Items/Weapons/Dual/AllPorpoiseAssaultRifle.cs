using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Dual
{
    /// <summary>
    /// Vs Vortex Beater
    /// Higher accuracy, similiar DPS at range due to vortex spray
    /// 
    /// </summary>
    public class AllPorpoiseAssaultRifle : ModItem
    {
        public static int proji;
        public static int projii;
        public static int projiii;
        public static int projiv;

        private const int rocketCooldownMax = 90;
        private int rocketCooldown;

        private const float maxAccuracyMod = 1.2f;
        private const float minAccuracyMod = -0.6f;
        private const float addAccuracyMod = 0.2f;
        private const float decayAccuracyMod = 0.07f;
        private const float accuracyDamageMod = 2;
        private float accuracyMod = minAccuracyMod;
        private float Accuracy { get { return Math.Max(0, accuracyMod); } }

        public static short customGlowMask = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("All-Porpoise Assault Rifle");
            Tooltip.SetDefault(
                "<right> to fire a powerful underbarrel rocket\n" + 
                "50% chance to not consume primary ammo\n" +
                "'Perfect for target rich environments'");
            proji = mod.GetProjectile("APARocketI").projectile.type;
            projii = mod.GetProjectile("APARocketII").projectile.type;
            projiii = mod.GetProjectile("APARocketIII").projectile.type;
            projiv = mod.GetProjectile("APARocketIV").projectile.type;
            customGlowMask = WeaponOut.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults()
        {
            item.width = 60;
            item.height = 20;
            item.scale = 1.1f;

            item.UseSound = SoundID.Item99;
            item.useStyle = 5;
            item.useAnimation = 7; //just above insanely fast, because its not really that fast
            item.useTime = 7;
            item.autoReuse = true;

            item.ranged = true;
            item.noMelee = true;
            item.damage = 60;
            item.knockBack = 1f;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 10f;

            item.glowMask = customGlowMask;
            item.rare = 9;
            item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableDualWeapons) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentVortex, 18);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player) { return rocketCooldown <= 0; }
        public override bool CanUseItem(Player player)
        {
            if (PlayerFX.DualItemCanUseItemAlt(player, this,
                1f, 1f,
                1f, 0.4f))
            {
                item.useAmmo = AmmoID.Rocket;
                item.shoot = ProjectileID.RocketI;
            }
            else
            {
                item.useAmmo = AmmoID.Bullet;
                item.shoot = ProjectileID.Bullet;
            }
            return true;
        }

        // Handle accuracy and rocket cooldowns
        Vector2 vector2Mouse = Vector2.Zero;
        public override void HoldItem(Player player)
        {
            if (rocketCooldown >= 0) rocketCooldown--;
            if (rocketCooldown == 0 && player.whoAmI == Main.myPlayer)
            {
                Main.PlaySound(25, player.Center); //alert player has rocket up
            }

            //increase accuracy
            if ((player.itemAnimation == 0 || player.altFunctionUse != 0) && accuracyMod > minAccuracyMod)
            {
                accuracyMod -= decayAccuracyMod;
            }

            if (player.whoAmI == Main.myPlayer)
            {
                if (player.itemAnimation > 0)
                {
                    if (player.itemAnimation == player.itemAnimationMax - 1)
                    {
                        // Make cool noise effect to indiciate crit attacks
                        if (accuracyMod <= 0 && player.altFunctionUse == 0) Main.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchVariance(-0.5f));

                        //Update shoot direction
                        vector2Mouse = Main.MouseWorld - player.Center
                        + new Vector2(Main.rand.NextFloat() - 0.5f, Main.rand.NextFloat() - 0.5f)
                        * 75f * Accuracy;
                        vector2Mouse.Normalize();
                    }
                    int d = Dust.NewDust(player.Center + vector2Mouse * 45f - new Vector2(4, 4) - player.velocity, 0, 0, 45, 0, 0, 125, Color.LightCyan, 0.9f);
                    Main.dust[d].velocity = vector2Mouse * 5f;
                    Main.dust[d].noLight = true;
                    // accuracy modifier
                    if (Accuracy <= 0) {
                        Main.dust[d].color = Color.White;
                        Main.dust[d].alpha = 200;
                        Main.dust[d].scale = 1.5f;
                    }
                }
            }
        }

        public override bool ConsumeAmmo(Player player)
        {
            if (player.altFunctionUse == 0 &&
                Main.rand.Next(2) == 0) return false;
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 0)
            {
                if (accuracyMod < maxAccuracyMod - addAccuracyMod) accuracyMod += addAccuracyMod;
                if (accuracyMod <= 0) {
                    damage = (int)(damage * accuracyDamageMod);
                    if (type == ProjectileID.Bullet) type = ProjectileID.BulletHighVelocity;
                }
                speedX += Accuracy * (Main.rand.NextFloat() - 0.5f);
                speedY += Accuracy * (Main.rand.NextFloat() - 0.5f);
            }
            else
            {
                // Modify values to match alt function damage
                damage = (int)(damage * 140f / 82f);
                knockBack *= 4f;
                speedX *= 5.5f / 10f;
                speedY *= 5.5f / 10f;

                if (rocketCooldown > 0) return false;
                rocketCooldown = rocketCooldownMax;
                damage *= 2; //double for direct hits, this halves on explosion in rocket code
                switch (type)
                {
                    case 134: //Rocket I
                        type = proji;
                        break;
                    case 137: //Rocket II
                        type = projii;
                        break;
                    case 140: //Rocket III
                        type = projiii;
                        break;
                    case 143: //Rocket IV
                        type = projiv;
                        break;
                }
            }
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-14, 0);
        }
    }
}
