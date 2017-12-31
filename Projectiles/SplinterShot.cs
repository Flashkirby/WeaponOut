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
    /// splinters
    /// </summary>
    public class SplinterShot : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        private const float bulletFadeTime = 10;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Splinter Shot");
            DisplayName.AddTranslation(GameCulture.Chinese, "破片霰弹");
        }
        
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.alpha = 255;

            projectile.timeLeft = 600;
            projectile.penetrate = 1;
            projectile.extraUpdates = 1;

            projectile.friendly = true;
            projectile.ranged = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void AI()
        {
            //Main.NewText("ai0: \n" + projectile.ai[0] + " | ai1: \n" + projectile.ai[1]);
            Player player = Main.player[projectile.owner];
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);

            //duping lol
            if (projectile.ai[0] == bulletFadeTime && projectile.ai[1] == 0)
            {
                Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 54);
                projectile.damage = 2 * projectile.damage / 3; //nerf damage because 3 shot
                if (Main.myPlayer == projectile.owner) //spawn extra 2 copies
                {
                    //dMain.NewText("Dupe!", 100, 200, 255);
                    for (int i = 0; i < 2; i++)
                    {//make 3 in total
                        Projectile.NewProjectile(
                            projectile.position.X,
                            projectile.position.Y,
                            projectile.velocity.X + inaccuracy(),
                            projectile.velocity.Y + inaccuracy(),
                            projectile.type,
                            2 * projectile.damage / 3,
                            0.2f,
                            projectile.owner,
                            0,
                            1 // set this to 1 so we don't infinitely spam
                        );
                    }
                }
                projectile.ai[1] = 1f;
            }
            Lighting.AddLight(
                (int)((projectile.position.X + (float)(projectile.width / 2)) / 16f),
                (int)((projectile.position.Y + (float)(projectile.height / 2)) / 16f),
                0.5f,
                0.4f,
                0f
            );

            if (projectile.ai[0] < bulletFadeTime) projectile.ai[0]++;

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 10);
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(Main.projectileTexture[projectile.type],
                projectile.position - Main.screenPosition + new Vector2(projectile.width / 2f, projectile.height / 2f),
                new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)),
                Color.White * (projectile.ai[0] / bulletFadeTime),
                projectile.rotation,
                new Vector2(projectile.width / 2f, projectile.height / 2f),
                projectile.scale,
                projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
            return false;
        }

        private float inaccuracy()
        {
            return (float)Main.rand.Next(-524, 525) * 0.003f;
        }
    }
}
