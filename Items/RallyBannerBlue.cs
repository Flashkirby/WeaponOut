using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    /// <summary>
    /// Provides AoE banner bonus to players "nearby".
    /// Buff DPS pays itself back in a 7-8 player team,
    /// </summary>
    public class RallyBannerBlue : ModItem
    {
        public override void SetStaticDefaults()
        {
            RallyBannerRed.SetStaticDefaults(this, "Blue Rally Banner");
			Tooltip.AddTranslation(GameCulture.Russian, "Синий Боевой Флаг");
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
