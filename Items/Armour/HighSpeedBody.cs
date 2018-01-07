using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighSpeedBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Champion Belt");
			DisplayName.AddTranslation(GameCulture.Russian, "Пояс Чемпиона");
            Tooltip.SetDefault("15% increased melee attack speed\n" +
                "125% increased uppercut damage and knockback");
				Tooltip.AddTranslation(GameCulture.Russian, "+15% скорость ближнего боя\n");
				"+125% урон и отбрасывание в прыжке");
        }
        public override void SetDefaults()
        {
            item.defense = 12;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddIngredient(ItemID.SoulofSight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeSpeed += 0.15f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.uppercutDamage += 1.25f;
            mpf.uppercutKnockback += 1.25f;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms) { drawArms = true; drawHands = true; }
    }
}