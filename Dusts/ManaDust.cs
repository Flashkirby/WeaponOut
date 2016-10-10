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
            dust.scale = 1f;
            dust.rotation += Main.rand.NextFloat() * 6.282f;
            dust.velocity *= 0.1f;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.85f;
            dust.scale -= 0.04f;
            if (dust.scale < 0.2f)
            {
                dust.active = false;
            }
            else
            {
                if (dust.velocity.X > 0)
                {
                    dust.rotation += 0.1f / dust.scale;
                }
                else
                {
                    dust.rotation -= 0.1f / dust.scale;
                }
                float strength = dust.scale / 3f;
                Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 
                    dust.color.R / 255f * 0.6f * strength, 
                    dust.color.G / 255f * 0.4f * strength, 
                    dust.color.B / 255f * 1.1f * strength);
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(
                128 + lightColor.R / 2,
                128 + lightColor.G / 2,
                128 + lightColor.B / 2,
                100 + (Main.rand.Next(3) == 0 ? 50 : 0));
        }
    }
}
