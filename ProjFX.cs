using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut
{
    public class ProjFX : GlobalProjectile
    {
        public override void PostAI(Projectile projectile)
        {
            LunarAccessoryVisuals(projectile);
        }

        private void LunarAccessoryVisuals(Projectile projectile)
        {
            // Servers and server projectiles ignore this otherwise falling star bug
            if (Main.netMode == 2 || projectile.owner == 255) return;

            //Ignore npcs and statics
            if (projectile.npcProj || projectile.hostile) return;
            if (projectile.position == projectile.oldPosition) return;

            PlayerFX p = Main.player[projectile.owner].GetModPlayer<PlayerFX>(mod);
            if (p.lunarMagicVisual && projectile.magic)
            {
                Dust d = Main.dust[Dust.NewDust(
                    projectile.position, projectile.width, projectile.height,
                    DustID.PinkFlame,
                    projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f,
                    0, default(Color), 0.2f
                    )];
                d.fadeIn = 1f;
                d.velocity = d.velocity * 0.7f;
                d.noGravity = true;

                if (Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
                { d.noLight = true; }
            }
            if (p.lunarRangeVisual && projectile.ranged)
            {
                Dust d = Main.dust[Dust.NewDust(
                    projectile.Center - new Vector2(4, 4), 0, 0,
                    DustID.Vortex
                    )];
                d.scale = (float)Math.Log10(projectile.width + projectile.height) * 0.7f;
                d.velocity = d.velocity * 0.1f + projectile.velocity / 2;
                d.noGravity = true;

                if (Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
                { d.noLight = true; }
            }
            if (p.lunarThrowVisual && projectile.thrown)
            {
                if (projectile.whoAmI < 200) // Do not use Beenades.
                {
                    float speed = (10f + projectile.velocity.Length()) / 11;
                    Vector2 trailLeft = new Vector2(
                        projectile.velocity.X * 0.2f,
                        projectile.velocity.Y * 0.2f).
                        RotatedBy(1);
                    Vector2 trailRght = new Vector2(
                        projectile.velocity.X * 0.2f,
                        projectile.velocity.Y * 0.2f).
                        RotatedBy(-1);

                    bool solid = Collision.SolidCollision(projectile.position, projectile.width, projectile.height);

                    Vector2 pos = projectile.Center - new Vector2(4, 4) + projectile.velocity * 2;
                    Dust d = Main.dust[Dust.NewDust(
                        pos, 0, 0,
                        75,
                        0, 0, 0, default(Color), projectile.scale * speed
                        )];
                    d.velocity = trailLeft;
                    d.noGravity = true;
                    if (solid) { d.noLight = true; }
                    d = Main.dust[Dust.NewDust(
                        pos, 0, 0,
                        75,
                        0, 0, 0, default(Color), projectile.scale * speed
                        )];
                    d.velocity = trailRght;
                    d.noGravity = true;
                    if (solid) { d.noLight = true; }
                }
                else
                {
                    Dust d = Main.dust[Dust.NewDust(
                        projectile.position, projectile.width, projectile.height,
                        75,
                        0, 0, 0, default(Color), 2f
                        )];
                    d.velocity = projectile.velocity * 0.6f;
                    d.noGravity = true;
                }
            }
        }
    }
}
