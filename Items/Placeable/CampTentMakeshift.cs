using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Placeable
{
    public class CampTentMakeshift : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Caving Tent");
            DisplayName.AddTranslation(GameCulture.Chinese, "洞穴帐篷");
			
			if (Main.netMode == 0) {
				Tooltip.SetDefault(
					"Acts as a temporary spawn point");
				Tooltip.AddTranslation(GameCulture.Chinese, "担当暂时的出生点");
			}
			else
			{
				Tooltip.SetDefault(
					"Acts as a temporary spawn point\n" +
					"Unreliable in multiplayer");
				Tooltip.AddTranslation(GameCulture.Chinese, "担当暂时的出生点\n在多人游戏并不靠谱");
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
            item.placeStyle = 1;

            item.useTurn = true;
            item.autoReuse = true;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Cobweb, 30);
            recipe.AddIngredient(ItemID.Wood, 8);
            recipe.anyWood = true;
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}