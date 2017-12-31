using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class RoyalHealGel : ModItem
    {
        public override void SetStaticDefaults() // Eye of Cthulu
        {
            DisplayName.SetDefault("Band of Panacea");
            DisplayName.AddTranslation(GameCulture.Chinese, "万灵丹手环");

            Tooltip.SetDefault(
                "Melee strikes reduce the duration of debuffs");
            Tooltip.AddTranslation(GameCulture.Chinese, "使用近战武器攻击敌人时细微减少不良状态的持续时间");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PlayerFX>().debuffRecover = true;
        }
    }
}
