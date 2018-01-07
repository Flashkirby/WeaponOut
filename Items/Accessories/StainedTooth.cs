using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class StainedTooth : ModItem
    {
        public override void SetStaticDefaults() // Brain of Cthulu
        {
            DisplayName.SetDefault("Impaling Tooth");
            DisplayName.AddTranslation(GameCulture.Chinese, "血肉刺牙");
            DisplayName.AddTranslation(GameCulture.Russian, "Пронзающий Зуб");

            Tooltip.SetDefault(
                "Hurting enemies has a chance to spawn a heart\n" + 
				"'What an eyesore'");
            Tooltip.AddTranslation(GameCulture.Chinese, "伤害敌人时有概率掉落红心");
			Tooltip.AddTranslation(GameCulture.Russian, 
				"При атаке иногда выпадают сердца\n" +
				"'Что-то в глаз попало'");

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
            player.GetModPlayer<PlayerFX>().heartDropper = true;
        }
    }
}
