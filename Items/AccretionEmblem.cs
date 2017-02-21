using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class AccretionEmblem : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableEmblems;
        }

        public override void SetDefaults()
        {
            item.name = "Accretion Emblem";
            item.toolTip = @"Supercharges magic weapons to their lunar potential
Increases maximum mana by 20";
            item.toolTip2 = "'Mind over matter'";
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
            recipe.AddIngredient(ItemID.SorcererEmblem, 1);
            recipe.AddIngredient(ItemID.LastPrism, 1);
            recipe.AddIngredient(ItemID.LunarFlareBook, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 3);
            player.statManaMax2 += 20;
            if (hideVisual) return;
            HeliosphereEmblem.DustVisuals(player, DustID.PinkFlame, 2f);
            player.GetModPlayer<PlayerFX>(mod).lunarMagicVisual = true;
        }
    }
}
