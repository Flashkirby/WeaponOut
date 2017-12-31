using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class Flurry : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Flurry");
            DisplayName.AddTranslation(GameCulture.Chinese, "爆发");

            Description.SetDefault("Forced autoswing with 100% increased melee attack speed");
            Description.AddTranslation(GameCulture.Chinese, "强制连点且增加100%的近战攻速");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.meleeSpeed += 1f;
            player.immuneTime++;

            // From lunar emblems
            if (!player.HeldItem.autoReuse && !player.HeldItem.channel)
            {
                if (player.itemAnimation == 0)
                {
                    player.releaseUseItem = true;
                }
            }
            else
            {
                player.controlUseItem = true;
            }
        }
    }
}
