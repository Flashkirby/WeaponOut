using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class RoyalHealGel : ModItem
    {
        public override void SetStaticDefaults() // Eye of Cthulu
        {
            DisplayName.SetDefault("Band of Panacea");
			DisplayName.AddTranslation(GameCulture.Russian, "Браслет Панацеи");
            Tooltip.SetDefault(
                "Melee strikes reduce the duration of debuffs");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Ближние атаки сокращают время действия негативных эффектов"
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
