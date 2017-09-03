using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighDefBody : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shaolin Robe");
            Tooltip.SetDefault("5% increased melee critical strike chance\n" +
                "Makes parrying with fists easier");
        }
        public override void SetDefaults()
        {
            item.defense = 16;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<FistDefBody>(), 1);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 5;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.longParry = true;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms) { drawArms = true; drawHands = true; }
    }
}