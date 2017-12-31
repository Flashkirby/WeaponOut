using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

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
            DisplayName.AddTranslation(GameCulture.Chinese, "魔晶碎片");

            Tooltip.SetDefault(
                "Breaks apart after a short distance");
            Tooltip.AddTranslation(GameCulture.Chinese, "发射短距离且高威力的魔晶");
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
