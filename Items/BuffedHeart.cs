using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class BuffedHeart : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            if (player.GetModPlayer<PlayerFX>().heartBuff)
            {
                if (item.type == ItemID.Heart ||
                    item.type == ItemID.CandyApple ||
                    item.type == ItemID.SugarPlum
                    )
                {
                    Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y, 1, 1f, 0f);
                    player.statLife += 25;
                    if (Main.myPlayer == player.whoAmI)
                    {
                        player.HealEffect(25, true);
                    }
                    if (player.statLife > player.statLifeMax2)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                    return false;
                }

            }
            return base.OnPickup(item, player);
        }
    }
}
