using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class HeartNecklace : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableAccessories; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lucklace");
            Tooltip.SetDefault(
                "Drop stars that restore life and mana after being struck");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 50, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PlayerFX>().criticalHealStar = true;
        }
    }
}
