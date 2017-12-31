using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class ButterflyGrace : ModBuff
    {
        public static readonly int POWERFACTOR = 200;

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Butterfly's Grace");
            DisplayName.AddTranslation(GameCulture.Chinese, "蝶之恩赐");

            Description.SetDefault("Dance through the air!");
            Description.AddTranslation(GameCulture.Chinese, "让你在空中舞蹈！");
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
