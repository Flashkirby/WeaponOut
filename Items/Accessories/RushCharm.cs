using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class RushCharm : ModItem
    {
        public override void SetStaticDefaults() // Eye of Cthulu
        {
            DisplayName.SetDefault("Rush Charm");
			DisplayName.AddTranslation(GameCulture.Russian, "Шарм Порыва");
            Tooltip.SetDefault(
                "Reduces cooldown between dashes");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Сокращает время между рывками");
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
            if (player.dashDelay > 1) player.dashDelay--;
        }
    }
}
