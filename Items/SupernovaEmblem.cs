using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class SupernovaEmblem : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableAccessories;
        }

        public override void SetDefaults()
        {
            item.name = "Supernova Emblem";
            item.toolTip = @"Supercharges summon weapons to their lunar potential
Increases your max number of minions";
            item.toolTip2 = "'Call to arms'";
            item.width = 28;
            item.height = 28;
            item.rare = 10;
            item.value = Item.sellPrice(0, 25, 0, 0);
            item.accessory = true;
            item.expert = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShinyStone, 1);
            recipe.AddIngredient(ItemID.SummonerEmblem, 1);
            recipe.AddIngredient(ItemID.RainbowCrystalStaff, 1);
            recipe.AddIngredient(ItemID.MoonlordTurretStaff, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 4);
            player.maxMinions += 1;
            HeliosphereEmblem.DustVisuals(player, 135, 2f);
        }
    }
}
