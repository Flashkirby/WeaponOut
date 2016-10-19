using System;
using Microsoft.Xna.Framework;

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

        HelperDual dual;
        public override void SetDefaults()
        {
            item.name = "All-Porpoise Assault Rifle";
            item.toolTip = "Right click to fire a powerful underbarrel rocket";
            item.toolTip2 = "'Perfect for target rich environments'";
            item.width = 60;
            item.height = 20;

            item.useSound = 99;
            item.useStyle = 5;
            item.useAnimation = 7; //just above insanely fast, because its not really that fast
            item.useTime = 7;
            item.autoReuse = true;

            item.ranged = true;
            item.noMelee = true;
            item.damage = 48;
            item.knockBack = 1f;

            item.useAmmo = 14;
            item.shoot = 10;
            item.shootSpeed = 10f;

            item.rare = 9;
            item.value = Item.sellPrice(0, 1, 0, 0);

            dual = new HelperDual(item, true);
            dual.UseAnimation = 16;
            dual.UseTime = 16;

            dual.Damage = 260; //+base 40
            dual.KnockBack = 4f;

            dual.UseAmmo = 771;
            dual.Shoot = 134;
            dual.ShootSpeed = 6f;

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
            recipe.AddIngredient(ItemID.AshBlock, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void OnCraft(Recipe recipe) { HelperDual.OnCraft(item); }

        public override bool AltFunctionUse(Player player) { return rocketCooldown <= 0; }
        public override void UseStyle(Player player)
        {
            dual.UseStyleMultiplayer(player);
            PlayerFX.modifyPlayerItemLocation(player, -22, 0);
        }
        public override bool CanUseItem(Player player)
        {
            dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player) { dual.HoldStyle(player); }

        public override void HoldItem(Player player)
        {
            if (rocketCooldown >= 0) rocketCooldown--;
            if (rocketCooldown == 0 && player.whoAmI == Main.myPlayer)
            {
                Main.PlaySound(25, player.Center); //alert player has rocket up
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 0)
            {
                speedX += 0.3f * (Main.rand.NextFloat() - 0.5f);
                speedY += 0.3f * (Main.rand.NextFloat() - 0.5f);
            }
            else
            {
                if (rocketCooldown > 0) return false;
                rocketCooldown = rocketCooldownMax;
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
    }
}
