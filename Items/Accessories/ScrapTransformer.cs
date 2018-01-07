using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class ScrapTransformer : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Transformer");
            DisplayName.AddTranslation(GameCulture.Chinese, "废弃的变压器");
            DisplayName.AddTranslation(GameCulture.Russian, "Механический Трансформер");

            Tooltip.SetDefault(
                "Parrying with fists will steal life");
            Tooltip.AddTranslation(GameCulture.Chinese, "使用拳套闪避敌人成功时窃取生命值");
			Tooltip.AddTranslation(GameCulture.Russian, 
				"Парирование руками крадёт здоровье");

        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 5;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            mpf.parryLifesteal += 0.05f; 
        }
    }
}
