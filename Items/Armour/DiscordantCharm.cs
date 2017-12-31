using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    /// <summary>
    /// Intercepts hook controls for discord teleporting when free
    /// </summary>
    [AutoloadEquip(EquipType.Head)]
    public class DiscordantCharm : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Discordant Charm");
            DisplayName.AddTranslation(GameCulture.Chinese, "混沌咒文");

            Tooltip.SetDefault(
                "Equippable as an accessory\n" +
                "$ROD\n" +
                "Uses the Rod of Discord instead of grappling\n");
            Tooltip.AddTranslation(GameCulture.Chinese, "可以作为饰品装备\n$ROD\n物品栏里有混沌法杖时可以按住钩爪键（键盘的E键）进行传送");

            ModTranslation text = mod.CreateTranslation("DiscordantRodTooltip");
            text.SetDefault("Will not work without the Rod of Discord!");
            text.AddTranslation(GameCulture.Chinese, "没有传送法杖将无法使用！");
            mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
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

        internal static void CheckRODTooltip(List<TooltipLine> tooltips)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].text.Contains("$ROD"))
                {
                    foreach (Item item in Main.LocalPlayer.inventory)
                    {
                        if (item.type == ItemID.RodofDiscord)
                        {
                            tooltips.RemoveAt(i);
                            return;
                        }
                    }
                    tooltips[i].text = tooltips[i].text.Replace("$ROD", WeaponOut.GetTranslationTextValue("DiscordantRodTooltip"));
                    return;
                }
            }
        }
    }
}
