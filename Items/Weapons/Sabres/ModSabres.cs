using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace WeaponOut
{
    /// <summary>
    /// Sabres are, first and foremost, weapons skills
    /// as opposed to actual weapons in their own right.
    /// However when not charge attacking they have quick slashes
    /// 
    /// item.isBeingGrabbed is used to track slash up/down
    /// item.noGrabDelay is used for tracking charge attacks
    /// </summary>
    public static class ModSabres
    {

        /// <summary> 
        /// Charge item time faster when not moving, or grounded (or both!)
        /// Also handles the item.useStyle to allow for custom animation
        /// </summary>
        /// <returns>True on the frame of a charged attack</returns>
        public static bool HoldItemManager(Player player, Item item, int slashProjectileID, Color chargeColour = default(Color), float slashDelay = 0.9f, float ai1 = 1f)
        {
            bool charged = false;
            if (player.itemAnimation > 0)
            {
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    if (ai1 == 1f || ai1 == -1f)
                    {
                        // Use isBeingGrabbed for alternating swings
                        ai1 = item.isBeingGrabbed ? 1f : -1f;
                        item.isBeingGrabbed = !item.isBeingGrabbed;
                    }
                    else if (ai1 == 0)
                    {
                        // Used to identify a charged attack
                        item.beingGrabbed = true;
                        charged = true;
                    }

                    if (Main.myPlayer == player.whoAmI)
                    {
                        // First frame of attack
                        Vector2 mouse = new Vector2(Main.screenPosition.X + Main.mouseX, Main.screenPosition.Y + Main.mouseY);
                        SetAttackRotation(player);
                        Projectile.NewProjectile(
                            player.MountedCenter,
                            (mouse - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0)),
                            slashProjectileID,
                            (int)(item.damage * player.meleeDamage),
                            item.scale,
                            player.whoAmI,
                            (int)(player.itemAnimationMax * slashDelay - player.itemAnimationMax), ai1);
                    }

                    // Set item time anyway, if not shoot, also make next slash upwards
                    if (item.shoot <= 0 && player.itemTime == 0)
                    { player.itemTime = item.useTime; item.isBeingGrabbed = false; }
                }

                item.useStyle = 0;
            }
            else
            {
                item.useStyle = 1;
                item.beingGrabbed = false;
            }

            // when counting down
            if (player.itemTime > 0)
            {

                // If not moving much, boost item charge speed
                int reduction = 1;
                if (Math.Abs(player.velocity.X) <= 1f)
                { reduction *= 2; }
                // If not grounded
                if (player.velocity.Y == 0)
                { reduction *= 2; }


                // Reset if swinging
                if (player.itemAnimation > 0) { player.itemTime = item.useTime; }
                else if (Main.myPlayer == player.whoAmI)
                {
                    for (int i = 0; i < reduction; i++)
                    {
                        // Charging dust
                        Vector2 vector = new Vector2(
                            Main.rand.Next(-2048, 2048) * (0.015f + 0.0003f * player.itemTime),
                            Main.rand.Next(-2048, 2048) * (0.015f + 0.0003f * player.itemTime));
                        Dust d = Main.dust[Dust.NewDust(
                            player.MountedCenter + vector, 1, 1,
                            45, -vector.X / 8, -vector.Y / 8, 255,
                            chargeColour, 1f)];
                        d.noLight = true;
                        d.noGravity = true;
                    }
                }


                // reduce
                player.itemTime -= (reduction - 1);

                // flash and correct
                if (player.itemTime <= 1)
                {
                    player.itemTime = 1;
                    PlayerFX.ItemFlashFX(player, 45, new PlayerFX.SoundData(25) { volumeScale = 0.5f });
                }
            }

            // HACK: allows the player to swing the sword when held on the mouse
            if (Main.mouseItem.type == item.type)
            {
                if (player.controlUseItem && player.itemAnimation == 0 && player.itemTime == 0 && player.releaseUseItem)
                { player.itemAnimation = 1; }
            }

            return charged;
        }

        /// <summary>
        /// For Shoot method, handles setting the shoot to true or not
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsChargedShot(Player player)
        {
            // Sync the projectile itemtime with the actual slash
            if (player.itemAnimation == player.itemAnimationMax - 1)
            { return true; }
            // Reset to 0 because Shoot is only called when itemtime is ready.
            // However we don't want it to try shooting in the middle of an animation
            // so this makes the next slash ready after the cooldown is done
            player.itemTime = 0;
            return false;
        }

        /// <summary> Some lazy copypasta'd code to set the item rotation on swing </summary>
        private static void SetAttackRotation(Player player, bool quiet = false)
        {
            // Get rotation at use
            if (Main.myPlayer == player.whoAmI)
            {
                Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
                Vector2 value = Vector2.UnitX.RotatedBy((double)player.fullRotation, default(Vector2));
                float num79 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
                float num80 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
                if (player.gravDir == -1f)
                {
                    num80 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector2.Y;
                }
                player.itemRotation = (float)Math.Atan2((double)(num80 * (float)player.direction), (double)(num79 * (float)player.direction)) - player.fullRotation;
            }

            if (Math.Abs(player.itemRotation) > Math.PI / 2) //swapping then doing it again because lazy and can't be bothered to find in code
            {
                player.direction *= -1;

                if (Main.myPlayer == player.whoAmI)
                {
                    Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
                    Vector2 value = Vector2.UnitX.RotatedBy((double)player.fullRotation, default(Vector2));
                    float num79 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
                    float num80 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
                    if (player.gravDir == -1f)
                    {
                        num80 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector2.Y;
                    }
                    player.itemRotation = (float)Math.Atan2((double)(num80 * (float)player.direction), (double)(num79 * (float)player.direction)) - player.fullRotation;
                }
            }

            if (!quiet && Main.netMode == 1 && Main.myPlayer == player.whoAmI)
            {
                NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                NetMessage.SendData(MessageID.ItemAnimation, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        /// <summary> Quick slash animation </summary>
        public static void  UseItemFrame(Player player, float delayStart = 0.9f, bool flip = false)
        {
            //counts down from 1 to 0
            float anim = player.itemAnimation / (float)(player.itemAnimationMax);
            int frames = player.itemAnimationMax - player.itemAnimation;

            // animation frames;
            int start, swing, swing2, end;

            if (flip)
            {
                start = 4 * player.bodyFrame.Height;
                swing = 3 * player.bodyFrame.Height;
                swing2 = 2 * player.bodyFrame.Height;
                end = 1 * player.bodyFrame.Height;
            }
            else
            {
                start = 1 * player.bodyFrame.Height;
                swing = 2 * player.bodyFrame.Height;
                swing2 = 3 * player.bodyFrame.Height;
                end = 4 * player.bodyFrame.Height;
            }

            // Actual animation
            if (delayStart < 0.4f) delayStart = 0.4f;
            if (anim > delayStart)
            {
                player.bodyFrame.Y = start;
            }
            else if (anim > delayStart - 0.1f)
            {
                player.bodyFrame.Y = swing;
            }
            else if (anim > delayStart - 0.2f)
            {
                player.bodyFrame.Y = swing2;
            }
            else
            {
                player.bodyFrame.Y = end;
            }
        }

        public static void UseItemHitboxCalculate(Player player, Item item, ref Rectangle hitbox, ref bool noHitbox, float delayStart, int height, int length, float magicHitNumber = 3)
        {
            height = (int)(height * item.scale);
            length = (int)(length * item.scale);
            int backOffset = Player.defaultWidth / 2; // dist from centre to edge
            int dist = Math.Max(0, length - height - Player.defaultWidth / 2); // total distance covered by the moving hitbox

            int startFrame = (int)(player.itemAnimationMax * delayStart);
            if (startFrame < magicHitNumber) startFrame = (int)magicHitNumber; //limit startframe

            int activeFrame = startFrame - player.itemAnimation;
            if (activeFrame >= 0 && activeFrame < magicHitNumber + 1)
            {
                hitbox.Width = (int)(height * 1.416f);
                hitbox.Height = (int)(height * 1.416f);

                float invert = 0f;
                if (player.direction < 0) invert = MathHelper.Pi;
                hitbox.Location = new Point(
                   // centre, cos by 3rd dist x frame, with backoffset to pull forward
                   (int)(player.MountedCenter.X + Math.Cos(player.itemRotation + invert)
                   * (dist / magicHitNumber * activeFrame + backOffset) - hitbox.Width / 2),
                   (int)(player.MountedCenter.Y + Math.Sin(player.itemRotation + invert)
                   * (dist / magicHitNumber * activeFrame + backOffset) - hitbox.Height / 2));

                player.attackCD = 0;

                // DEBUG hitbox
                //for (int i = 0; i < 50; i++)
                //{ Dust.NewDust(hitbox.Location.ToVector2(), hitbox.Width, hitbox.Height, 6, 0, 0, 0, default(Color), 0.5f); }
            }
            else
            {
                hitbox = player.Hitbox;
                noHitbox = true;
            }
        }
        
        public static void OnHitFX(Player player, Entity target, bool crit, Color colour)
        {
            Vector2 dir = (target.Center - player.MountedCenter).SafeNormalize(Vector2.Zero);
            Dust.NewDustPerfect(target.Center - dir * 30f,
                WeaponOut.DustIDSlashFX, dir * 15f, 0, colour, (crit ? 1.5f : 1f));
        }

        /// <summary>
        /// Client-side check for if the sabre's attack is charged
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sabre"></param>
        /// <returns></returns>
        public static bool SabreIsChargedStriking(Player player, Item sabre)
        {
            return player == Main.LocalPlayer && sabre.beingGrabbed;
        }


        //////////////////////
        // Projectile stuff //
        //////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="slashDirection"></param>
        /// <returns>true for normal slashes, false if a charged attack</returns>
        public static bool AINormalSlash(Projectile projectile, float slashDirection)
        {
            Player player = Main.player[projectile.owner];

            if (slashDirection == 1 || slashDirection == -1)
            {
                player.HeldItem.noGrabDelay = 0;
                NormalSlash(projectile, player);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Manages logic for a dash with immunity and camera lerp.
        /// </summary>
        /// <param name="dashFrameDuration">Number of frames to dash for</param>
        /// <param name="dashSpeed">SPeed of dash, eg. player.maxRunSpeed * 5f</param>
        /// <param name="freezeFrame">The frame to freeze at eg. 2</param>
        /// <param name="dashEndVelocity">The ending velocity eg. preDashVelocity</param>
        /// <returns>True if currently in the dash</returns>
        public static bool AIDashSlash(Player player, Projectile projectile, float dashFrameDuration, float dashSpeed, int freezeFrame, ref Vector2 dashEndVelocity)
        {
            bool a = false; ;
            if ((int)projectile.ai[0] < dashFrameDuration)
            {
                // Fine-tuned tilecollision
                for(int i = 0; i < 4; i++)
                {
                    player.position += Collision.TileCollision(player.position, projectile.velocity * dashSpeed / 4,
                        player.width, player.height, false, false, (int)player.gravDir);
                }
                player.velocity *= 0.1f;
                player.armorEffectDrawShadow = true;

                // Prolong mid-slash
                RecentreSlash(projectile, player);
                if (player.itemAnimation <= player.itemAnimationMax - freezeFrame)
                { player.itemAnimation = player.itemAnimationMax - freezeFrame; }

                // Set immunities
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 6);
                player.immuneNoBlink = true;
                //player.fallStart = (int)(player.position.Y / 16f);
                //player.fallStart2 = player.fallStart;

                a = true;
            }
            else if ((int)projectile.ai[0] >= dashFrameDuration && dashEndVelocity != new Vector2(float.MinValue, float.MinValue))
            {
                player.velocity = dashEndVelocity;
                dashEndVelocity = new Vector2(float.MinValue, float.MinValue);
            }

            // Trigger lerp by offsetting camera
            if (projectile.timeLeft == 59)
            {
                Main.SetCameraLerp(0.1f, 10);
                Main.screenPosition -= projectile.velocity * 2;
            }
            return a;
        }

        /// <summary>
        /// Does player.HeldItem. noGrabDelay and isBeingGrabbed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="chargeSlashDirection">Set the charge direction. </param>
        public static void AISetChargeSlashVariables(Player player, int chargeSlashDirection)
        {
            player.HeldItem.noGrabDelay = player.itemAnimation;
            player.HeldItem.isBeingGrabbed = chargeSlashDirection < 0;
        }

        const float HALFPI = (float)(Math.PI / 2);
        public static void NormalSlash(Projectile projectile, Player player)
        {
            if (projectile.ai[0] <= 0f)
            {
                projectile.rotation = (float)System.Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
                if (Math.Abs(projectile.rotation) == HALFPI)
                { projectile.spriteDirection = player.direction; }
                else
                { projectile.spriteDirection = Math.Abs(projectile.rotation) <= HALFPI ? 1 : -1; }

                RecentreSlash(projectile, player);
            }
            else
            {
                projectile.position -= projectile.velocity * 2;
            }

            projectile.frame = (int)projectile.ai[0];
            if (projectile.frame >= Main.projFrames[projectile.type] && player.itemAnimation < 2)
            {
                projectile.timeLeft = 0;
            }

            projectile.scale = projectile.knockBack;
        }

        public static void RecentreSlash(Projectile projectile, Player player)
        {
            // Centre the projectile on player
            projectile.Center = player.MountedCenter;
            if (player.direction < 0) projectile.position.X += projectile.width;

            // move to intended side, then pull back to player width
            Vector2 offset = new Vector2(
               (float)System.Math.Cos(projectile.rotation) * ((projectile.width / 2) - Player.defaultWidth / projectile.scale),
               (float)System.Math.Sin(projectile.rotation) * ((projectile.width / 2) - Player.defaultWidth / projectile.scale)
                );

            if (player.direction < 0) projectile.position.X -= projectile.width;
        }

        public static bool PreDrawSlashAndWeapon(SpriteBatch spriteBatch, Projectile projectile, int weaponItemID, Color weaponColor, Texture2D slashTexture, Color slashColor, int slashFramecount, float slashDirection = 1f)
        {
            Player player = Main.player[projectile.owner];
            Texture2D weapon = Main.itemTexture[weaponItemID];
            if (slashTexture == null)
            {
                slashTexture = Main.projectileTexture[projectile.type];
                slashFramecount = Main.projFrames[projectile.type];
            }

            float slashNormal = MathHelper.Clamp(slashDirection, -1, 1);

            // Flip Horziontally
            SpriteEffects spriteEffect = SpriteEffects.None;
            spriteEffect = SpriteEffects.None;
            if (projectile.spriteDirection < 0)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
            }

            // Flip Vertically
            float vDir = slashNormal;
            Vector2 weaponOrigin;
            if (vDir <= 0)
            {
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                {
                    weaponOrigin = weapon.Bounds.TopRight();
                    spriteEffect = spriteEffect | SpriteEffects.FlipVertically;
                }
                else
                {
                    weaponOrigin = weapon.Bounds.TopLeft();
                    spriteEffect = SpriteEffects.FlipVertically;
                }
            }
            else
            {
                weaponOrigin = weapon.Bounds.BottomLeft();
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                { weaponOrigin = weapon.Bounds.BottomRight(); }
            }

            // Draw weapon if at the start or end animation
            if (projectile.frame > 0 && (
                player.bodyFrame.Y == 1 * player.bodyFrame.Height ||
                player.bodyFrame.Y == 4 * player.bodyFrame.Height))
            {
                spriteBatch.Draw(weapon,
                    player.MountedCenter - Main.screenPosition + new Vector2(0f, 8f * player.gravDir * slashNormal),
                    weapon.Frame(1, 1, 0, 0),
                    weaponColor,
                    player.itemRotation + 3.26f * slashNormal * projectile.spriteDirection,
                    weaponOrigin,
                    projectile.scale,
                    spriteEffect,
                    1f);
            }

            if (projectile.frame >= 0 &&
                projectile.frame < slashFramecount)
            {
                // Draw slash
                spriteBatch.Draw(slashTexture,
                    projectile.Center - Main.screenPosition,
                    slashTexture.Frame(1, slashFramecount, 0, projectile.frame),
                    slashColor,
                    player.itemRotation,
                    new Vector2(slashTexture.Width / 2, slashTexture.Height / (2 * slashFramecount)),
                    projectile.scale,
                    spriteEffect,
                    1f);
            }
            return false;
        }

    }
}
