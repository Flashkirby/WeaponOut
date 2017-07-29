using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    /// <summary>
    /// Provides AoE banner bonus to players "nearby".
    /// Buff DPS pays itself back in a 7-8 player team,
    /// </summary>
    public class RallyBannerRed : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetStaticDefaults()
        {
            RallyBannerRed.SetStaticDefaults(this, "Red Rally Banner");
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

        #region Static Methods

        public static void SetStaticDefaults(ModItem modItem, string name)
        {
            modItem.DisplayName.SetDefault(name);
            modItem.Tooltip.SetDefault(
                "Increases stats for your team whilst held\n" +
                "Team is not affected by banner color");
        }
        public static void SetDefaults(Item item)
        {
            modItem.item.CloneDefaults(ItemID.Umbrella);
            item.width = 28;
            item.height = 48;
            item.value = 3000;
        }
        public static void AddRecipe(ModItem modItem)
        {
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
