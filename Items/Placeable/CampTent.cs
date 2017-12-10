using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Placeable
{
    public class CampTent : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Camping Tent");
            Tooltip.SetDefault(
                "Acts as a temporary spawn point\n" +
                "Unreliable in multiplayer");
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