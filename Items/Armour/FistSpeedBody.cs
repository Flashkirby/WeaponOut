using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class FistSpeedBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boxing Vest");
            DisplayName.AddTranslation(GameCulture.Chinese, "拳击背心");
            DisplayName.AddTranslation(GameCulture.Russian, "Боксёрский Жилет");

            Tooltip.SetDefault("5% increased melee attack speed\n" +
                "100% increased uppercut damage and knockback");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加5%近战攻速\n增加100%上勾拳伤害和击退\n提醒：上勾拳指的是你使用拳套按上方向键攻击");
            Tooltip.AddTranslation(GameCulture.Russian,
				"+5% скорость ближнего боя\n" +
                "+100% урон и отбрасывание в прыжке");

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
            recipe.AddIngredient(ItemID.Blinkroot, 3);
            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddTile(TileID.Loom);
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