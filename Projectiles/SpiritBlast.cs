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
    public class SpiritBlast : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.name = "Spirit Blast";
            projectile.width = 24;
            projectile.height = 24;
            Main.projFrames[projectile.type] = 3;
            
            projectile.penetrate = 1;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (projectile.ai[0] < 10)
            {
                if (projectile.ai[0] == 0)
                {
                    Main.PlaySound(SoundID.Item20, projectile.Center);
                    projectile.velocity /= 2; // Half speed
                }
                else
                {
                    projectile.velocity *= 1.1f; // Speed up to original (roughly)
                }
                projectile.alpha = Math.Max(0, 200 - (int)projectile.ai[0] * 20);
            }

            Dust d = Main.dust[Dust.NewDust(projectile.Center - new Vector2(projectile.width / 4 + 2, projectile.height / 4 + 2), 
                projectile.width / 2, projectile.height / 2,
                 226, 0, 0, 100, default(Color), Main.rand.NextFloat() + 0.5f)];
            d.noGravity = true;
            d.noLight = true;
            d.velocity = projectile.velocity / 2;

            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            projectile.ai[0]++;
            projectile.frameCounter++;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height,
                     226, 0, 0, 100, default(Color), Main.rand.NextFloat() + 1f)];
                d.noGravity = true;
                d.velocity *= 2;
            }
            Main.PlaySound(SoundID.Item118, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if(projectile.frameCounter > 10)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame >= Main.projFrames[projectile.type]) projectile.frame = 0;
            }

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
