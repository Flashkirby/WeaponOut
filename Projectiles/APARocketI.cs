using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Rockets move up to a set speed, and upon collision
    /// with anything, drop to 3 timeleft where they
    /// spend 2 frames in explosion mode then terminate
    /// spawning dust and possibly destroying tiles
    /// </summary>
    public class APARocketI : ModProjectile
    {
        public override void SetDefaults()
        {
            APARocketI.setDefaults(projectile);
        }
        public override void AI()
        {
            APARocketI.rocketAI(projectile, 90);
        }
        public override void Kill(int timeLeft)
        {
            APARocketI.aiKill(projectile, false, 0);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            APARocketI.OnHitCollide(projectile);
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            APARocketI.OnHitCollide(projectile);
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            APARocketI.OnHitCollide(projectile);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            APARocketI.OnHitCollide(projectile, true); return false;
        }

        public const int explosionTimeLeft = 3;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="tile"></param>
        public static void OnHitCollide(Projectile projectile, bool tile = false)
        {
            if (tile) projectile.velocity = Vector2.Zero;
            projectile.timeLeft = explosionTimeLeft;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="largeExplode"></param>
        /// <param name="tileExplode">tile radius, 3 or 5</param>
        public static void aiKill(Projectile projectile, bool largeExplode = false, int tileExplode = 0)
        {
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 14);

            explosionFX(projectile, largeExplode);
            tileBreaker(projectile, tileExplode);
        }

        private static void tileBreaker(Projectile projectile, int tileExplode)
        {
            if (projectile.owner == Main.myPlayer && tileExplode > 0)
            {
                int num702 = (int)(projectile.position.X / 16f - (float)tileExplode);
                int num703 = (int)(projectile.position.X / 16f + (float)tileExplode);
                int num704 = (int)(projectile.position.Y / 16f - (float)tileExplode);
                int num705 = (int)(projectile.position.Y / 16f + (float)tileExplode);
                if (num702 < 0)
                {
                    num702 = 0;
                }
                if (num703 > Main.maxTilesX)
                {
                    num703 = Main.maxTilesX;
                }
                if (num704 < 0)
                {
                    num704 = 0;
                }
                if (num705 > Main.maxTilesY)
                {
                    num705 = Main.maxTilesY;
                }
                bool flag3 = false;
                int num9;
                for (int num706 = num702; num706 <= num703; num706 = num9 + 1)
                {
                    for (int num707 = num704; num707 <= num705; num707 = num9 + 1)
                    {
                        float num708 = Math.Abs((float)num706 - projectile.position.X / 16f);
                        float num709 = Math.Abs((float)num707 - projectile.position.Y / 16f);
                        double num710 = Math.Sqrt((double)(num708 * num708 + num709 * num709));
                        if (num710 < (double)tileExplode && Main.tile[num706, num707] != null && Main.tile[num706, num707].wall == 0)
                        {
                            flag3 = true;
                            break;
                        }
                        num9 = num707;
                    }
                    num9 = num706;
                }
                AchievementsHelper.CurrentlyMining = true;
                for (int num711 = num702; num711 <= num703; num711 = num9 + 1)
                {
                    for (int num712 = num704; num712 <= num705; num712 = num9 + 1)
                    {
                        float num713 = Math.Abs((float)num711 - projectile.position.X / 16f);
                        float num714 = Math.Abs((float)num712 - projectile.position.Y / 16f);
                        double num715 = Math.Sqrt((double)(num713 * num713 + num714 * num714));
                        if (num715 < (double)tileExplode)
                        {
                            bool flag4 = true;
                            if (Main.tile[num711, num712] != null && Main.tile[num711, num712].active())
                            {
                                flag4 = true;
                                if (Main.tileDungeon[(int)Main.tile[num711, num712].type] || Main.tile[num711, num712].type == 21 || Main.tile[num711, num712].type == 26 || Main.tile[num711, num712].type == 107 || Main.tile[num711, num712].type == 108 || Main.tile[num711, num712].type == 111 || Main.tile[num711, num712].type == 226 || Main.tile[num711, num712].type == 237 || Main.tile[num711, num712].type == 221 || Main.tile[num711, num712].type == 222 || Main.tile[num711, num712].type == 223 || Main.tile[num711, num712].type == 211 || Main.tile[num711, num712].type == 404)
                                {
                                    flag4 = false;
                                }
                                if (!Main.hardMode && Main.tile[num711, num712].type == 58)
                                {
                                    flag4 = false;
                                }
                                if (!TileLoader.CanExplode(num711, num712))
                                {
                                    flag4 = false;
                                }
                                if (flag4)
                                {
                                    WorldGen.KillTile(num711, num712, false, false, false);
                                    if (!Main.tile[num711, num712].active() && Main.netMode != 0)
                                    {
                                        NetMessage.SendData(17, -1, -1, "", 0, (float)num711, (float)num712, 0f, 0, 0, 0);
                                    }
                                }
                            }
                            if (flag4)
                            {
                                for (int num716 = num711 - 1; num716 <= num711 + 1; num716 = num9 + 1)
                                {
                                    for (int num717 = num712 - 1; num717 <= num712 + 1; num717 = num9 + 1)
                                    {
                                        if ((Main.tile[num716, num717] != null && Main.tile[num716, num717].wall > 0) & flag3)
                                        {
                                            WorldGen.KillWall(num716, num717, false);
                                            if (Main.tile[num716, num717].wall == 0 && Main.netMode != 0)
                                            {
                                                NetMessage.SendData(17, -1, -1, "", 2, (float)num716, (float)num717, 0f, 0, 0, 0);
                                            }
                                        }
                                        num9 = num717;
                                    }
                                    num9 = num716;
                                }
                            }
                        }
                        num9 = num712;
                    }
                    num9 = num711;
                }
                AchievementsHelper.CurrentlyMining = false;
            }
        }

        private static void explosionFX(Projectile projectile, bool largeExplode)
        {
            if (!largeExplode)
            {
                projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
                projectile.width = 22;
                projectile.height = 22;
                projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
                int num9;
                for (int num616 = 0; num616 < 30; num616 = num9 + 1)
                {
                    int num617 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                    Dust dust = Main.dust[num617];
                    dust.velocity *= 1.4f;
                    num9 = num616;
                }
                for (int num618 = 0; num618 < 20; num618 = num9 + 1)
                {
                    int num619 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 3.5f);
                    Main.dust[num619].noGravity = true;
                    Dust dust = Main.dust[num619];
                    dust.velocity *= 7f;
                    num619 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                    dust = Main.dust[num619];
                    dust.velocity *= 3f;
                    num9 = num618;
                }
                for (int num620 = 0; num620 < 2; num620 = num9 + 1)
                {
                    float scaleFactor9 = 0.4f;
                    if (num620 == 1)
                    {
                        scaleFactor9 = 0.8f;
                    }
                    int num621 = Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Gore gore = Main.gore[num621];
                    gore.velocity *= scaleFactor9;
                    Gore gore98 = Main.gore[num621];
                    gore98.velocity.X = gore98.velocity.X + 1f;
                    Gore gore99 = Main.gore[num621];
                    gore99.velocity.Y = gore99.velocity.Y + 1f;
                    num621 = Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                    gore = Main.gore[num621];
                    gore.velocity *= scaleFactor9;
                    Gore gore100 = Main.gore[num621];
                    gore100.velocity.X = gore100.velocity.X - 1f;
                    Gore gore101 = Main.gore[num621];
                    gore101.velocity.Y = gore101.velocity.Y + 1f;
                    num621 = Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                    gore = Main.gore[num621];
                    gore.velocity *= scaleFactor9;
                    Gore gore102 = Main.gore[num621];
                    gore102.velocity.X = gore102.velocity.X + 1f;
                    Gore gore103 = Main.gore[num621];
                    gore103.velocity.Y = gore103.velocity.Y - 1f;
                    num621 = Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64), 1f);
                    gore = Main.gore[num621];
                    gore.velocity *= scaleFactor9;
                    Gore gore104 = Main.gore[num621];
                    gore104.velocity.X = gore104.velocity.X - 1f;
                    Gore gore105 = Main.gore[num621];
                    gore105.velocity.Y = gore105.velocity.Y - 1f;
                    num9 = num620;
                }
            }
            else
            {
                projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
                projectile.width = 80;
                projectile.height = 80;
                projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
                int num9;
                for (int num622 = 0; num622 < 40; num622 = num9 + 1)
                {
                    int num623 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
                    Dust dust = Main.dust[num623];
                    dust.velocity *= 3f;
                    if (Main.rand.Next(2) == 0)
                    {
                        Main.dust[num623].scale = 0.5f;
                        Main.dust[num623].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                    num9 = num622;
                }
                for (int num624 = 0; num624 < 70; num624 = num9 + 1)
                {
                    int num625 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 3f);
                    Main.dust[num625].noGravity = true;
                    Dust dust = Main.dust[num625];
                    dust.velocity *= 5f;
                    num625 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
                    dust = Main.dust[num625];
                    dust.velocity *= 2f;
                    num9 = num624;
                }
                for (int num626 = 0; num626 < 3; num626 = num9 + 1)
                {
                    float scaleFactor10 = 0.33f;
                    if (num626 == 1)
                    {
                        scaleFactor10 = 0.66f;
                    }
                    if (num626 == 2)
                    {
                        scaleFactor10 = 1f;
                    }
                    int num627 = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Gore gore = Main.gore[num627];
                    gore.velocity *= scaleFactor10;
                    Gore gore106 = Main.gore[num627];
                    gore106.velocity.X = gore106.velocity.X + 1f;
                    Gore gore107 = Main.gore[num627];
                    gore107.velocity.Y = gore107.velocity.Y + 1f;
                    num627 = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    gore = Main.gore[num627];
                    gore.velocity *= scaleFactor10;
                    Gore gore108 = Main.gore[num627];
                    gore108.velocity.X = gore108.velocity.X - 1f;
                    Gore gore109 = Main.gore[num627];
                    gore109.velocity.Y = gore109.velocity.Y + 1f;
                    num627 = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    gore = Main.gore[num627];
                    gore.velocity *= scaleFactor10;
                    Gore gore110 = Main.gore[num627];
                    gore110.velocity.X = gore110.velocity.X + 1f;
                    Gore gore111 = Main.gore[num627];
                    gore111.velocity.Y = gore111.velocity.Y - 1f;
                    num627 = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
                    gore = Main.gore[num627];
                    gore.velocity *= scaleFactor10;
                    Gore gore112 = Main.gore[num627];
                    gore112.velocity.X = gore112.velocity.X - 1f;
                    Gore gore113 = Main.gore[num627];
                    gore113.velocity.Y = gore113.velocity.Y - 1f;
                    num9 = num626;
                }
                projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
                projectile.width = 10;
                projectile.height = 10;
                projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="dustType">90,89,92,87</param>
        public static void rocketAI(Projectile projectile, int dustType, bool largeExplode = false)
        {
            aiDust(projectile, dustType);
            aiExplode(projectile, largeExplode);

            //rotate in direction
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            projectile.ai[0]++;
        }

        private static void aiExplode(Projectile projectile, bool largeExplode)
        {
            int explodeSize = largeExplode ? 200 : 120;
            if (projectile.owner == Main.myPlayer && projectile.timeLeft <= explosionTimeLeft)
            {
                projectile.aiStyle = 16; //explosion damage player

                projectile.tileCollide = false;
                projectile.ai[1] = 0f;
                projectile.alpha = 255;

                projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
                projectile.width = explodeSize;
                projectile.height = explodeSize;
                projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
                projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
                projectile.knockBack = 8f;
            }
        }

        private static void aiDust(Projectile projectile, int dustType)
        {
            if (projectile.timeLeft <= explosionTimeLeft) return;
            float length = projectile.velocity.Length();
            if (length > 8f)
            {
                for (int i = 0; i < 4; i++) //do projectile twice per frame
                {
                    for (int j = 0; j < 3; j++) //do projectile 3 times per particle
                    {
                        float sin = length * (0.3f + 0.1f * j) * (float)Math.Sin(projectile.localAI[1] + 2.09f * j);
                        //sine wave positions
                        Vector2 dustPos = new Vector2(
                            (float)(projectile.position.X + 3f + (sin) * Math.Cos((double)projectile.rotation)),
                            (float)(projectile.position.Y + 3f + (sin) * Math.Sin((double)projectile.rotation)));
                        dustPos -= projectile.velocity / 4 * i;


                        Dust d = Main.dust[
                                Dust.NewDust(dustPos, projectile.width - 8, projectile.height - 8,
                                dustType, 0f, 0f, 100, default(Color), 0.3f + 0.1f * j)
                                                    ];
                        d.scale *= 2f + (float)Main.rand.Next(10) * 0.1f;
                        d.velocity *= 0.2f;
                        d.noGravity = true;

                        if (j == 2)
                        {
                            //smoke
                            d = Main.dust[
                                Dust.NewDust(dustPos, projectile.width - 8, projectile.height - 8,
                                31, 0f, 0f, 100, default(Color), 0.5f)
                                ];
                            d.fadeIn = 0.7f + (float)Main.rand.Next(3) * 0.1f;
                            d.velocity *= 0.05f;
                        }
                    }

                    projectile.localAI[1] += 0.1f;
                }
            }
            if (Math.Abs(projectile.velocity.X) < 15f && Math.Abs(projectile.velocity.Y) < 15f)
            {
                projectile.velocity *= 1.05f;
            }
        }

        public static void setDefaults(Projectile projectile)
        {
            projectile.name = "Rocket"; //134, 137, 140, 143
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
