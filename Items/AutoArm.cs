using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class AutoArm : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Voodoo Arm");
            Tooltip.SetDefault(
                "Enables auto-swing when favorited in inventory\n" + 
                "'It twitches occasionally'");
        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.rare = 1;
            item.value = Item.sellPrice(0, 0, 5, 0);
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableBasicContent) return;
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.ZombieArm, 1);
                if (i == 0)
                { recipe.AddIngredient(ItemID.CopperWatch, 1); }
                else
                { recipe.AddIngredient(ItemID.TinWatch, 1); }
                recipe.AddTile(TileID.WorkBenches);
                recipe.SetResult(this, 1);
                recipe.AddRecipe();
            }
        }
        public override void UpdateInventory(Player player)
        {
            if (!item.favorited) return;
            if (!player.HeldItem.autoReuse && !player.HeldItem.channel)
            {
                if (player.itemAnimation == 0)
                {
                    player.releaseUseItem = true;
                }
            }
        }
    }
}
