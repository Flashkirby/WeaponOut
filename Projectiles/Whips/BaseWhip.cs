using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Enums;

namespace WeaponOut.Projectiles.Whips
{
    /// <summary>
    /// Version 1.4 by Flashkirby
    /// </summary>
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
                projectile.restrikeDelay = 0;
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
            //projectile.alpha -= 42;
            //if (projectile.alpha < 0)
            //{
            //    projectile.alpha = 0;
            //}
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
            player.itemTime = Math.Max(player.itemTime, projectile.restrikeDelay);
            player.itemAnimation = Math.Max(2, projectile.restrikeDelay);
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
                        projectile.restrikeDelay = player.itemAnimationMax - (int)projectile.ai[0] * 2;
                        projectile.ai[0] = Math.Max(1f, projectile.localAI[1] - projectile.ai[0] + 1);
                        projectile.ai[1] *= -0.9f; // come back in reverse
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
            // Main.NewText("anim: " + player.itemAnimation + " / " + player.itemTime);// ==== 2
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
            //Main.NewText("tip hit : \n" + (projectile.ai[0]) + " | \n" + (projectile.localAI[1] / 2));
            if (IsCrit(projectile, easyCrit))
            {
                Player p = Main.player[projectile.owner];
                //Main.NewText("crit: \n" + p.inventory[p.selectedItem].crit + p.meleeCrit);
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
            foreach (Projectile p in Main.projectile)
            {
                if (!p.active || p.owner != projectile.owner || p.type != projectile.type) continue;
                Vector2 vector38 = p.position + new Vector2((float)p.width, (float)p.height) / 2f + Vector2.UnitY * p.gfxOffY - Main.screenPosition;
                Texture2D texture2D17 = Main.projectileTexture[p.type];
                Color alpha3;
                if (ignoreLight)
                {
                    alpha3 = Color.White;
                }
                else
                {
                    alpha3 = p.GetAlpha(Lighting.GetColor((int)p.Center.X / 16, (int)p.Center.Y / 16));
                }
                alpha3 *= p.Opacity;

                //define texture parts here
                Rectangle handle = new Rectangle(0, 0, texture2D17.Width, handleHeight);
                Rectangle chain = new Rectangle(0, handleHeight, texture2D17.Width, chainHeight);
                Rectangle part = new Rectangle(0, handleHeight + chainHeight, texture2D17.Width, partHeight);
                Rectangle tip = new Rectangle(0, handleHeight + chainHeight + partHeight, texture2D17.Width, tipHeight);


                if (p.velocity == Vector2.Zero)
                {
                    return false;
                }
                SpriteEffects se = SpriteEffects.None;
                if (p.spriteDirection < 0)
                {
                    se = SpriteEffects.FlipHorizontally;
                }
                float chainCount = p.velocity.Length() + 16f - tipHeight / 2;
                bool halfSize = chainCount < partHeight * 4.5f;
                Vector2 normalVel = Vector2.Normalize(p.velocity);
                Rectangle currentRect = handle;
                Vector2 gfxOffY = new Vector2(0f, Main.player[p.owner].gfxOffY);
                float rotation24 = p.rotation + 3.14159274f;
                Main.spriteBatch.Draw(texture2D17, p.Center.Floor() - Main.screenPosition + gfxOffY, new Microsoft.Xna.Framework.Rectangle?(currentRect), alpha3, rotation24, currentRect.Size() / 2f - Vector2.UnitY * 4f, p.scale, se, 0f);
                chainCount -= 40f * p.scale;
                Vector2 centre = p.Center.Floor();
                centre += normalVel * p.scale * handle.Height / 2;
                Vector2 centreOffY;
                currentRect = chain;
                if (chainCount > 0f)
                {
                    float i = 0f;
                    while (i + 1f < chainCount)
                    {
                        if (chainCount - i < (float)currentRect.Height)
                        {
                            currentRect.Height = (int)(chainCount - i);
                        }
                        centreOffY = centre + gfxOffY;
                        if (!ignoreLight) alpha3 = p.GetAlpha(Lighting.GetColor((int)centreOffY.X / 16, (int)centreOffY.Y / 16));
                        Main.spriteBatch.Draw(texture2D17, centreOffY - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(currentRect), alpha3, rotation24, new Vector2((float)(currentRect.Width / 2), 0f), p.scale, se, 0f);
                        i += (float)currentRect.Height * p.scale;
                        centre += normalVel * (float)currentRect.Height * p.scale;
                    }
                }
                Vector2 centre2 = centre;
                centre = p.Center.Floor();
                centre += normalVel * p.scale * chain.Height / 2;
                currentRect = part;
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
                    centre += normalVel * num202 * 0.25f;
                    for (int i = 0; i < partCount; i++)
                    {
                        float num204 = num202;
                        if (i == 0)
                        {
                            num204 *= 0.75f;
                        }
                        centreOffY = centre + gfxOffY;
                        if (!ignoreLight) alpha3 = p.GetAlpha(Lighting.GetColor((int)centreOffY.X / 16, (int)centreOffY.Y / 16));
                        Main.spriteBatch.Draw(texture2D17, centreOffY - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(currentRect), alpha3, rotation24, new Vector2((float)(currentRect.Width / 2), 0f), p.scale, se, 0f);
                        num201 += num204;
                        centre += normalVel * num204;
                    }
                }
                currentRect = tip;
                Vector2 centreOffY2 = centre2 + gfxOffY;
                if (!ignoreLight) alpha3 = p.GetAlpha(Lighting.GetColor((int)centreOffY2.X / 16, (int)centreOffY2.Y / 16));

                // Apply crit visual
                if (IsCrit(p, easyCrit) && p.oldVelocity != Vector2.Zero)
                {
                    Vector2 pPosition = new Vector2(), pOldPos = new Vector2();
                    if (p.owner >= 0 && p.owner < Main.player.Length)
                    {
                        pPosition = Main.player[p.owner].position;
                        pOldPos = Main.player[p.owner].oldPosition;
                    }
                    Vector2 lastDiff = ((pPosition + p.velocity) - (pOldPos + p.oldVelocity));
                    Vector2 lastPosition2 = centreOffY2 - lastDiff * 2;
                    Vector2 lastPosition = centreOffY2 - lastDiff * 1;

                    Main.spriteBatch.Draw(texture2D17, lastPosition2 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(currentRect), alpha3 * 0.4f, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), p.scale * 2f, se, 0f);

                    Main.spriteBatch.Draw(texture2D17, lastPosition - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(currentRect), alpha3 * 0.66f, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), p.scale * 1.5f, se, 0f);
                }

                Main.spriteBatch.Draw(texture2D17, centreOffY2 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(currentRect), alpha3, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), p.scale, se, 0f);
            }
            return false;
        }
    
    
    }
}
