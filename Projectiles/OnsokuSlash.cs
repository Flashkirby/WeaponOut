using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class OnsokuSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.name = "Onsoku Slash";
            projectile.melee = true;
            projectile.width = Player.defaultWidth;
            projectile.height = Player.defaultHeight;
            
            projectile.penetrate = -1;
        }

        public float UpdateCount { get { return projectile.ai[0]; }set { projectile.ai[0] = value; } }
        public float DashCount { get { return projectile.ai[0] - 20; } }
        public Vector2 dashStep;
        public const float dashStepCount = 6;
        public const float dashStepDelay = 8;

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (player.dead || !player.active)
            {
                projectile.timeLeft = 0;
                return;
            }
            player.GetModPlayer<PlayerFX>(mod).dashingSpecialAttack = PlayerFX.dashingSpecialAttackOnsoku;

            // Get dash location
            if (UpdateCount == 0)
            {
                for (int i = 0; i < dashStepCount * 8; i++)
                {
                    Vector2 move = Collision.TileCollision(
                        projectile.position, projectile.velocity / 2,
                        projectile.width, projectile.height,
                        true, true, (int)player.gravDir);
                    if (move == Vector2.Zero) break;
                    projectile.position += move / 2;
                }
                dashStep = (projectile.Center - player.Center) / dashStepCount;


                // dash dust
                for (int i = 0; i < dashStepCount; i++)
                {
                    Vector2 pos = player.Center + (dashStep * i) - new Vector2(4, 4);
                    for (int j = 0; j < 5; j++)
                    {
                        pos += dashStep * (j / 5f);
                        Dust d = Main.dust[Dust.NewDust(pos, 0, 0,
                            175, projectile.velocity.X, projectile.velocity.Y,
                            0, Color.White, 1f)];
                        d.noGravity = true;
                        d.velocity *= 0.05f;
                    }
                }
                projectile.velocity = Vector2.Zero;
            }

            // Dash towards location
            if (UpdateCount >= dashStepDelay)
            {
                if (UpdateCount == dashStepDelay)
                {
                    Main.PlaySound(2, player.Center, 28);

                    dashStep = (projectile.Center - player.Center) / dashStepCount;
                    player.inventory[player.selectedItem].useStyle = 3;
                }

                // freeze in swing
                player.itemAnimation = player.itemAnimationMax - 2;
                player.itemTime = player.itemAnimationMax - 2;

                // dash, change position to influence camera lerp
                player.position += Collision.TileCollision(player.position,
                    dashStep / 2,
                    player.width,
                    player.height,
                    true, true, (int)player.gravDir);
                player.velocity = Collision.TileCollision(player.position,
                    dashStep * 0.8f,
                    player.width,
                    player.height,
                    true, true, (int)player.gravDir);

                // Set immunities
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 2);
                player.immuneNoBlink = true;
                player.fallStart = (int)(player.position.Y / 16f);
                player.fallStart2 = player.fallStart;

                //point in direction
                if (dashStep.X > 0) player.direction = 1;
                if (dashStep.X < 0) player.direction = -1;

                if (UpdateCount >= dashStepDelay + dashStepCount - 1)
                {
                    projectile.timeLeft = 0;
                }

                Vector2 pos = player.Center - new Vector2(4, 4);
                for (int i = 0; i < 10; i++)
                {
                    pos -= dashStep * (i / 20f);
                    Dust d = Main.dust[Dust.NewDust(pos, 0, 0,
                        181, projectile.velocity.X, projectile.velocity.Y,
                            0, default(Color), 1.3f)];
                    d.noGravity = true;
                    d.velocity *= 0.1f;
                }
            }
            else
            {
                // slow until move
                player.velocity *= 0.8f;
            }

            //Dust.NewDust(projectile.position, projectile.width, projectile.height, 20);

            UpdateCount++;
        }

        public override void Kill(int timeLeft)
        {
            Player player = Main.player[projectile.owner];
            player.velocity = dashStep / dashStepCount;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false; // slide not stop on tiles
        }
    }
}
