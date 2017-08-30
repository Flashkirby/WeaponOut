using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class HighPowerLegs : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sahyun Trousers");
            Tooltip.SetDefault("5% increased melee damage\n" + 
                "100% increased divekick damage and knockback");
        }
        public override void SetDefaults()
        {
            item.defense = 0;
            item.value = Item.sellPrice(0, 0, 6, 0);
            item.rare = 6;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.05f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.divekickDamage += 1f;
            mpf.divekickKnockback += 1f;
        }
    }
}