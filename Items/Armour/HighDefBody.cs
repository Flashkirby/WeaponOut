using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighDefBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shaolin Robe");
            DisplayName.AddTranslation(GameCulture.Chinese, "少林武僧长袍");

            Tooltip.SetDefault("12% increased melee critical strike chance\n" +
                "Parrying with fists will steal life\n" +
                "Reduces combo power cost by 1");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加12%近战暴击率\n使用拳套闪避敌人成功时窃取生命值\n减少1点连击能量消耗");
        }
        public override void SetDefaults()
        {
            item.defense = 16;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 12;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.parryLifesteal += 0.1f;
            mpf.comboCounterMaxBonus -= 1;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms) { drawArms = true; drawHands = true; }
    }
}