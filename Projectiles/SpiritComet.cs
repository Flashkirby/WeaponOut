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
    public class SpiritComet : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spirit Meteor");
            DisplayName.AddTranslation(GameCulture.Chinese, "精神陨石");
        }
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;

            projectile.MaxUpdates = 2;

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
                    Main.PlaySound(SoundID.Item88, projectile.Center);
                    projectile.velocity /= 2; // Half speed
                }
                else
                {
                    projectile.velocity *= 1.1f; // Speed up to original (roughly)
                }
                projectile.alpha = Math.Max(0, 200 - (int)projectile.ai[0] * 20);
            }

            #region Meteor Dust Code
            Vector2 ahead = projectile.Center + Vector2.Normalize( projectile.velocity) * 10f;
            Dust dust37 = Main.dust[Dust.NewDust( projectile.position,  projectile.width,  projectile.height, 6, 0f, 0f, 0, default(Color), 1f)];
            dust37.position = ahead;
            dust37.velocity =  projectile.velocity.RotatedBy(1.5707963705062866, default(Vector2)) * 0.33f +  projectile.velocity / 4f;
            Dust dust3 = dust37;
            dust3.position +=  projectile.velocity.RotatedBy(1.5707963705062866, default(Vector2));
            dust37.fadeIn = 0.5f;
            dust37.noGravity = true;
            dust37 = Main.dust[Dust.NewDust( projectile.position,  projectile.width,  projectile.height, 6, 0f, 0f, 0, default(Color), 1f)];
            dust37.position = ahead;
            dust37.velocity =  projectile.velocity.RotatedBy(-1.5707963705062866, default(Vector2)) * 0.33f +  projectile.velocity / 4f;
            dust3 = dust37;
            dust3.position +=  projectile.velocity.RotatedBy(-1.5707963705062866, default(Vector2));
            dust37.fadeIn = 0.5f;
            dust37.noGravity = true;
            for (int num190 = 0; num190 < 1; num190++)
            {
                int num191 = Dust.NewDust(new Vector2( projectile.position.X,  projectile.position.Y),  projectile.width,  projectile.height, 6, 0f, 0f, 0, default(Color), 1f);
                dust3 = Main.dust[num191];
                dust3.velocity *= 0.5f;
                dust3 = Main.dust[num191];
                dust3.scale *= 1.3f;
                Main.dust[num191].fadeIn = 1f;
                Main.dust[num191].noGravity = true;
            }
            #endregion

            projectile.rotation += projectile.direction / 2f;
            projectile.ai[0]++;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item89, projectile.Center);

            #region Meteor Dust Code
            int num3;
            for (int num337 = 0; num337 < 8; num337 = num3 + 1)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                num3 = num337;
            }
            for (int num338 = 0; num338 < 32; num338 = num3 + 1)
            {
                int num339 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[num339].noGravity = true;
                Dust dust = Main.dust[num339];
                dust.velocity *= 3f;
                num339 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                dust = Main.dust[num339];
                dust.velocity *= 2f;
                Main.dust[num339].noGravity = true;
                num3 = num338;
            }
            for (int num340 = 0; num340 < 2; num340 = num3 + 1)
            {
                int num341 = Gore.NewGore(projectile.position + new Vector2((float)(projectile.width * Main.rand.Next(100)) / 100f, (float)(projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64), 1f);
                Gore gore = Main.gore[num341];
                gore.velocity *= 0.3f;
                Gore gore43 = Main.gore[num341];
                gore43.velocity.X = gore43.velocity.X + (float)Main.rand.Next(-10, 11) * 0.05f;
                Gore gore44 = Main.gore[num341];
                gore44.velocity.Y = gore44.velocity.Y + (float)Main.rand.Next(-10, 11) * 0.05f;
                num3 = num340;
            }
            #endregion
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

            // Draw with trail
            for (int i = 2; i >= 0; i--)
            {
                spriteBatch.Draw(t,
                    p - (projectile.velocity * 3 * i) + c,
                    source, new Color(
                        255 - (projectile.alpha + 80 * i), 255 - (projectile.alpha + 80 * i),
                        255 - (projectile.alpha + 80 * i), 255 - (projectile.alpha + 80 * i)),
                    projectile.rotation,
                    new Vector2(t.Width / 2, height / 2),
                    3f * projectile.scale / (i + 3),
                    SpriteEffects.None,
                    0f);
            }
            #endregion
            return false;
        }
    }
}
