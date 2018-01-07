using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System;

namespace WeaponOut.Projectiles
{
    public class PsyWave : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psychic Wave");
            DisplayName.AddTranslation(GameCulture.Chinese, "精神波动");
			DisplayName.AddTranslation(GameCulture.Russian, "Пси-Волна");
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;

            projectile.penetrate = -1;
            projectile.alpha = 50;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if((player.itemAnimation == 0 && projectile.ai[1] == 0f)
                || projectile.ai[1] == 1f
                || projectile.ai[0] > 480)
            {
                // Break effect
                projectile.ai[1] = 1f;
                projectile.netUpdate = true;
            }

            if (projectile.ai[1] > 0f)
            { 
                //disappearing
                projectile.friendly = false;
                projectile.tileCollide = false;
                projectile.alpha += 3;
                projectile.ai[1]++;
                if (projectile.ai[1] > 66)
                {
                    projectile.timeLeft = 0;
                }
            }else
            {
                projectile.position += player.position - player.oldPosition;
            }

            projectile.direction = 1;
            if (projectile.velocity.X < 0) projectile.direction = -1;
            projectile.ai[0]++;

            // Set frame and direction
            projectile.spriteDirection = projectile.direction;
            projectile.frameCounter++;
            if(projectile.frameCounter > 4)
            {
                projectile.frame++;
                if(projectile.frame >= Main.projFrames[projectile.type])
                {
                    projectile.frame = 0;
                }
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = projectile.width / 2;
            height = projectile.height / 2;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.ai[1] < 1f) projectile.ai[1] = 1;
            projectile.velocity = projectile.oldVelocity;
            projectile.tileCollide = false;
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {

            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameCount = Main.projFrames[projectile.type];

            int frameHeight = texture.Height / frameCount;

            // Flip Horziontally
            SpriteEffects spriteEffect = SpriteEffects.None;
            spriteEffect = SpriteEffects.None;
            if (projectile.spriteDirection < 0)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
            }

            

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight),
                new Color(projectile.Opacity, projectile.Opacity, projectile.Opacity, projectile.Opacity),
                0f,
                new Vector2(texture.Width / 2, frameHeight / 2),
                projectile.scale,
                spriteEffect,
                0f);
            return false;
        }
    }
}
