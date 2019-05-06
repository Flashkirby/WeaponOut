using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items
{
    /// <summary>
    /// Breaks into 3 shots
    /// </summary>
    public class SplinterShot : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Splinter Shot");
            DisplayName.AddTranslation(GameCulture.Chinese, "破片霰弹");
            DisplayName.AddTranslation(GameCulture.Russian, "Отделённый Осколок");

            Tooltip.SetDefault(
                "Separates shortly after firing");
            Tooltip.AddTranslation(GameCulture.Chinese, "开火不久后分裂");
            Tooltip.AddTranslation(GameCulture.Russian, "Отделяется незадолго после выстрела");

        }
        public override void SetDefaults()
        {
            item.width = 14;
            item.height = 14;
            item.maxStack = 999;
            item.consumable = true;

            item.ranged = true;
            item.ammo = AmmoID.Bullet;
            item.shoot = mod.ProjectileType<Projectiles.SplinterShot>();
            item.shootSpeed = 2;
            item.damage = 4;
            item.knockBack = 1;

            item.rare = 1;
            item.value = 15;

        }

        public override void AddRecipes()
        {
            if (!ModConf.EnableBasicContent) return;
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
