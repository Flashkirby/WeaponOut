using System.Collections.Generic;

using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    /// <summary>
    /// Intercepts hook controls for discord teleporting when free. Copy of Discordant Charm, but visually different
    /// </summary>
    [AutoloadEquip(EquipType.Head)]
    public class DiscordantShades : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Discordant Shades");
			DisplayName.AddTranslation(GameCulture.Russian, "Очки Раздора");
            Tooltip.SetDefault(
                "Equippable as an accessory\n" +
                "$ROD\n" +
                "Uses the Rod of Discord instead of grappling\n" +
                "'The future's so bright, I gotta wear shades'");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Можно надеть как аксессуар\n" +
                "$ROD\n" +
                "Использует Жезл Раздора вместо крюка\n" +
                "'Будущее такое светлое, надо надеть очки'");
        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 12;
            item.rare = 4;
            item.accessory = true;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableAccessories) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofLight, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            DiscordantCharm.CheckRODTooltip(tooltips);
        }
    }
}
