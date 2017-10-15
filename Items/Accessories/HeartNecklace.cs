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
            DisplayName.SetDefault("Heart Frame Necklace");
            Tooltip.SetDefault(
                "Drop star shards after being struck\n" + 
                "Pick falling star shards up to restore life and mana");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 50, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.LifeCrystal, 1);
            recipe.AddIngredient(ItemID.SunplateBlock, 25);
            recipe.AddTile(TileID.SkyMill);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PlayerFX>().criticalHealStar = true;
        }
    }
}
