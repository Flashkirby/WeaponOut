using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class SupernovaEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Supernova Emblem");
            DisplayName.AddTranslation(GameCulture.Chinese, "超新星徽章");

            Tooltip.SetDefault(
                "Supercharges summon weapons to their lunar potential\n" +
                "Increases your max number of minions\n" +
                "'Call to arms'");
            Tooltip.AddTranslation(GameCulture.Chinese, "激发召唤武器的月之潜力\n增加你的召唤物上限\n“战争号令”");
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
            if (hideVisual) return;
            HeliosphereEmblem.DustVisuals(player, 135, 2f);
        }
    }
}
