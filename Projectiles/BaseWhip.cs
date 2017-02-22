using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Enums;
// version = 1.3
namespace WeaponOut.Projectiles
{
    public static class BaseWhip
    {
        /// <summary>
        /// Imitates the Solar Eruption, makes the whip swing forwards and pull back halfway through
        /// </summary>
        /// <param name="projectile">Reference to the projectile</param>
        /// <param name="whipLength">Approximately half length in tiles</param>
        /// <returns>End position of the whip</returns>
        public static Vector2 WhipAI(Projectile projectile, float whipLength = 16, bool ignoreTiles = false, int sndgroup = 2, int sound = 39)
        {
            //use localAI[1] to track hitting something
            Player player = Main.player[projectile.owner];
            if (projectile.ai[0] == 0f)
            {
                projectile.localAI[1] = player.itemAnimationMax;
            }
            float speedIncrease = (float)player.HeldItem.useAnimation / projectile.localAI[1];

            return AI_075(projectile, whipLength * speedIncrease / projectile.MaxUpdates, 
                (int)projectile.localAI[1], ignoreTiles, sndgroup, sound);
        }

        /// <summary>
        /// ai0:time out
        /// ai1:swing rotation
        /// lai0:static rotation
        /// lai1:custom swingtime
        /// </summary>
        private static Vector2 AI_075(Projectile projectile, float swingLength, int swingTime, bool ignoreTiles, int sndgroup, int sound)
        {
            Player player = Main.player[projectile.owner];
            float num = 1.57079637f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, true);

            //make visible quickly
            projectile.alpha -= 42;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            //set local AI to direction

            if (projectile.localAI[0] == 0f)
            {
                projectile.localAI[0] = projectile.velocity.ToRotation();
            }
            float num33 = (float)((projectile.localAI[0].ToRotationVector2().X >= 0f) ? 1 : -1);
            if (projectile.ai[1] <= 0f)
            {
                num33 *= -1f;
            }
            Vector2 vector25 = (num33 * (projectile.ai[0] / swingTime * 6.28318548f - 1.57079637f)).ToRotationVector2();
            vector25.Y *= (float)Math.Sin((double)projectile.ai[1]);
            if (projectile.ai[1] <= 0f)
            {
                vector25.Y *= -1f;
            }
            vector25 = vector25.RotatedBy((double)projectile.localAI[0], default(Vector2));
            projectile.ai[0] += 1f / projectile.MaxUpdates;
            if (projectile.ai[0] < swingTime)
            {
                projectile.velocity += swingLength * vector25;
            }
            else
            {
                projectile.Kill();
            }

            projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.itemTime = Math.Max(player.itemTime, player.itemAnimationMax - (int)projectile.localAI[1]);
            player.itemAnimation = Math.Max(2, player.itemAnimationMax - (int)projectile.localAI[1]);
            player.itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));

            Vector2 vector34 = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
            if (player.direction != 1)
            {
                vector34.X = (float)player.bodyFrame.Width - vector34.X;
            }
            if (player.gravDir != 1f)
            {
                vector34.Y = (float)player.bodyFrame.Height - vector34.Y;
            }
            vector34 -= new Vector2((float)(player.bodyFrame.Width - player.width), (float)(player.bodyFrame.Height - 42)) / 2f;
            projectile.Center = player.RotatedRelativePoint(player.position + vector34, true) - projectile.velocity;

            //collide with tiles
            Vector2 endPoint = projectile.position + projectile.velocity * 2f;

            if (projectile.ai[0] > 1 && !ignoreTiles)//delay and stops close tile collision
            {
                Vector2 prevPoint = projectile.oldPosition + projectile.oldVelocity * 2f;
                if (!Collision.CanHit(endPoint, projectile.width, projectile.height, prevPoint, projectile.width, projectile.height))
                {
                    if (projectile.ai[0] * 2 < projectile.localAI[1])
                    {
                        projectile.localAI[1] = projectile.ai[0] * 2;
                        Main.PlaySound(sndgroup, endPoint, sound);
                        Collision.HitTiles(endPoint, endPoint - prevPoint, 8, 8);
                    }
                }
            }

            return endPoint;


            // Main.NewText("========================");
            // Main.NewText("ai0: " + projectile.ai[0]);// ==== 0 - 30
            // Main.NewText("ai1: " + projectile.ai[1]);// ==== 0
            // Main.NewText("lai0: " + projectile.localAI[0]);// ==== rotation 0 - 3.14
            // Main.NewText("lai1: " + projectile.localAI[1]);// ==== swingtime
            // Main.NewText("anim: " + player.itemAnimation);// ==== 2
        }

        public static bool IsCrit(Projectile projectile, bool easyCrit)
        {
            return projectile.ai[0] <= projectile.localAI[1] / 2 + (projectile.localAI[1] / (easyCrit ? 12 : 16)) &&
                projectile.ai[0] >= projectile.localAI[1] / 2 - (projectile.localAI[1] / (easyCrit ? 6 : 8));
        }

        public static void ModifyHitAny(Projectile projectile, ref int damage, ref bool crit)
        {
            return; //disabled due to not working in multiplayer.
            /*
            float pvpKnockback = 0f;
            ModifyHitAny(projectile, ref damage, ref pvpKnockback, ref crit);*/
        }
        public static void ModifyHitAny(Projectile projectile, ref int damage, ref float knockback, ref bool crit, bool easyCrit = false)
        {
            //Main.NewText("tip hit : " + (projectile.ai[0]) + " | " + (projectile.localAI[1] / 2));
            if (IsCrit(projectile, easyCrit))
            {
                Player p = Main.player[projectile.owner];
                //Main.NewText("crit: " + p.inventory[p.selectedItem].crit + p.meleeCrit);
                damage = (int)(damage * (1 + (p.inventory[p.selectedItem].crit + p.meleeCrit) * 0.01f));
                knockback *= 2;
                crit = true;
            }
            else
            {
                crit = false;
            }
        }

        public static void OnHitAny(Projectile projectile, bool crit, bool whipSoftSound)
        {
            OnHitAny(projectile, null, crit, whipSoftSound);
        }
        public static void OnHitAny(Projectile projectile, NPC target, bool crit, bool whipSoftSound)
        {
            if (crit) { Main.PlaySound(2, target.Center, whipSoftSound ? 98 : 40); }
            if (target != null)
            {
                projectile.localNPCImmunity[target.whoAmI] = 10;
                target.immune[projectile.owner] = (int)(projectile.localAI[1] - projectile.ai[0] / projectile.MaxUpdates);
            }
        }

        public static bool? Colliding(Projectile projectile, Rectangle targetHitbox)
        {

            float collisionPoint = 0f;
            if (Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                projectile.Center, projectile.Center + projectile.velocity,
                projectile.width * projectile.scale, ref collisionPoint))
            {
                return true;
            }
            return false;
        }

        public static bool CanCutTiles(Projectile projectile)
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity, (float)projectile.width * projectile.scale, new Utils.PerLinePoint(DelegateMethods.CutTiles));
            return true;
        }
        
        public static bool PreDraw(Projectile projectile, int handleHeight, int chainHeight, int partHeight, int tipHeight, int partCount, bool ignoreLight, bool easyCrit)
        {
            Vector2 vector38 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
            Texture2D texture2D17 = Main.projectileTexture[projectile.type];
            Color alpha3;
            if (ignoreLight)
            {
                alpha3 = Color.White;
            }
            else
            {
                alpha3 = projectile.GetAlpha(Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16));
            }

            //define texture parts here
            Rectangle handle = new Rectangle(0, 0, texture2D17.Width, handleHeight);
            Rectangle chain = new Rectangle(0, handleHeight, texture2D17.Width, chainHeight);
            Rectangle part = new Rectangle(0, handleHeight + chainHeight, texture2D17.Width, partHeight);
            Rectangle tip = new Rectangle(0, handleHeight + chainHeight + partHeight, texture2D17.Width, tipHeight);


            if (projectile.velocity == Vector2.Zero)
            {
                return false;
            }
            float chainCount = projectile.velocity.Length() + 16f;
            bool halfSize = chainCount < partHeight * 4.5f;
            Vector2 value17 = Vector2.Normalize(projectile.velocity);
            Rectangle rectangle6 = handle;
            Vector2 value18 = new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            float rotation24 = projectile.rotation + 3.14159274f;
            Main.spriteBatch.Draw(texture2D17, projectile.Center.Floor() - Main.screenPosition + value18, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, rectangle6.Size() / 2f - Vector2.UnitY * 4f, projectile.scale, SpriteEffects.None, 0f);
            chainCount -= 40f * projectile.scale;
            Vector2 vector39 = projectile.Center.Floor();
            vector39 += value17 * projectile.scale * handle.Height / 2;
            Vector2 vector40;
            rectangle6 = chain;
            if (chainCount > 0f)
            {
                float num198 = 0f;
                while (num198 + 1f < chainCount)
                {
                    if (chainCount - num198 < (float)rectangle6.Height)
                    {
                        rectangle6.Height = (int)(chainCount - num198);
                    }
                    vector40 = vector39 + value18;
                    if(!ignoreLight) alpha3 = projectile.GetAlpha(Lighting.GetColor((int)vector40.X / 16, (int)vector40.Y / 16));
                    Main.spriteBatch.Draw(texture2D17, vector40 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, new Vector2((float)(rectangle6.Width / 2), 0f), projectile.scale, SpriteEffects.None, 0f);
                    num198 += (float)rectangle6.Height * projectile.scale;
                    vector39 += value17 * (float)rectangle6.Height * projectile.scale;
                }
            }
            Vector2 value19 = vector39;
            vector39 = projectile.Center.Floor();
            vector39 += value17 * projectile.scale * chain.Height / 2;
            rectangle6 = part;
            //int partCount = 18;
            if (halfSize)
            {
                partCount /= 2;
            }
            float num200 = chainCount;
            if (chainCount > 0f)
            {
                float num201 = 0f;
                float num202 = num200 / (float)partCount;
                num201 += num202 * 0.25f;
                vector39 += value17 * num202 * 0.25f;
                int num44;
                for (int num203 = 0; num203 < partCount; num203 = num44 + 1)
                {
                    float num204 = num202;
                    if (num203 == 0)
                    {
                        num204 *= 0.75f;
                    }
                    vector40 = vector39 + value18;
                    if (!ignoreLight) alpha3 = projectile.GetAlpha(Lighting.GetColor((int)vector40.X / 16, (int)vector40.Y / 16));
                    Main.spriteBatch.Draw(texture2D17, vector40 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, new Vector2((float)(rectangle6.Width / 2), 0f), projectile.scale, SpriteEffects.None, 0f);
                    num201 += num204;
                    vector39 += value17 * num204;
                    num44 = num203;
                }
            }
            rectangle6 = tip;
            Vector2 vector41 = value19 + value18;
            if (!ignoreLight) alpha3 = projectile.GetAlpha(Lighting.GetColor((int)vector41.X / 16, (int)vector41.Y / 16));
            
            if (IsCrit(projectile, easyCrit) && projectile.oldVelocity != Vector2.Zero)
            {
                Vector2 lastDiff = (projectile.velocity - projectile.oldVelocity);
                Vector2 lastPosition2 = vector41 - lastDiff * 2;
                Vector2 lastPosition = vector41 - lastDiff * 1;

                Main.spriteBatch.Draw(texture2D17, lastPosition2 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3 * 0.4f, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), projectile.scale * 2f, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(texture2D17, lastPosition - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3 * 0.66f, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), projectile.scale * 1.5f, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture2D17, vector41 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    
    
    }
}
