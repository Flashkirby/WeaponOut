using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class ErodingWind : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Eroding Wind");
            Description.SetDefault("Losing life from harsh winds");
            Main.debuff[Type] = true;
        }
        int dustTimer = 0;
        public override void Update(Player player, ref int buffIndex)
        {
            float windSpeed = Main.windSpeed * 100f; // in mph
            if (player.FindBuffIndex(BuffID.WindPushed) >= 0)
            {
                windSpeed = Math.Max(windSpeed, 30f);
            }
            windSpeed = Math.Min(windSpeed, 80f); // Max -40 life per second

            if (player.lifeRegen > 0) player.lifeRegen = 0;
            player.lifeRegenTime = 0;
            player.lifeRegen -= (int)Math.Abs(windSpeed);

            dustTimer += (int)Math.Abs(windSpeed);
            if (dustTimer >= 120)
            {
                dustTimer -= 120;
                Dust.NewDust(player.position, player.width, player.height, DustID.Sandstorm,
                     Main.windSpeed * 15f, 0f, 0, Color.DarkKhaki);
            }
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            float windSpeed = Main.windSpeed * 100f; // in mph
            if (npc.HasValidTarget && Main.player[npc.target].FindBuffIndex(BuffID.WindPushed) >= 0)
            {
                windSpeed = Math.Max(windSpeed, 40f);
            }
            windSpeed = Math.Min(windSpeed, 160f); // Max -80 life per second

            if (npc.lifeRegen > 0) npc.lifeRegen = 0;
            npc.lifeRegen -= (int)Math.Abs(windSpeed);

            dustTimer += (int)Math.Abs(windSpeed);
            if (dustTimer >= 120)
            {
                dustTimer -= 120;
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Sandstorm,
                     Main.windSpeed * 15f, 0f, 0, Color.DarkKhaki);
            }
        }
    }
}
