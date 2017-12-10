using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class FistPowerBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dobok Jacket");
            Tooltip.SetDefault("5% increased melee damage\n" +
                "50% increased parry damage");
        }
        public override void SetDefaults()
        {
            item.defense = 2;
            item.value = Item.sellPrice(0, 0, 8, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddIngredient(ItemID.Cactus, 10);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.05f;
            ModPlayerFists.Get(player).parryDamage += 0.5f;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms)
        {
            drawHands = true;
        }
    }
}