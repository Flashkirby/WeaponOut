using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Goes in either direction based on ai[1]
    /// </summary>
    public class Reverb : ModProjectile
    {
        public const float explosionSize = 3f;
        public const float explosionTime = 5f;
        public override void SetDefaults()
        {
            projectile.name = "Reverb";
            projectile.width = 32;
            projectile.height = 32;
            Main.projFrames[projectile.type] = 2;
            
            projectile.penetrate = -1;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.ignoreWater = true;

            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
        }

        public override void AI()
        {
            if (projectile.ai[0] < 20)
            {
                if (projectile.ai[1] < 0) // Blue Wave
                {
                    projectile.frame = 1;
                    int d = Dust.NewDust(projectile.position, projectile.width, projectile.height,
                        88, projectile.velocity.X, projectile.velocity.Y, 200);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.4f;
                }
                else
                {
                    projectile.frame = 0; // Red Wave
                    int d = Dust.NewDust(projectile.position, projectile.width, projectile.height,
                        90, projectile.velocity.X, projectile.velocity.Y, 200);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.4f;
                }
            }

            if(projectile.ai[0] >= 20)
            {
                projectile.hide = true;

                if(projectile.ai[0] == 20)
                {
                    projectile.netUpdate = true;

                    int dustType = 90;
                    if (projectile.ai[1] < 0) // Blue Wave
                    {
                        dustType = 88;
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        int d = Dust.NewDust(projectile.position, projectile.width, projectile.height,
                            dustType, 0, 0, 200);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= explosionSize;
                    }

                    Vector2 centre = projectile.Center;
                    projectile.width = (int)(projectile.width * explosionSize);
                    projectile.height = (int)(projectile.height * explosionSize);
                    projectile.Center = centre;

                    Main.PlaySound(2, centre, 25);
                }

                //stop
                projectile.velocity = Vector2.Zero;

                if (projectile.ai[0] >= 20 + explosionTime)
                {
                    projectile.timeLeft = 0;
                }
            }

            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            projectile.ai[0]++;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.ai[0] < 20)
            {
                projectile.ai[0] = 20;
                projectile.position -= projectile.velocity;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.ai[0] < 20)
            {
                projectile.ai[0] = 20;
                projectile.position -= projectile.velocity;
            }
            return false;
        }

        public override void TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width /= 2;
            height /= 2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.White;
            return true;
        }
    }
}
