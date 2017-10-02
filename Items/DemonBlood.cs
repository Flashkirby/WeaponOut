using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class DemonBlood : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Blood Pact");
            Tooltip.SetDefault(
                "Permanently grants a new form of healing");
        }
        public override void SetDefaults()
        {
            item.maxStack = 99;
            item.consumable = true;
            item.width = 18;
            item.height = 18;
            item.useStyle = 4;
            item.useTime = 30;
            item.UseSound = SoundID.Item4;
            item.useAnimation = 30;
            item.rare = 4;
            item.expert = true;
            item.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.GetModPlayer<PlayerFX>().demonBlood && Main.expertMode;
        }
        public override bool UseItem(Player player)
        {
            player.GetModPlayer<PlayerFX>().demonBlood = true;
            return true;
        }
    }
}
