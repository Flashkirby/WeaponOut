using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;

namespace WeaponOut.Projectiles
{
    public class BuddyPortalExit : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Team Portal Exit");
            Main.projFrames[projectile.type] = 8;
        }
        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 68;
            projectile.tileCollide = false;
            projectile.timeLeft = 1230;
            projectile.netImportant = true;
        }
        private bool instanced = false;
        public override void AI()
        {
            if (!instanced)
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 15);
                    d.scale *= 2f;
                    d.velocity *= 0.5f;
                }
                instanced = true;
            }
            Lighting.AddLight(projectile.Center, 0.15f, 0.5f, 0.5f);

            if (Main.rand.Next(20) == 0)
            {
                Dust portal = Dust.NewDustDirect(projectile.position, 0, 0, 20, 0, 0);
                portal.position = projectile.Center + new Vector2(0, 12);
                portal.velocity.Y *= 1.5f;
                portal.velocity *= 0.5f;
            }

            projectile.frameCounter++;
            if (projectile.frameCounter > 5)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame >= Main.projFrames[projectile.type]) projectile.frame = 0;
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 15);
                d.scale *= 2f;
                d.velocity *= 0.5f;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            // Draw frame
            spriteBatch.Draw(
                texture, projectile.Center - Main.screenPosition, 
                new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight), lightColor,
                0f, new Vector2(texture.Width, frameHeight) / 2, 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
