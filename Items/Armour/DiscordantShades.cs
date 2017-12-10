using System.Collections.Generic;

using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    /// <summary>
    /// Intercepts hook controls for discord teleporting when free
    /// </summary>
    [AutoloadEquip(EquipType.Head)]
    public class DiscordantShades : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Discordant Shades");
            Tooltip.SetDefault(
                "Equippable as an accessory\n" +
                "$ROD\n" +
                "Uses the Rod of Discord instead of grappling\n" +
                "'The future's so bright, I gotta wear shades'");
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
