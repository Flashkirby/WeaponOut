using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public class HyperSash : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.EnableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hyper Sash");
            DisplayName.AddTranslation(GameCulture.Chinese, "特急腰带");
			DisplayName.AddTranslation(GameCulture.Russian, "Супер Пояс");

            Tooltip.SetDefault(
                "Restores lost life at the end of a combo\n" +
                "Restores up to 26% of maximum life\n" +
                "Bosses are slower to follow you in the air\n" + 
                "Grants immunity to knockback");
            Tooltip.AddTranslation(GameCulture.Chinese, "恢复在连击时所承受的伤害\n最多可以恢复最大生命值的26%\nBoss在空中追踪你的速度变慢\n免疫击退");
			Tooltip.AddTranslation(GameCulture.Russian,
			    "Восстанавливает потерянное здоровье после комбо\n" +
                "Восстанавливает до 26% здоровья\n" +
                "Боссы медленнее следуют за вами в воздухе\n" + 
                "Защита от отбрасывания");

        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 7;
            item.accessory = true;
            item.value = Item.buyPrice(0, 3, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<FightingSash>(), 1);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 2);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();
            pfx.sashMaxLifeRecoverMult = Math.Max(0.26f, pfx.sashMaxLifeRecoverMult);
            pfx.ghostPosition = true;
            player.noKnockback = true;
        }
    }
}
