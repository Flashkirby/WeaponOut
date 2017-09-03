using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class Momentum : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Momentum");
            Description.SetDefault("Smash through enemies and projectiles");
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            player.buffTime[buffIndex] = time;
            return true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PlayerFX>().momentumActive = true;
            if (player.buffTime[buffIndex] == 1) player.buffTime[buffIndex]++;
            
            float i = Main.rand.NextFloatDirection();
            Dust d = Dust.NewDustPerfect(new Vector2(
                player.Center.X + player.width * 0.6f * player.direction,
                player.Center.Y + player.height * 0.6f * i),
                31,
                new Vector2(0, i * Math.Abs(player.velocity.X) * 0.3f), 100);
        }

    }
}
