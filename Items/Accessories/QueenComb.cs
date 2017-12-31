using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class QueenComb : ModItem
    {
        public override void SetStaticDefaults() // Eye of Cthulu
        {
            DisplayName.SetDefault("Honey Pack");
            DisplayName.AddTranslation(GameCulture.Chinese, "蜂蜜包");

            Tooltip.SetDefault(
                "Melee strikes on enemies releases honey bees that heal players");
            Tooltip.AddTranslation(GameCulture.Chinese, "使用近战武器攻击敌人时释放治疗玩家的蜜蜂");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 3;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PlayerFX>().beeHealing = true;
        }
    }
}
