using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class StainedTooth : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults() // Brain of Cthulu
        {
            DisplayName.SetDefault("Impaling Tooth");
            Tooltip.SetDefault(
                "Melee strikes on enemies has a chance to spawn a heart\n" + 
				"'What an eyesore'");
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
