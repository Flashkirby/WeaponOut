using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Goes in either direction based on ai[1]
    /// </summary>
    public class Reverb : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public const float explosionSize = 3f;
        public const float explosionTime = 5f;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reverb");
            DisplayName.AddTranslation(GameCulture.Chinese, "回响光杖");
            Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            
            projectile.penetrate = -1;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.ignoreWater = true;
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

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width /= 2;
            height /= 2;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            #region lighting free multi-frame alpha render
            Texture2D t = Main.projectileTexture[projectile.type];
            int height = t.Height;
            Rectangle? source = null;
            if (Main.projFrames[projectile.type] > 1)
            {
                height = t.Height / Main.projFrames[projectile.type]; ;
                source = new Rectangle(0, height * projectile.frame, t.Width, height);
            }
            Vector2 p = projectile.position - Main.screenPosition;
            Vector2 c = new Vector2(projectile.width / 2, projectile.height / 2);
            spriteBatch.Draw(t,
                p + c,
                source, new Color(
                    255 - projectile.alpha, 255 - projectile.alpha,
                    255 - projectile.alpha, 255 - projectile.alpha),
                projectile.rotation,
                new Vector2(t.Width / 2, height / 2),
                projectile.scale,
                SpriteEffects.None,
                0f);
            #endregion
            return false;
        }
    }
}
