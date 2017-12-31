using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class FistDefBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dynasty Jacket");
            DisplayName.AddTranslation(GameCulture.Chinese, "王朝夹克");

            Tooltip.SetDefault("5% increased melee critical strike chance\n" +
                "Makes parrying with fists easier");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加5%近战暴击率\n让你使用拳套闪避敌人更加容易");
        }
        public override void SetDefaults()
        {
            item.defense = 2;
            item.value = Item.buyPrice(0, 0, 50, 0);

            item.width = 18;
            item.height = 18;
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 5;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.longParry = true;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms) { drawArms = true; drawHands = true; }
    }
}