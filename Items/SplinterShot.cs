using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    /// <summary>
    /// Breaks into 3 shots
    /// </summary>
    public class SplinterShot : ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Splinter Shot";
            item.toolTip = "Seperates shortly after firing";
            item.width = 14;
            item.height = 14;
            item.maxStack = 999;
            item.consumable = true;

            item.ranged = true;
            item.ammo = AmmoID.Bullet;
            item.shoot = mod.ProjectileType("SplinterShot");
            item.shootSpeed = 2;
            item.damage = 4;
            item.knockBack = 1;

            item.rare = 1;
            item.value = 15;

        }

        public override void AddRecipes()
        {

            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.MusketBall, 15);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.WormTooth, 1);
                }
                else
                {
                    recipe.AddIngredient(ItemID.Vertebrae, 1);
                }
                recipe.AddTile(TileID.WorkBenches);
                recipe.SetResult(this, 15);
                recipe.AddRecipe();
            }
        }
    }
}
