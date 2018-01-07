using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class DiscordHook : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hook of Discord");
            DisplayName.AddTranslation(GameCulture.Chinese, "传送钩爪");
            DisplayName.AddTranslation(GameCulture.Russian, "Крюк Раздора");

            Tooltip.SetDefault("Teleports you to the position of the mouse"); //ItemTooltip.RodofDiscord
            Tooltip.AddTranslation(GameCulture.Chinese, "传送到你鼠标的位置");
			Tooltip.AddTranslation(GameCulture.Russian, "Телепортирует к курсору мыши");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.IlluminantHook);
            item.rare = 7;
            item.value += Item.sellPrice(0, 5, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableAccessories) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.RodofDiscord, 1);
            recipe.AddIngredient(ItemID.SoulofLight, 15);
            recipe.AddIngredient(ItemID.Hook, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
            //Conversion from
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(this, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(ItemID.RodofDiscord);
            recipe.AddRecipe();
        }
    }
}
