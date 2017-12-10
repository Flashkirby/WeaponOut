using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class Pavise : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fossil Shield"); //Ceratopsian Shield
            Tooltip.SetDefault(
                "10 defense when facing attacks\n" +
                "Grants immunity to knockback when facing attacks");
        }
        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 28;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableAccessories) return;
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
