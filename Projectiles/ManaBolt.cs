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
    /// scale, size and damage increased with mana when cast
    /// </summary>
    public class ManaBolt : ModProjectile
    {

        private const int BEAM_LENGTH = 10;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mana Bolt");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔力射线");
			DisplayName.AddTranslation(GameCulture.Russian, "Луч Маны");
        }
        public override void SetDefaults()
        {
            Main.projFrames[projectile.type] = 2;
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
        bool sizeChanged;
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (projectile.localAI[0] == 0)
            {
                spawnPosCentre = projectile.Center;
                projectile.ai[1] = projectile.scale;
            }
            if (projectile.ai[1] == 1f)
            {
                //damage scales between 100-300 average
                projectile.ai[1] = 1f + 0.25f + projectile.damage * 0.005f;
            }
            //limit size to stonkingly big (instead of ゴジラ big)
            projectile.ai[1] = Math.Min(3f, projectile.ai[1]);
            projectile.scale = projectile.ai[1] - 1f;

            //only the first projectile, on spawn
            if (projectile.ai[0] == 0f && projectile.localAI[0] == 0) 
            {
                projectile.velocity *= (2f + projectile.ai[1]) / 2;
            }

            //resize projectile to match scale
            if (!sizeChanged)
            {
                sizeChanged = true;
                Vector2 centre = projectile.Center;
                projectile.width = (int)(projectile.width * projectile.scale);
                projectile.height = (int)(projectile.height * projectile.scale);
                projectile.Center = centre;
            }

            int reachEnd = (int)(150f / projectile.velocity.Length() * (projectile.ai[1] - 1f));
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
                //Main.NewText(projectile.localAI[0] + " < \n" + (-reachEnd));
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
                generateTileDust(projectile.scale);
            }
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
        }

        private void generateTileDust(float scale)
        {
            int d;
            float timeLeftNormal = (float)projectile.timeLeft / 45;
            d = Dust.NewDust(tileHit, projectile.width, projectile.height, WeaponOut.DustIDManaDust, 0f, 0f, 0, default(Color), projectile.scale * 2);
            Main.dust[d].velocity = new Vector2(
                projectile.velocity.Y * scale * 0.3f * timeLeftNormal,
                -projectile.velocity.X * scale * 0.3f * timeLeftNormal);
            d = Dust.NewDust(tileHit, projectile.width, projectile.height, WeaponOut.DustIDManaDust, 0f, 0f, 0, default(Color), projectile.scale * 2);
            Main.dust[d].velocity = new Vector2(
                -projectile.velocity.Y * scale * 0.3f * timeLeftNormal,
                projectile.velocity.X * scale * 0.3f * timeLeftNormal);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 8;
            height = 8;
            return true;
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
            Texture2D texture = Main.projectileTexture[projectile.type];
            int framePoint = (projectile.timeLeft / 5);
            projectile.frame = (projectile.timeLeft / 15) % 2 == 0 ? 1 : 0;
            projectile.alpha = 0;
            switch(framePoint)
            {
                case 5:
                    projectile.alpha = 0;
                    break;
                case 4:
                    projectile.alpha = 50;
                    break;
                case 3:
                    projectile.alpha = 100;
                    break;
                case 2:
                    projectile.alpha = 150;
                    break;
                default:
                    projectile.alpha = 200;
                    break;
            }


            SpriteEffects se = SpriteEffects.None;
            if (projectile.velocity.X < 0) se = SpriteEffects.FlipHorizontally;

            Player player = Main.player[projectile.owner];//owner
            int halfWidth = texture.Width / 2;
            int singleFrameHeight = texture.Height / Main.projFrames[projectile.type];
            Vector2 centre = new Vector2(halfWidth, singleFrameHeight - halfWidth * 0.5f);

            spriteBatch.Draw(texture,
                spawnPosCentre - Main.screenPosition,
                new Rectangle?(new Rectangle(0, singleFrameHeight * projectile.frame, texture.Width, texture.Height / Main.projFrames[projectile.type])),
                new Color(projectile.Opacity, projectile.Opacity, projectile.Opacity, projectile.Opacity),
                projectile.rotation + 1.57f,
                centre,
                projectile.scale,
                se,
                0
            );
            return false;
        }
    }
}
