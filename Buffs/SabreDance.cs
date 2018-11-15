using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class SabreDance: ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Saber Dance");

            Description.SetDefault("Forced autoswing with 200% increased melee attack speed");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.meleeSpeed += 2f;
            player.itemAnimation = Math.Min(player.itemAnimation, player.itemAnimationMax);
            if(player.itemAnimation <= 1)
            {
                player.HeldItem.useStyle = 1;
                player.controlUseItem = true;
            }
        }
    }
}
