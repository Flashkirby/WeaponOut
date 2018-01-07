using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    /// <summary>
    /// Breaks into crystal shards
    /// </summary>
    public class ScatterShot : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shattered Crystals");
			DisplayName.AddTranslation(GameCulture.Russian, "Сломанные Кристаллы");
            Tooltip.SetDefault(
                "Breaks apart after a short distance");
				Tooltip.AddTranslation(GameCulture.Russian, "Разбиваются на лету");
        }
        public override void SetDefaults()
        {
            item.width = 14;
            item.height = 14;
            item.maxStack = 999;
            item.consumable = true;

            item.ranged = true;
            item.ammo = AmmoID.Bullet;
            item.shoot = mod.ProjectileType<Projectiles.ScatterShot>();
            item.shootSpeed = 4;
            item.damage = 7;
            item.knockBack = 1;

            item.rare = 3;
            item.value = 15;

        }

        public override void AddRecipes()
        {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrystalBullet, 10);
            recipe.SetResult(this, 33);
            recipe.AddRecipe();
        }
    }
}
