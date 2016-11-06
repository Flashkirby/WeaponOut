using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class Pavise : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            equips.Add(EquipType.Shield);
            return true;
        }
        public override void SetDefaults()
        {
            item.name = "Pavise";
            item.width = 24;
            item.height = 28;
            item.toolTip = "10 defense when facing attacks"; //see playerfx
            item.toolTip2 = "Grants immunity to knockback when facing attacks"; //see playerfx
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 50, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX modPlayer = player.GetModPlayer<PlayerFX>(mod);
            modPlayer.FrontDefence += 10;
            modPlayer.frontNoKnockback = true;
        }
    }
}
