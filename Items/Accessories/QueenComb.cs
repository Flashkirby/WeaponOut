using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class QueenComb : ModItem
    {
        public override void SetStaticDefaults() // Eye of Cthulu
        {
            DisplayName.SetDefault("Honey Pack");
			DisplayName.AddTranslation(GameCulture.Russian, "Медовый Рюкзак");
            Tooltip.SetDefault(
                "Melee strikes on enemies releases honey bees that heal players");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Ближние атаки выпускают медовых пчёл, лечащих игроков");
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
