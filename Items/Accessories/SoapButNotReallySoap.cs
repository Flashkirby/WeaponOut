using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class SoapButNotReallySoap : ModItem
    {
        public override void SetStaticDefaults() // Brain of Cthulu
        {
            DisplayName.SetDefault("Soapron");
            DisplayName.AddTranslation(GameCulture.Chinese, "猪龙皂");

            Tooltip.SetDefault(
                "Summons bubbles over time that can be popped for combo power");
            Tooltip.AddTranslation(GameCulture.Chinese, "召唤泡泡，打破后可以获得连击能量");

        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 8;
            item.accessory = true;
            item.value = Item.sellPrice(0, 2, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PlayerFX>().summonSoap = true;
        }
    }
}
