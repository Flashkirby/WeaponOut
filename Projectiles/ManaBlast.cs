using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Flies forward, then flies back and restores mana if it hits
    /// 
    /// projectile.ai[1] is fly state
    /// </summary>
    public class ManaBlast : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mana Blast");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;

            projectile.timeLeft = 120;
            projectile.penetrate = 2;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (target.immortal) return;
            projectile.ai[1] = 2;
            projectile.netUpdate = true;
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            projectile.ai[1] = 2;
            projectile.netUpdate = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            //modified player centre
            Vector2 playerCentre = new Vector2(player.position.X - player.width / 2 + 4, player.position.Y + player.height / 3);
            
            //return path
            if (projectile.ai[1] == 2)
            {
                //don't expire for a while
                projectile.timeLeft = 3600;

                //ignore everything, just go back to player
                int dustIndex = Dust.NewDust(projectile.position, projectile.width, projectile.height, 45, 0, 0, 255, Color.Transparent, 1f);
                Main.dust[dustIndex].noGravity = true;
                projectile.damage = 0;
                projectile.tileCollide = false;
                projectile.ignoreWater = true;

                float distance = Vector2.Distance(new Vector2(projectile.position.X + projectile.width / 2, projectile.position.Y + projectile.height / 2), new Vector2(playerCentre.X + player.width / 2, playerCentre.Y + player.height / 2));

                float spd = 9f;
                if (distance > 512) spd = 18f;
                float rotToPlayer = (float)Math.Atan2(playerCentre.Y - projectile.position.Y, playerCentre.X - projectile.position.X);
                projectile.velocity *= spd;
                projectile.velocity += new Vector2(spd * (float)Math.Cos(rotToPlayer), spd * (float)Math.Sin(rotToPlayer));
                projectile.velocity /= spd + 1;

                if (distance < 20)
                {
                    player.statMana += 9;
                    if (projectile.owner == Main.myPlayer) player.ManaEffect(9); //other players will see this already
                    projectile.Kill();
                }

            }

            //self explanatory textureal stuff

            Lighting.AddLight(
                (int)((projectile.position.X + (float)(projectile.width / 2)) / 16f),
                (int)((projectile.position.Y + (float)(projectile.height / 2)) / 16f),
                0.6f,
                0.4f,
                1.1f
            );
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(25, (int)projectile.position.X, (int)projectile.position.Y, 0);
            for (int i = 0; i < 8; i++)
            {
                int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, 21, 0, 0, 0, default(Color), 1.5f);
                Main.dust[d].velocity *= 0.1f;
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 20, 0, 0, 0, default(Color), 1.1f);
            }
        }








        #region fancy star drawing

        private int tick = 0;
        private Vector2 oldStar1 = default(Vector2);
        private Vector2 oldStar2 = default(Vector2);
        private Vector2 oldStar3 = default(Vector2);
        private float oldStar1rot = 0;
        private float oldStar2rot = 0;
        private float oldStar3rot = 0;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            tick++;
            Player player = Main.player[projectile.owner];//owner

            //this is the colour of the weapon in pvp (as a multiplication value)
            Color cmpvp = new Color(255, 255, 255);
            if (player.team != 0)
            {
                cmpvp = new Color((int)((byte)((float)Main.teamColor[player.team].R * 0.007f)),
                    (int)((byte)((float)Main.teamColor[player.team].G * 0.007f)),
                    (int)((byte)((float)Main.teamColor[player.team].B * 0.007f)),
                    1);
            }
            Vector2 centre = new Vector2(texture.Width / 2f, texture.Height / 2f);

            //star3
            Color projColour = new Color(cmpvp.R * 0.6f, cmpvp.G * 0.6f, cmpvp.B * 0.6f, 0.8f);
            spriteBatch.Draw(texture,
                oldStar3 - Main.screenPosition + centre,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                projColour,
                oldStar3rot,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            //star2
            projColour = new Color(cmpvp.R * 0.7f, cmpvp.G * 0.7f, cmpvp.B * 0.7f, 0.6f);
            spriteBatch.Draw(texture,
                oldStar2 - Main.screenPosition + centre,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                projColour,
                oldStar2rot,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            //star1
            projColour = new Color(cmpvp.R * 0.85f, cmpvp.G * 0.85f, cmpvp.B * 0.85f, 0.4f);
            spriteBatch.Draw(texture,
                oldStar1 - Main.screenPosition + centre,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                projColour,
                oldStar1rot,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            //this star
            projColour = new Color(cmpvp.R * 1f, cmpvp.G * 1f, cmpvp.B * 1f, 0.2f);
            spriteBatch.Draw(texture,
                projectile.position - Main.screenPosition + centre,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                projColour,
                projectile.rotation,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            if (tick % 2 == 0 && !Main.gamePaused)
            {
                oldStar2 = oldStar1;
                oldStar3 = oldStar2;
                oldStar1 = projectile.position;
                oldStar3rot = oldStar2rot;
                oldStar2rot = oldStar1rot;
                oldStar1rot = projectile.rotation;
            }
            return false;
        }
        #endregion
    
    }
}
