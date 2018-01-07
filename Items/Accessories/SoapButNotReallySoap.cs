using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class SoapButNotReallySoap : ModItem
    {
        public override void SetStaticDefaults() // Brain of Cthulu
        {
            DisplayName.SetDefault("Soapron");
			DisplayName.AddTranslation(GameCulture.Russian, "Мыльрон");
            Tooltip.SetDefault(
                "Summons bubbles over time that can be popped for combo power");
				Tooltip.AddTranslation(GameCulture.Russian, "Создаёт пузыри, при лопании которых генерируется заряд комбо");
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
