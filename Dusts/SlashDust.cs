using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Dusts
{
    public class SlashDust : ModDust
    {

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.rotation += Main.rand.NextFloat() * 6.282f;
			dust.scale *= 16;
            dust.velocity *= 0.1f;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.5f;

            if (dust.scale < 0.06f)
            {
                dust.active = false;
            }
            else
            {
                dust.scale /= 1.5f;
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(
                128 + lightColor.R / 2,
                128 + lightColor.G / 2,
                128 + lightColor.B / 2,
                100);
        }
    }
}
