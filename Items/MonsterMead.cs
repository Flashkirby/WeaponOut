using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items
{
    public class MonsterMead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Monster Mead");
            DisplayName.AddTranslation(GameCulture.Chinese, "恶魔蜂蜜酒");
			DisplayName.AddTranslation(GameCulture.Russian, "Чудовищный Мёд");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Ale);
            item.buffType = ModContent.BuffType<Buffs.TipsyMead>();
            item.buffTime = 3600 * 5; // 5 mins
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Mug);
            recipe.AddIngredient(ItemID.Gel);
            recipe.AddIngredient(ItemID.Lens);
            recipe.AddIngredient(ItemID.Stinger);
            recipe.AddTile(TileID.Kegs);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void HoldStyle(Player player)
        {
            player.itemLocation.X -= player.direction * 3;
            player.itemLocation.Y += player.gravDir * 8;
        }
    }
}
