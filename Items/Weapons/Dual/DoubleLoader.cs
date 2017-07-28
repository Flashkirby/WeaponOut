using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Dual
{
    public class DoubleLoader : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableDualWeapons;
        }
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("IOU: Double Loader");
            Tooltip.SetDefault(
                "20% chance to not consume ammo\n" +
                "<right> to fire darts");
        }
        public override void SetDefaults()
        {
            item.width = 50;
            item.height = 20;

            item.UseSound = SoundID.Item98;
            item.useStyle = 5;
            item.useAnimation = 11;
            item.useTime = 11;
            item.autoReuse = true;

            item.ranged = true;
            item.noMelee = true;
            item.damage = 44;
            item.knockBack = 4f;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = 10;
            item.shootSpeed = 12f;

            item.rare = 8;
            item.value = Item.sellPrice(0, 5, 0, 0);

            /*
            dual = new HelperDual(item, true);
            dual.Damage = 48;
            dual.UseAnimation = 28;
            dual.UseTime = 28;

            dual.UseAmmo = AmmoID.Dart;
            dual.ShootSpeed = 13f;

            dual.FinishDefaults();
            //end by setting default values
            */
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShroomiteBar, 18);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        /*
        public override void OnCraft(Recipe recipe)
        {
            HelperDual.OnCraft(this);
            base.OnCraft(recipe);
        }

        public override bool AltFunctionUse(Player player) { return true; }
        public override void UseStyle(Player player)
        {
            Dual.UseStyleMultiplayer(player);
            PlayerFX.modifyPlayerItemLocation(player, -14, 2);
        }
        public override bool CanUseItem(Player player)
        {
            Dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player)
        {
            Dual.HoldStyle(player);
            base.HoldStyle(player);
        }

        // consume ammo chance
        public override bool ConsumeAmmo(Player player)
        {
            if (Main.rand.Next(5) == 0) { return false; } // if number is 0, don't use ammo also
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.altFunctionUse == 0)
            {
                // (float)r.NextDouble() * 2f - 1f
                speedX += 0.3f * Main.rand.NextFloatDirection();
                speedY += 0.3f * Main.rand.NextFloatDirection();
            }
            return true;
        }
        */
    }
}
