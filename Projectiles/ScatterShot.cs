using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System;

namespace WeaponOut.Projectiles
{
    public class ScatterShot : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public const float framesPerBullet = 4f;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shatter Shard");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔晶碎片");
			DisplayName.AddTranslation(GameCulture.Russian, "Осколок");
        }
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.timeLeft = 600;
            projectile.penetrate = 1;
            projectile.alpha = 50;
            projectile.MaxUpdates = 3;

            projectile.friendly = true; //no damage on its own
            projectile.ranged = true;
        }

        public override void AI()
        {
            if(projectile.ai[0] >= framesPerBullet * 5)
            {
                projectile.timeLeft = projectile.numUpdates;
            }
            if(projectile.ai[0] > framesPerBullet * 3)
            {
                projectile.scale -= 1f / (framesPerBullet * 4);
            }

            if (projectile.ai[0] % framesPerBullet == 0)
            {
                Projectile.NewProjectile(projectile.Center,
                    new Vector2(
                        projectile.velocity.X * 0.5f + inaccuracy() * (projectile.ai[0] + 0.1f) / (3f * 5),
                        projectile.velocity.Y * 0.5f + inaccuracy() * (projectile.ai[0] + 0.1f) / (3f * 5)),
                    ProjectileID.CrystalShard,
                    (int)(projectile.damage * 0.8f), projectile.knockBack, projectile.owner, 0f, 0f);
                // Kills on spazmatism vs Crystal bullet 40/60 seconds
                // x1 = 15 seconds
                // x0.8 = 30 seconds
                // x0.5 = 60 seconds
            }

            Lighting.AddLight(projectile.Center,
                0.1f,
                0.025f,
                0.5f);

            projectile.rotation = Main.rand.NextFloatDirection() * (float)Math.PI;
            projectile.ai[0]++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 10);
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 centre = new Vector2(
                texture.Width / 2f,
                texture.Height / 2f);

            spriteBatch.Draw(texture,
                projectile.Center - Main.screenPosition,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                Color.White,
                projectile.rotation,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );

            //draw previous set
            for (int i = 1; i < projectile.MaxUpdates; i++)
            {
                spriteBatch.Draw(texture,
                    projectile.Center - Main.screenPosition - projectile.velocity * i,
                    new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                    Color.White,
                    (projectile.velocity.X + projectile.velocity.Y) * i * MathHelper.Pi,
                    centre,
                    projectile.scale,
                    SpriteEffects.None,
                    0
                );
            }
            return false;
        }

        private float inaccuracy()
        {
            return (float)Main.rand.Next(-1024, 1023) * 0.005f;
        }
    }
}
