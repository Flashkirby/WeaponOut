using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class ChaosHook : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableAccessories; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chaos Hook");
            Tooltip.SetDefault(
                "Teleports to the position of the mouse\n" +
                "Does not wait for Chaos State to finish\n" + 
                "'Faster! Faster!'");
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
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.RodofDiscord, 1);
            recipe.AddIngredient(ItemID.SoulofNight, 15);
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
