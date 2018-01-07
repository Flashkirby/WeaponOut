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
    /// Draws a laser to the destination, then fires at high speed
    /// ai[0] is the timer
    /// ai[1] is the laser scale
    /// </summary>
    public class DemonBlast : ModProjectile
    {

        private const int chargeTime = 200;
        private const float muzzleDist = 1f;
        private const int hitboxSize = 4;
        private const int hitboxHalfSize = hitboxSize / 2;

        private Vector2 staPos; //visual laser start position
        private Vector2 endPos; //visual laser end position


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Blast");
            DisplayName.AddTranslation(GameCulture.Chinese, "恶魔冲击波");
			DisplayName.AddTranslation(GameCulture.Russian, "Демонический Луч");
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;

            projectile.timeLeft = 360;
            projectile.penetrate = -1;
            projectile.extraUpdates = 3; //THE SECRET TO GOING VERY FAST (updates per frame)

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            //Main.NewText("time: "+ projectile.timeLeft + "ai[0]: \n" + projectile.ai[0] + " | ai[1]: \n" + projectile.ai[1]);
            if (projectile.ai[0] < chargeTime) //spindown
            {
                AICharge();
            }
            else
            {
                AILaunch();
            }
            projectile.ai[0]++;
        }

        private void AICharge()
        {
            projectile.position -= projectile.velocity; //stay still (but keep velo in case)
            if (projectile.ai[0] == 0) //initial firing
            {
                //move forwards
                projectile.position += projectile.velocity * muzzleDist;
                projectile.ai[1] = 0.1f;

                //set rotation direction scale
                if (projectile.velocity.X > 0) projectile.spriteDirection = 1;
                else projectile.spriteDirection = -1;
                projectile.scale = 1.5f;

                //get end point of laser
                staPos = projectile.position + projectile.velocity * muzzleDist;
                endPos = projectile.position;
                for (int i = 0; i < projectile.timeLeft; i++) //roughly 1024 ft.
                {
                    //custom collision to match laser size, once per frame
                    Vector2 halfVelo = projectile.velocity * 0.5f;
                    Vector2 alteredVelo = Collision.TileCollision(new Vector2(endPos.X - hitboxHalfSize + projectile.width / 2, endPos.Y - hitboxHalfSize + projectile.height / 2), halfVelo, hitboxSize, hitboxSize, true, true);
                    if (halfVelo != alteredVelo)
                    {
                        endPos += alteredVelo;
                        break;
                    }
                    alteredVelo = Collision.TileCollision(new Vector2(endPos.X - hitboxHalfSize + projectile.width / 2, endPos.Y - hitboxHalfSize + projectile.height / 2) + halfVelo, halfVelo, hitboxSize, hitboxSize, true, true);
                    if (halfVelo != alteredVelo)
                    {
                        endPos += halfVelo + alteredVelo;
                        break;
                    }
                    endPos += projectile.velocity;
                }

                //emit dust
                for (int i = 0; i < 5; i++)
                {
                    int d1 = Dust.NewDust(projectile.position + new Vector2(projectile.width / 2, projectile.height / 2) + projectile.velocity * muzzleDist, 1, 1, 66, 0f, 0f, 100, Color.Purple, 1.5f);
                    Main.dust[d1].noGravity = true;
                    Main.dust[d1].velocity *= 0.4f;
                    d1 = Dust.NewDust(endPos + new Vector2(projectile.width / 2, projectile.height / 2), 0, 0, 66, 0f, 0f, 100, Color.Purple, 1f);
                    Main.dust[d1].noGravity = true;
                    Main.dust[d1].velocity *= 0.4f;
                }
            }

            projectile.scale -= 0.007f; //shrink
            projectile.ai[1] *= 1.004f; //grow laser
            projectile.rotation += 0.05f * projectile.spriteDirection * (projectile.ai[0] * 0.2f); //spin faster and faster
        }
        private void AILaunch()
        {
            //don't update actually, causes weird desync issues clientside...
            //if (Main.myPlayer == projectile.owner) projectile.netUpdate = true;

            if (projectile.ai[0] == chargeTime)
            {
                //move backwards
                projectile.position -= projectile.velocity * muzzleDist;

                //set laser size etc...
                projectile.ai[1] = 1.7f;
                projectile.scale = 1.2f;

                Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 33, 0.4f, 0.2f);
            }
            //damage fall off once per 2 frames
            if (projectile.damage > 1 && projectile.timeLeft % 2 == 0) projectile.damage -= 1;

            //point in direction and shrink over time
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
            projectile.scale *= 0.992f;
            if (projectile.ai[1] > 0.12f) projectile.ai[1] *= 0.98f;

            //purpel stuph
            int d1 = Dust.NewDust(projectile.position + new Vector2(projectile.width / 2f, projectile.height / 2f), 1, 1, 66, 0f, 0f, 100, Color.Purple, 1f);
            Main.dust[d1].noGravity = true;
            Main.dust[d1].velocity *= 0.5f;

            //custom collision to match laser size, once per frame
            Vector2 halfVelo = projectile.velocity * 0.5f;
            Vector2 alteredVelo = Collision.TileCollision(new Vector2(projectile.position.X - hitboxHalfSize + projectile.width / 2, projectile.position.Y - hitboxHalfSize + projectile.height / 2), halfVelo, hitboxSize, hitboxSize, true, true);
            if (halfVelo != alteredVelo)
            {
                projectile.Kill();
            }
            alteredVelo = Collision.TileCollision(new Vector2(projectile.position.X - hitboxHalfSize + projectile.width / 2, projectile.position.Y - hitboxHalfSize + projectile.height / 2) + halfVelo, halfVelo, hitboxSize, hitboxSize, true, true);
            if (halfVelo != alteredVelo)
            {
                projectile.Kill();
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int j = 0; j < 40; j++)
            {
                int d2 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 66, (j / 90f) * -projectile.velocity.X, (j / 90f) * -projectile.velocity.Y, 100, Color.Purple, 1.2f);
                Main.dust[d2].noGravity = true;
                Main.dust[d2].velocity *= 0.6f;
            }
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 centre = new Vector2(texture.Width / 2f, texture.Height / 2f);

            drawLaser(spriteBatch, centre);

            spriteBatch.Draw(texture,
                projectile.position - Main.screenPosition + centre,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                Color.White,
                projectile.rotation,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }

        private void drawLaser(SpriteBatch spritebatch, Vector2 centre)
        {
            Vector2 projectileCentre = projectile.position + new Vector2(projectile.width / 2, projectile.height / 2);
            Vector2 start, end;
            if (projectile.ai[0] > 0)
            {
                if (projectile.ai[0] < chargeTime) //charge
                {
                    start = projectileCentre;
                    end = endPos + centre;
                }
                else                               //fire!
                {
                    start = staPos + centre;
                    end = projectileCentre;
                }

                Utils.DrawLaser(
                    spritebatch,
                    mod.GetTexture("Projectiles/DemonBlast_Beam"),
                    start - Main.screenPosition,
                    end - Main.screenPosition,
                    new Vector2(projectile.ai[1]),
                    new Utils.LaserLineFraming(DemonLaser)); //uses delegate (see method below)
            }
            
            
        }
        //define which frames are used in each stage (0 = start, 1 = mid, 2 = end
        private void DemonLaser(int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame, out float distCovered, out Rectangle frame, out Vector2 origin, out Color color)
        {
            color = Color.White;
			if (stage == 0)
			{
				distCovered = 33f;
				frame = new Rectangle(0, 0, 16, 16);
				origin = frame.Size() / 2f;
				return;
			}
			if (stage == 1)
			{
				frame = new Rectangle(0, 16, 16, 16);
				distCovered = (float)frame.Height;
				origin = new Vector2((float)(frame.Width / 2), 0f);
				return;
			}
			if (stage == 2)
			{
				distCovered = 22f;
				frame = new Rectangle(0, 24, 16, 16);
				origin = new Vector2((float)(frame.Width / 2), 1f);
				return;
			}
			distCovered = projectile.velocity.Length() * 90;
			frame = Rectangle.Empty;
			origin = Vector2.Zero;
			color = Color.Transparent;
        }
        
    }
}
