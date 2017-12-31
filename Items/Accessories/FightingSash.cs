using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public class FightingSash : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Journeyman Sash");
            DisplayName.AddTranslation(GameCulture.Chinese, "熟手腰带");

            Tooltip.SetDefault(
                "Restores lost life at the end of a combo\n" +
                "Restores up to 25% of maximum life\n" + 
                "Grants 20 damage knockback immunity");
            Tooltip.AddTranslation(GameCulture.Chinese, "恢复在连击时所承受的伤害\n最多可以恢复最大生命值的25%\n所受单次伤害低于20时免疫击退");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 2;
            item.accessory = true;
            item.value = Item.buyPrice(0, 1, 0, 0);
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.JungleSpores, 4);
            recipe.AddIngredient(ItemID.Amber, 1);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();
            pfx.sashMaxLifeRecoverMult = Math.Max(0.25f, pfx.sashMaxLifeRecoverMult);
            pfx.DamageKnockbackThreshold += 20;
        }
    }
}
