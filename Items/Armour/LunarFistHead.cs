using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class LunarFistHead : ModItem
    {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Starlight Circlet");

            Tooltip.SetDefault(
                "15% increased minion damage, 13% increased melee critical strike chance\n" +
                "Fighting bosses slowly empowers next melee attack, up to 2500%\n" +
                "Increases your max number of minions by 1");
        }
        public override void SetDefaults() {
            item.defense = 16;
            item.value = 0;
            item.rare = 10;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 4);
            recipe.AddIngredient(ItemID.FragmentSolar, 4);
            recipe.AddIngredient(ItemID.LunarBar, 6);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player) {
            player.minionDamage += 0.15f;
            player.meleeCrit += 13;
            player.maxMinions += 1;
            player.GetModPlayer<PlayerFX>().patienceDamage = 25f; // Can do up to 2500%
            player.GetModPlayer<PlayerFX>().patienceBuildUpModifier += 0.4f; // 75->105%
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair) { drawHair = true; }
    }
}
