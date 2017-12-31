using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class HeartNecklace : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Heart Frame Necklace");
            DisplayName.AddTranslation(GameCulture.Chinese, "心之框架项链");

            Tooltip.SetDefault(
                "Drop star shards after being struck\n" + 
                "Catch falling star shards to restore life and mana");
            Tooltip.AddTranslation(GameCulture.Chinese, "受击时有概率掉落星之碎片\n收集星之碎片可以回复生命值和魔力值");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 0, 50, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableAccessories) return;
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
