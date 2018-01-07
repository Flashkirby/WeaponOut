using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class DriedEye : ModItem
    {
        public override void SetStaticDefaults() // Eater of Worlds
        {
            DisplayName.SetDefault("Dried Eye");
            DisplayName.AddTranslation(GameCulture.Chinese, "干涸之眼");
            DisplayName.AddTranslation(GameCulture.Russian, "Высушенный Глаз");

            Tooltip.SetDefault(
                "Reduces combo power cost by 2");
            Tooltip.AddTranslation(GameCulture.Chinese, "减少2点连击能量消耗");
			Tooltip.AddTranslation(GameCulture.Russian, 
				"-2 стоимость заряда комбо");

        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 50, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists.Get(player).comboCounterMaxBonus -= 2;
        }
    }
}
