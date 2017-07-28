using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class AccretionEmblem : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableEmblems;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Accretion Emblem");
            Tooltip.SetDefault(
                "Supercharges magic weapons to their lunar potential\n" +
                "Increases maximum mana by 20\n" +
                "'Mind over matter'");
        }
        public override void SetDefaults()
        {
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
