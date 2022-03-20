using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class PerihelionEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Perihelion Emblem");
            DisplayName.AddTranslation(GameCulture.Chinese, "近日点徽章");
			DisplayName.AddTranslation(GameCulture.Russian, "Эмблема Перигелия");

            Tooltip.SetDefault(
                "Supercharges throwing weapons to their lunar potential\n" +
                "30% increased throwing velocity\n" +
                "'Swing back around'");
            Tooltip.AddTranslation(GameCulture.Chinese, "激发投掷武器的月之潜力\n增加30%投掷速度\n“来回摆动”");
            Tooltip.AddTranslation(GameCulture.Russian,
				"Заряжает метательое оружие космической энергией\n" +
                "+30% скорость метания\n" +
                "'Закинь подальше'");

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
            ModRecipe recipe;
            if (thorium != null)
            {
                recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.ShinyStone, 1);
                recipe.AddIngredient(thorium.GetItem("NinjaEmblem"), 1);
                recipe.AddIngredient(thorium.GetItem("AngelsEnd"), 1);
                recipe.AddIngredient(thorium.GetItem("StarEater"), 1);
                recipe.AddTile(TileID.LunarCraftingStation);
                recipe.SetResult(this, 1);
                recipe.AddRecipe();
            }
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShinyStone, 1);
            recipe.AddIngredient(ItemID.AvengerEmblem, 1);
            recipe.AddIngredient(ItemID.DayBreak, 1);
            recipe.AddIngredient(ItemID.MagicDagger, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 2);
            player.thrownVelocity += 0.3f;
            if (hideVisual) return;
            HeliosphereEmblem.DustVisuals(player, 75, 1.5f);
            player.GetModPlayer<PlayerFX>().lunarThrowVisual = true;
        }
    }
}
