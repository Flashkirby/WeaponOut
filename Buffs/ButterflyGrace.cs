using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class ButterflyGrace : ModBuff
    {
        public static readonly int POWERFACTOR = 200;

        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Butterfly's Grace");
            Description.SetDefault("Dance through the air!");
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            player.runAcceleration *= 10f;

            if (player.velocity.Y == 0)
            {
                player.buffTime[buffIndex] = 0;
            }
            else
            {
                if (player.controlJump && player.wingTime > 0f)
                {
                    player.velocity.Y -= player.gravDir * 2f;
                }
                else if(player.jump == 0 && player.controlDown)
                {
                    player.velocity.Y += player.gravity;
                }
            }
        }
    }
}
