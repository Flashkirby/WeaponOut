using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items
{
    /// <summary>
    /// Provides AoE banner bonus to players "nearby".
    /// Buff DPS pays itself back in a 7-8 player team,
    /// </summary>
    public class RallyBannerGreen : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Green Rally Banner");
            DisplayName.AddTranslation(GameCulture.Chinese, "振军旗帜");
            DisplayName.AddTranslation(GameCulture.Russian, "Зелёный Боевой Флаг");
            RallyBannerRed.SetStaticToolTipDefaults(this);
        }
        public override void SetDefaults()
        {
            RallyBannerRed.SetDefaults(item);
        }
        public override void AddRecipes()
        {
            RallyBannerRed.AddRecipe(this);
        }

        public override void HoldStyle(Player player)
        {
            RallyBannerRed.HoldStyle(item, player);
        }
    }
}
