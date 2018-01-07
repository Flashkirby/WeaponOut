using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class WoodenShield : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Round Shield");
            DisplayName.AddTranslation(GameCulture.Chinese, "圆盾");
            DisplayName.AddTranslation(GameCulture.Russian, "Баклер");

            Tooltip.SetDefault(
                "Grants 10 damage knockback immunity");
            Tooltip.AddTranslation(GameCulture.Chinese, "所受单次伤害低于10时免疫击退");
			Tooltip.AddTranslation(GameCulture.Russian, 
				"Защита от отбрасывания, если урон меньше 10");

        }
        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 28;
            item.accessory = true;
            item.defense = 1;
            item.value = Item.sellPrice(0,0,15,0);
        }

        public override void AddRecipes() {
            if (!ModConf.EnableAccessories) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IronBar, 5);
            recipe.anyIronBar = true;
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.anyWood = true;
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX modPlayer = player.GetModPlayer<PlayerFX>(mod);
            modPlayer.DamageKnockbackThreshold += 10;
        }
    }
}
