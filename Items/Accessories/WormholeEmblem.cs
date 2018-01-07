using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class WormholeEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wormhole Emblem");
			DisplayName.AddTranslation(GameCulture.Russian, "Эмблема Червоточины");
            Tooltip.SetDefault(
                "Supercharges ranged weapons to their lunar potential\n" +
                "20% chance to not consume ammo\n" +
                "'Blast with the past'");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Заряжает оружие дальнего боя космической энергией\n" +
                "20% шанс не использовать боеприпасы\n" +
                "'Взрыв с прошлым'");
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
        public override void AddRecipes() {
            if (!ModConf.EnableEmblems) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShinyStone, 1);
            recipe.AddIngredient(ItemID.RangerEmblem, 1);
            recipe.AddIngredient(ItemID.SDMG, 1);
            recipe.AddIngredient(ItemID.FireworksLauncher, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 1);
            player.ammoCost80 = true;
            if (hideVisual) return;
            HeliosphereEmblem.DustVisuals(player, DustID.Vortex);
            player.GetModPlayer<PlayerFX>(mod).lunarRangeVisual = true;
        }
    }
}
