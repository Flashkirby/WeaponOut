using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class LifeRoot : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.EnableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Symbiotic Root");
            DisplayName.AddTranslation(GameCulture.Chinese, "共生之心");

            Tooltip.SetDefault(
                "Hearts drop more frequently and heal 5 more life");
            Tooltip.AddTranslation(GameCulture.Chinese, "红心的掉落概率更高\n治疗5点更多的生命值");

        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 7;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PlayerFX>().heartDropper = true;
            player.GetModPlayer<PlayerFX>().heartBuff = true;
        }
    }
}
