using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items
{
    /// <summary>
    /// Provides AoE banner bonus to players "nearby".
    /// Buff DPS pays itself back in a 7-8 player team,
    /// </summary>
    public class RallyBannerRed : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Red Rally Banner");
            DisplayName.AddTranslation(GameCulture.Chinese, "振军旗帜");
            DisplayName.AddTranslation(GameCulture.Russian, "Красный Боевой Флаг");
            RallyBannerRed.SetStaticToolTipDefaults(this);
        }
        public static void SetStaticToolTipDefaults(ModItem modItem) {
            modItem.Tooltip.SetDefault(
                "Increases stats for your team whilst held\n" +
                "Team is not affected by banner color");
            modItem.Tooltip.AddTranslation(GameCulture.Chinese, "举起旗帜时增加你队友的属性\n团队不受旗帜颜色的影响");
            modItem.Tooltip.AddTranslation(GameCulture.Russian, "Вдохновляет членов вашей команды\n" +
                "Цвет флага не имеет значения");
        }
        public override void SetDefaults()
        {
            RallyBannerRed.SetDefaults(item);
        }
        public override void AddRecipes()
        {
            RallyBannerRed.AddRecipe(this);
        }

        public override void HoldStyle(Player player)
        {
            RallyBannerRed.HoldStyle(item, player);
        }

        #region Other Static Methods

        public static void SetDefaults(Item item)
        {
            item.CloneDefaults(ItemID.Umbrella);
            item.width = 28;
            item.height = 48;
            item.value = 3000;
        }
        public static void AddRecipe(ModItem modItem)
        {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(modItem.mod);
            recipe.AddIngredient(ItemID.Silk, 5);
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.anyWood = true;
            recipe.AddIngredient(ItemID.FallenStar, 1);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(modItem);
            recipe.AddRecipe();
        }

        public static void HoldStyle(Item item, Player player)
        {
            player.itemRotation += (0.79f * (float)player.direction) * player.gravDir;
            player.itemLocation.X -= (float)(item.width / 2) * (float)player.direction;
        }

        #endregion

    }
}
