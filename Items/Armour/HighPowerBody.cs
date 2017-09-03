using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighPowerBody : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sahyun Jacket");
            Tooltip.SetDefault("5% increased melee damage\n" +
                "50% increased parry damage");
        }
        public override void SetDefaults()
        {
            item.defense = 8;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<FistPowerBody>(), 1);
            recipe.AddIngredient(ItemID.SoulofFright, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.05f;
            ModPlayerFists.Get(player).divekickDamage += 0.5f;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms)
        {
            drawHands = true;
        }
    }
}