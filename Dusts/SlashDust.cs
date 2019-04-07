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
            dust.scale *= 3f;
            dust.velocity *= 0.25f;

        }

        public override bool Update(Dust dust)
        {
            if(dust.firstFrame)
            {
                dust.rotation = (float)System.Math.Atan2(dust.velocity.Y, dust.velocity.X) + MathHelper.PiOver2;
                dust.frame = new Rectangle(0, 0, 4, 28);
            }
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;

            if (dust.scale < 0.5f || dust.alpha >= 255)
            {
                dust.active = false;
            }
            else
            {
                dust.scale -= 0.5f;
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            if (!dust.noLight) { return lightColor; }
            int scale = (int)(20 * dust.scale);
            return new Color(
                128 + lightColor.R / 2 + scale,
                128 + lightColor.G / 2 + scale,
                128 + lightColor.B / 2 + scale,
                100 + scale);
        }
    }
}
