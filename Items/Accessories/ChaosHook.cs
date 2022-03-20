using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class ChaosHook : ModItem
    {
        public override void SetStaticDefaults()
        {

            DisplayName.SetDefault("Hook of Chaos");
            DisplayName.AddTranslation(GameCulture.Chinese, "混沌钩爪");
			DisplayName.AddTranslation(GameCulture.Russian, "Крюк Хаоса");

            Tooltip.SetDefault(
                Language.GetTextValue("ItemTooltip.RodofDiscord") +
                "\nCan teleport whilst suffering from Chaos State\n" + 
                "'Faster! Faster!'");
            Tooltip.AddTranslation(GameCulture.Chinese, Language.GetTextValue("ItemTooltip.RodofDiscord") + "\n能够在混沌状态下进行传送\n“快一点！更快一点！”");
            Tooltip.AddTranslation(GameCulture.Russian, Language.GetTextValue("ItemTooltip.RodofDiscord") +
				"\nМожно телепортироваться будучи в Состоянии Хаоса\n" + 
                "'Быстрее! Быстрее!'");

        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.IlluminantHook);
            item.shootSpeed += 0.5f;
            item.rare = 7;
            item.value += Item.sellPrice(0, 5, 0, 0);
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableAccessories) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<DiscordHook>(), 1);
            recipe.SetResult(this);
            recipe.AddRecipe();
            //Conversion from
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(this, 1);
            recipe.SetResult(ModContent.ItemType<DiscordHook>());
            recipe.AddRecipe();
        }
    }
}
