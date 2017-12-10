using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class PerihelionEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Perihelion Emblem");
            Tooltip.SetDefault(
                "Supercharges throwing weapons to their lunar potential\n" +
                "30% increased throwing velocity\n" +
                "'Swing back around'");
        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.rare = 10;
            item.value = Item.sellPrice(0, 15, 0, 0);
            item.accessory = true;
            item.expert = true;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableEmblems) return;
            Mod thorium = ModLoader.GetMod("ThoriumMod");
            if (thorium != null)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.ShinyStone, 1);
                recipe.AddIngredient(thorium.GetItem("NinjaEmblem"), 1);
                recipe.AddIngredient(thorium.GetItem("WhiteDwarfKunai"), 450);
                recipe.AddIngredient(thorium.GetItem("BlackDagger"), 1);
                recipe.AddTile(TileID.LunarCraftingStation);
                recipe.SetResult(this, 1);
                recipe.AddRecipe();
            }
            else
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.ShinyStone, 1);
                recipe.AddIngredient(ItemID.AvengerEmblem, 1);
                recipe.AddIngredient(ItemID.DayBreak, 1);
                recipe.AddIngredient(ItemID.MagicDagger, 1);
                recipe.AddTile(TileID.LunarCraftingStation);
                recipe.SetResult(this, 1);
                recipe.AddRecipe();
            }
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 2);
            player.thrownVelocity += 0.3f;
            if (hideVisual) return;
            HeliosphereEmblem.DustVisuals(player, 75, 1.5f);
            player.GetModPlayer<PlayerFX>(mod).lunarThrowVisual = true;
        }
    }
}
