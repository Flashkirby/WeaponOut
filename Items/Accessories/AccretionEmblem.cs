using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class AccretionEmblem : ModItem
    {
        public override void SetStaticDefaults() 
        {
            DisplayName.SetDefault("Accretion Emblem");
            DisplayName.AddTranslation(GameCulture.Chinese, "吸积徽章");
            DisplayName.AddTranslation(GameCulture.Russian, "Эмблема Туманности");

            Tooltip.SetDefault(
                "Supercharges magic weapons to their lunar potential\n" +
                "Increases maximum mana by 20\n" +
                "'Mind over matter'");
            Tooltip.AddTranslation(GameCulture.Chinese, "激发魔法武器的月之潜力\n最大魔力值增加20\n“心胜于物”");
            Tooltip.AddTranslation(GameCulture.Russian,
            "Заряжает магическое оружие космической энергией\n" +
            "+20 максимум маны\n" +
            "'Сила духа'");
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
