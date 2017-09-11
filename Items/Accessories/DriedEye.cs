using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class DriedEye : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults() // Eater of Worlds
        {
            DisplayName.SetDefault("Dried Eye");
            Tooltip.SetDefault(
                "Reduces combo power cost by 2");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists.Get(player).comboCounterMaxBonus -= 2;
        }
    }
}
