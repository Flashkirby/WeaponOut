using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class HighSpeedLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Champion Guards");
            DisplayName.AddTranslation(GameCulture.Chinese, "冠军护靴");
            DisplayName.AddTranslation(GameCulture.Russian, "Поножи Чемпиона");

            Tooltip.SetDefault("10% increased movement speed\n" +
                "50% increased increased divekick damage\n" +
                "Reduces combo power cost by 2");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加10%移动速度\n增加50%下踢伤害\n减少2点连击能量消耗\n提醒：下踢指的是你使用拳套在空中按下方向键攻击");
            Tooltip.AddTranslation(GameCulture.Russian,
                "+10% скорость бега\n" +
                "+50% урон в падении\n" +
                "-2 стоимость заряда комбо");
        }
        public override void SetDefaults()
        {
            item.defense = 9;
            item.value = Item.sellPrice(0, 4, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ItemID.SoulofSight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            if (!male) equipSlot = mod.GetEquipSlot("HighSpeedLegs_Female", EquipType.Legs);
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.1f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.divekickDamage += 0.5f;
            mpf.comboCounterMaxBonus -= 2;
        }
    }
}