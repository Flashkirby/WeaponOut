using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// splinters
    /// </summary>
    public class SpiritFlameBlast : ModProjectile
    {
        private const float bulletFadeTime = 10;

        public override void SetDefaults()
        {
            projectile.name = "Furious Flames";
            projectile.width = 8;
            projectile.height = 8;
            projectile.alpha = 255;

            projectile.timeLeft = 15;
            projectile.penetrate = 1;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            //rotate in direction
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
            //follow player
            projectile.position += player.position - player.oldPosition;

            //core dust effects
            int d;
            for (int i = 0; i < 5; i++)
            {
                d = Dust.NewDust(projectile.position, projectile.width, projectile.height, 174,
                    projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
                Main.dust[d].noGravity = true;
            }

            //side fire
            d = Dust.NewDust(projectile.position, projectile.width, projectile.height, 127,
                projectile.velocity.Y * 0.8f, -projectile.velocity.X * 0.8f, 0, Color.White,
                1.2f);
            Main.dust[d].noGravity = true;
            Main.dust[d].noLight = true;
            d = Dust.NewDust(projectile.position, projectile.width, projectile.height, 127,
                -projectile.velocity.Y * 0.8f, projectile.velocity.X * 0.8f, 0, Color.White,
                1.2f);
            Main.dust[d].noGravity = true;
            Main.dust[d].noLight = true;

            Lighting.AddLight(
                (int)((projectile.Center.X) / 16f),
                (int)((projectile.Center.Y) / 16f),
                0.8f,
                0.2f,
                0.2f
            );
        }

        public override void Kill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, 90,
                        -projectile.velocity.X * 0.6f, -projectile.velocity.Y * 0.6f, 0, Color.White,
                        0.8f);
                    Main.dust[d].noGravity = true;
                }
            }
        }
    }
}
