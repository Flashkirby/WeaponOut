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
    public class MeteorBreakshot : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Meteoric Breakshot");
            DisplayName.AddTranslation(GameCulture.Chinese, "陨铁爆旋弹");

            Tooltip.SetDefault(
                "Creates several meteoric shards on impact");
            Tooltip.AddTranslation(GameCulture.Chinese, "击中物体后溅射陨铁碎片");
        }
        public override void SetDefaults()
        {
            item.width = 14;
            item.height = 14;
            item.maxStack = 999;
            item.consumable = true;

            item.ranged = true;
            item.ammo = AmmoID.Bullet;
            item.shoot = mod.ProjectileType<Projectiles.MeteorBreakshot>();
            item.shootSpeed = 2.5f;
            item.damage = 9;
            item.knockBack = 3f;

            item.rare = 2;
            item.value = 20;

        }

        public override void AddRecipes()
        {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteorShot, 50);
            recipe.AddIngredient(ItemID.Hellstone);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 50);
            recipe.AddRecipe();
        }
    }
}
