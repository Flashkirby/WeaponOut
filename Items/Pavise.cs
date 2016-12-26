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
            if (ModConf.enableAccessories)
            {
                equips.Add(EquipType.Shield);
                return true;
            }
            return false;
        }

        public override void SetDefaults()
        {
            item.name = "Ceratopsian Shield";
            item.width = 24;
            item.height = 28;
            item.toolTip = "10 defense when facing attacks"; //see playerfx
            item.toolTip2 = "Grants immunity to knockback when facing attacks"; //see playerfx
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FossilOre, 25);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX modPlayer = player.GetModPlayer<PlayerFX>(mod);
            modPlayer.FrontDefence += 10;
            modPlayer.frontNoKnockback = true;
        }
    }
}
