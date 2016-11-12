using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Dusts
{
    public class ManaDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.rotation += Main.rand.NextFloat() * 6.282f;
            dust.velocity *= 0.1f;
        }

        public override bool Update(Dust dust)
        {
            if (dust.noLight)
            {
                if (Main.rand.Next(4) == 0)
                {
                    dust.noLight = false;
                    dust.scale += 0.3f;
                }
            }
            else
            {
                if (Main.rand.Next(10) == 0)
                {
                    dust.scale -= 0.3f;
                    dust.noLight = true;
                }
            }

            dust.position += dust.velocity;
            dust.velocity *= 0.85f;

            if (dust.scale < 0.05f && !dust.noLight)
            {
                dust.active = false;
            }
            else
            {
                dust.scale -= 0.04f;
                if (dust.velocity.X > 0)
                {
                    dust.rotation += 0.1f / dust.scale;
                }
                else
                {
                    dust.rotation -= 0.1f / dust.scale;
                }
                float strength = dust.scale * 0.5f;
                Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 
                    0.3f * strength, 
                    0.2f * strength, 
                    0.55f * strength);
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
