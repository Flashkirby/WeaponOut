using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class APARocketI : ModProjectile
    {
        public override void SetDefaults()
        {
            APARocketI.setDefaults(projectile);
        }
        public override void AI()
        {
            if (Math.Abs(projectile.velocity.X) >= 8f || Math.Abs(projectile.velocity.Y) >= 8f)
            {
                int num3;
                for (int num247 = 0; num247 < 2; num247 = num3 + 1)
                {
                    float num248 = 0f;
                    float num249 = 0f;
                    if (num247 == 1)
                    {
                        num248 = projectile.velocity.X * 0.5f;
                        num249 = projectile.velocity.Y * 0.5f;
                    }
                    int num250 = Dust.NewDust(new Vector2(projectile.position.X + 3f + num248, projectile.position.Y + 3f + num249) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, 6, 0f, 0f, 100, default(Color), 1f);
                    Dust dust3 = Main.dust[num250];
                    dust3.scale *= 2f + (float)Main.rand.Next(10) * 0.1f;
                    dust3 = Main.dust[num250];
                    dust3.velocity *= 0.2f;
                    Main.dust[num250].noGravity = true;
                    num250 = Dust.NewDust(new Vector2(projectile.position.X + 3f + num248, projectile.position.Y + 3f + num249) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, 31, 0f, 0f, 100, default(Color), 0.5f);
                    Main.dust[num250].fadeIn = 1f + (float)Main.rand.Next(5) * 0.1f;
                    dust3 = Main.dust[num250];
                    dust3.velocity *= 0.05f;
                    num3 = num247;
                }
            }
            if (Math.Abs(projectile.velocity.X) < 15f && Math.Abs(projectile.velocity.Y) < 15f)
            {
                projectile.velocity *= 1.05f;
            }

            //rotate in direction
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
        }

        public override void Kill(int timeLeft)
        {
            //else if (projectile.type == 133 || projectile.type == 134 || projectile.type == 135 || projectile.type == 136 || projectile.type == 137 || projectile.type == 138 || projectile.type == 338 || projectile.type == 339)
			//{
				projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
				projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
				projectile.width = 128;
				projectile.height = 128;
				projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
				projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
				projectile.knockBack = 8f;
			//}
            /*
			else if (projectile.type == 139 || projectile.type == 140 || projectile.type == 141 || projectile.type == 142 || projectile.type == 143 || projectile.type == 144 || projectile.type == 340 || projectile.type == 341)
			{
				projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
				projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
				projectile.width = 200;
				projectile.height = 200;
				projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
				projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
				projectile.knockBack = 10f;
			}
             */ 
        }

        public static void setDefaults(Projectile projectile)
        {
            projectile.name = "Rocket";
            projectile.width = 14;
            projectile.height = 14;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.ranged = true;
            projectile.extraUpdates = 1; //doublespeed
        }
    }
}
