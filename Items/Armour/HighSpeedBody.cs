using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighSpeedBody : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Champion Belt");
            Tooltip.SetDefault("5% increased melee attack speed\n" +
                "100% increased uppercut damage and knockback");
        }
        public override void SetDefaults()
        {
            item.defense = 1;
            item.value = Item.sellPrice(0, 4, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<FistSpeedBody>(), 1);
            recipe.AddIngredient(ItemID.SoulofSight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeSpeed += 0.05f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.uppercutDamage += 1f;
            mpf.uppercutKnockback += 1f;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms) { drawArms = true; drawHands = true; }
    }
}