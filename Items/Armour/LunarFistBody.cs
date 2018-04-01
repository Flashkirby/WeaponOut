using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class LunarFistBody : ModItem
    {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Starlight Plate");

            Tooltip.SetDefault(
                "15% increased minion and melee damage\n" +
                "100% increased uppercut damage and knockback\n" +
                "Increases your max number of minions by 1");
        }
        public override void SetDefaults() {
            item.defense = 18;
            item.value = 0;
            item.rare = 10;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 10);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddIngredient(ItemID.LunarBar, 18);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player) {
            player.minionDamage += 0.15f;
            player.meleeDamage += 0.15f;
            player.maxMinions += 1;
            ModPlayerFists.Get(player).uppercutDamage += 1f;
            ModPlayerFists.Get(player).uppercutKnockback += 1f;
        }


        public override void DrawHands(ref bool drawHands, ref bool drawArms) { drawArms = true; }
    }
}
