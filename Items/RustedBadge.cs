using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    /// <summary>
    /// The only way to obtain mirror badge pre-hardmode, by fishing as a bonus.
    /// </summary>
    public class RustedBadge : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rusted Badge");
			DisplayName.AddTranslation(GameCulture.Russian, "Ржавая Медаль");
            Tooltip.SetDefault("'It could do with some polishing...'");
			Tooltip.AddTranslation(GameCulture.Russian, "'Её бы отполировать...'");
        }
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 20;
            item.rare = -1;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableAccessories) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<RustedBadge>(), 1);
            recipe.AddIngredient(ItemID.Obsidifish, 3); // Rough polish
            recipe.AddIngredient(ItemID.ArmoredCavefish, 2); // Fine polish
            recipe.AddIngredient(ItemID.Silk, 1); // Buffing
            recipe.AddTile(TileID.Sawmill);
            recipe.needWater = true;
            recipe.SetResult(mod.ItemType<Accessories.MirrorBadge>(), 1);
            recipe.AddRecipe();
        }
    }
}
