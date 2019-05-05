using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Placeable
{
    public class CampTent : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Camping Tent");
            DisplayName.AddTranslation(GameCulture.Chinese, "野营帐篷");
            DisplayName.AddTranslation(GameCulture.Russian, "Палатка");

            if (Main.netMode == 0) {
				Tooltip.SetDefault(
					"Acts as a temporary spawn point");
				Tooltip.AddTranslation(GameCulture.Chinese, "担当暂时的出生点");
                Tooltip.AddTranslation(GameCulture.Russian, "Служит временной точкой спавна");
            }
			else
			{
				Tooltip.SetDefault(
					"Acts as a temporary spawn point\n" +
					"Unreliable in multiplayer");
				Tooltip.AddTranslation(GameCulture.Chinese, "担当暂时的出生点\n在多人游戏并不靠谱");
                Tooltip.AddTranslation(GameCulture.Russian,
                    "Служит временной точкой спавна\n" +
                    "Нестабильна в многопользовательской игре");
            }
        }
        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 99;
            item.consumable = true;
            item.value = 100;
            item.rare = 0;

            item.useStyle = 1;
            item.useAnimation = 15;
            item.useTime = 10;
            item.createTile = mod.TileType("CampTent");
            item.placeStyle = 0;

            item.useTurn = true;
            item.autoReuse = true;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.Wood, 8);
            recipe.anyWood = true;
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}