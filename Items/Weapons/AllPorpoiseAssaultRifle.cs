using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
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
        private float accuracyMod = minAccuracyMod;
        private float Accuracy { get { return Math.Max(0, accuracyMod); } }

        public static short customGlowMask = 0;

        /// <summary>
        /// Generate a completely legit glowmask ;)
        /// </summary>
        public override bool Autoload(ref string name)
        {
            if (Main.netMode != 2 && ModConf.enableDualWeapons)
            {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++)
                {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = mod.GetTexture("Glow/" + this.GetType().Name + "_Glow");
                customGlowMask = (short)(glowMasks.Length - 1);
                Main.glowMaskTexture = glowMasks;
                return true;
            }
            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("IOU: All-Porpoise Assault Rifle");
            Tooltip.SetDefault(
                "<right> to fire a powerful underbarrel rocket\n" + 
                "50% chance to not consume ammo\n" +
                "'Perfect for target rich environments'");
        }
        /*
        HelperDual dual;
        HelperDual Dual { get { if (dual == null) { HelperDual.OnCraft(this); } return dual; } }
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
            item.damage = 82;
            item.knockBack = 1f;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = 10;
            item.shootSpeed = 10f;

            item.glowMask = customGlowMask;
            item.rare = 9;
            item.value = Item.sellPrice(0, 1, 0, 0);

            dual = new HelperDual(item, true);
            dual.UseAnimation = 16;
            dual.UseTime = 16;

            dual.Damage = 140; //+base 40
            dual.KnockBack = 4f;

            dual.UseAmmo = AmmoID.Rocket;
            dual.Shoot = 134;
            dual.ShootSpeed = 5.5f;

            proji = mod.GetProjectile("APARocketI").projectile.type;
            projii = mod.GetProjectile("APARocketII").projectile.type;
            projiii = mod.GetProjectile("APARocketIII").projectile.type;
            projiv = mod.GetProjectile("APARocketIV").projectile.type;

            dual.FinishDefaults();
            //end by setting default values

            rocketCooldown = 0;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentVortex, 18);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void OnCraft(Recipe recipe) { HelperDual.OnCraft(this); }

        public override bool AltFunctionUse(Player player) { return rocketCooldown <= 0; }
        public override void UseStyle(Player player)
        {
            Dual.UseStyleMultiplayer(player);
            PlayerFX.modifyPlayerItemLocation(player, -22, 0);
        }
        public override bool CanUseItem(Player player)
        {
            Dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player) { Dual.HoldStyle(player); }

        Vector2 vector2Mouse = Vector2.Zero;
        public override void HoldItem(Player player)
        {
            if (rocketCooldown >= 0) rocketCooldown--;
            if (rocketCooldown == 0 && player.whoAmI == Main.myPlayer)
            {
                Main.PlaySound(25, player.Center); //alert player has rocket up
            }

            //increase accuracy
            if((player.itemAnimation == 0 || player.altFunctionUse != 0) && accuracyMod > minAccuracyMod)
            {
                accuracyMod -= decayAccuracyMod;
            }

            if(player.whoAmI == Main.myPlayer)
            {
                if (player.itemAnimation > 0)
                {
                    if(player.itemAnimation == player.itemAnimationMax - 1)
                    {
                        //Update shoot direction
                        vector2Mouse = Main.MouseWorld - player.Center
                            + new Vector2(Main.rand.NextFloat() - 0.5f, Main.rand.NextFloat() - 0.5f)
                            * 75f * Accuracy;
                        vector2Mouse.Normalize();
                    }
                    int d = Dust.NewDust(player.Center + vector2Mouse * 45f - new Vector2(4, 4) - player.velocity,
                        0, 0, 45, 0, 0, 125, Accuracy <= 0 ? Color.White : Color.LightCyan, Accuracy <= 0 ? 1.5f : 0.9f);
                    Main.dust[d].velocity = vector2Mouse * 5f;
                    Main.dust[d].noLight = true;
                }
            }
        }

        public override bool ConsumeAmmo(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                if (Main.rand.Next(2) == 0) return false;
            }
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 0)
            {
                if (accuracyMod < maxAccuracyMod - addAccuracyMod) accuracyMod += addAccuracyMod;
                if (accuracyMod <= 0) damage = (int)(damage * 1.33);
                speedX += Accuracy * (Main.rand.NextFloat() - 0.5f);
                speedY += Accuracy * (Main.rand.NextFloat() - 0.5f);
            }
            else
            {
                if (rocketCooldown > 0) return false;
                rocketCooldown = rocketCooldownMax;
                damage *= 2; //douvle for direct hits, this halves on explosion in rocket code
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
        */
    }
}
