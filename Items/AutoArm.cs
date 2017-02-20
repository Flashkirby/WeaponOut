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
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetDefaults()
        {
            item.name = "Voodoo Arm";
            item.toolTip = "Enables auto-swing for weapons while in inventory";
            item.toolTip2 = "'It twitches occasionally'";
            item.width = 28;
            item.height = 28;
            item.rare = 1;
            item.value = Item.sellPrice(0, 0, 5, 0);
        }
        public override void AddRecipes()
        {
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
