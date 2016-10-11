using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// scale, size and damage increased with mana when cast
    /// </summary>
    public class ManaBolt : ModProjectile
    {
        private const int BEAM_LENGTH = 10;
        public override void SetDefaults()
        {
            projectile.name = "Mana Bolt";
            Main.projFrames[projectile.type] = 3;
            projectile.width = 32;
            projectile.height = 32;
           // projectile.aiStyle = -1;

            projectile.timeLeft = 45;
            projectile.penetrate = -1;
            projectile.extraUpdates = 2;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        //ai[0] is count
        //ai[1] is spawnscale
        //localAI[0] is countdowntospawn
        Vector2 spawnPosCentre;
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (projectile.localAI[0] == 0)
            {
                spawnPosCentre = projectile.Center;
                projectile.scale = 0.5f + 0.8f * player.statMana / 400f;
                projectile.ai[1] = projectile.scale;
            }
            if (projectile.ai[1] != 0)
            {
                projectile.scale = projectile.ai[1];
            }
            int reachEnd = (int)(150f / projectile.velocity.Length() * projectile.ai[1]);
            if (projectile.localAI[0] == reachEnd)
            {
                if (projectile.owner == Main.myPlayer && projectile.ai[0] < BEAM_LENGTH) //spawn new projectil to carry on
                {
                    Projectile.NewProjectile(
                        projectile.Center,
                        projectile.velocity,
                        projectile.type,
                        projectile.damage,
                        projectile.knockBack,
                        projectile.owner,
                       projectile.ai[0] + 1,
                        projectile.ai[1]);
                }
                projectile.localAI[0] = -reachEnd; //disable any possible projectile spawns

                Main.PlaySound(2, projectile.position, 9);
            }
            if (projectile.localAI[0] >= 0) //add up
            {
                projectile.localAI[0]++;
            }
            else //loop projectile
            {
                projectile.localAI[0]--;
                projectile.damage = 0;

                //loop around until death
                //Main.NewText(projectile.localAI[0] + " < " + (-reachEnd));
                if (projectile.localAI[0] < -reachEnd)
                {
                    projectile.localAI[0] = -1;
                    projectile.Center = spawnPosCentre;
                }
            }

            Lighting.AddLight(
                (int)((projectile.position.X + (float)(projectile.width / 2)) / 16f),
                (int)((projectile.position.Y + (float)(projectile.height / 2)) / 16f),
                0.6f,
                0.4f,
                1.1f
            );

            //trailer
            int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, WeaponOut.DustIDManaDust);
            Main.dust[d].velocity = projectile.velocity / 2;
            d = Dust.NewDust(projectile.position, projectile.width, projectile.height, WeaponOut.DustIDManaDust, 0f, 0f, 0, default(Color), projectile.scale);
            Main.dust[d].velocity = projectile.velocity / 2;
            
            //dividers
            d = Dust.NewDust(projectile.position, projectile.width, projectile.height, WeaponOut.DustIDManaDust);
            Main.dust[d].velocity = new Vector2(
                projectile.velocity.Y * projectile.scale * 0.2f,
                -projectile.velocity.X * projectile.scale * 0.2f);
            d = Dust.NewDust(projectile.position, projectile.width, projectile.height, WeaponOut.DustIDManaDust);
            Main.dust[d].velocity = new Vector2(
                -projectile.velocity.Y * projectile.scale * 0.2f,
                projectile.velocity.X * projectile.scale * 0.2f);

            //tilehitter
            if (tileHit != null)
            {
                float timeLeftNormal = (float)projectile.timeLeft / 45;
                d = Dust.NewDust(tileHit, projectile.width, projectile.height, WeaponOut.DustIDManaDust, 0f, 0f, 0, default(Color), projectile.scale * 2);
                Main.dust[d].velocity = new Vector2(
                    projectile.velocity.Y * projectile.scale * 0.6f * timeLeftNormal,
                    -projectile.velocity.X * projectile.scale * 0.6f * timeLeftNormal);
                d = Dust.NewDust(tileHit, projectile.width, projectile.height, WeaponOut.DustIDManaDust, 0f, 0f, 0, default(Color), projectile.scale * 2);
                Main.dust[d].velocity = new Vector2(
                    -projectile.velocity.Y * projectile.scale * 0.6f * timeLeftNormal,
                    projectile.velocity.X * projectile.scale * 0.6f * timeLeftNormal);
            }
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
        }

        public override void TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 8;
            height = 8;
        }

        public Vector2 tileHit;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (tileHit == null)
            {
                Main.PlaySound(2, projectile.position, 9);
            }
            tileHit = projectile.position;
            projectile.localAI[0] = -1;
            projectile.Center = spawnPosCentre;
            projectile.velocity = oldVelocity;
            projectile.netUpdate = true;
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft < 15) projectile.frame = 2;
            else if (projectile.timeLeft < 30) projectile.frame = 1;
            else projectile.frame = 0;

            Player player = Main.player[projectile.owner];//owner
            int halfWidth = WeaponOut.textureMANBO.Width / 2;
            int singleFrameHeight = WeaponOut.textureMANBO.Height / Main.projFrames[projectile.type];
            Vector2 centre = new Vector2(halfWidth, singleFrameHeight - halfWidth);

            spriteBatch.Draw(WeaponOut.textureMANBO,
                spawnPosCentre - Main.screenPosition,
                new Rectangle?(new Rectangle(0, singleFrameHeight * projectile.frame, WeaponOut.textureMANBO.Width, WeaponOut.textureMANBO.Height / Main.projFrames[projectile.type])),
                Color.White,
                projectile.rotation + 1.57f,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
