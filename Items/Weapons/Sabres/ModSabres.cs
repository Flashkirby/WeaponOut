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
        private static byte internalChargeTicker;

        /// <summary> 
        /// Charge item time faster when not moving, or grounded (or both!)
        /// Also handles the item.useStyle to allow for custom animation
        /// </summary>
        /// <param name="slashDelay">Delay before slash appears, as a ratio (default 0.9f) of the total attack time (1f - no delay to 0f - start at the end)</param>
        /// <param name="ai1">Set ai1, usually the direction of the slash, or power attack. </param>
        /// <param name="customCharge">Custom function call replacing normal charge effect, using player.itemTime; </param>
        /// <returns>True on the frame of a charged attack</returns>
        public static bool HoldItemManager(Player player, Item item, int slashProjectileID, Color chargeColour = default(Color), float slashDelay = 0.9f, float ai1 = 1f, Action<Player, bool> customCharge = null, int delaySpeed = 4)
        {
            bool charged = false;
            // Attacking
            if (player.itemAnimation > 0)
            {
                // JUST attacked
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
                        Vector2 velocity = (mouse - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0));
                        int p = Projectile.NewProjectile(
                            player.MountedCenter,
                            velocity,
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
                // internalChargeTicker hangs the item time until past the delaySpeed
                // in which case it allows the itemTime to be reduced on that frame.

                // If not moving much, boost item charge speed
                //int delaySpeed = 4; // default, item charged 1 1 / 3 speed

                // If grounded, half the dleay speed
                if (player.velocity.Y == 0)
                { delaySpeed /= 2; }

                // cap
                delaySpeed = Math.Max(delaySpeed, 1);

                // Reset if swinging
                if (player.itemAnimation > 0) { player.itemTime = Math.Max(player.itemTime, item.useTime); }
                else if (Main.myPlayer == player.whoAmI)
                {
                    if (customCharge != null)
                    {
                        customCharge(player, false);
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            // Charging dust
                            Vector2 vector = new Vector2(
                                Main.rand.Next(-2048, 2048) * (0.003f * player.itemTime) - 4,
                                Main.rand.Next(-2048, 2048) * (0.003f * player.itemTime) - 4);
                            Dust d = Main.dust[Dust.NewDust(
                                player.MountedCenter + vector, 1, 1,
                                45, 0, 0, 255,
                                chargeColour, 1.5f)];
                            d.velocity = -vector / 16;
                            d.velocity -= player.velocity / 8;
                            d.noLight = true;
                            d.noGravity = true;
                        }
                    }
                }

                // allow item time when past "limit"

                if (internalChargeTicker >= delaySpeed)
                { internalChargeTicker = 0; }

                // delay item time unless at 0
                if (internalChargeTicker > 0)
                { player.itemTime++; }
                internalChargeTicker++;

                // flash and correct
                if (player.itemTime <= 1)
                {
                    player.itemTime = 1;
                    if (customCharge != null)
                    { customCharge(player, true); }
                    else
                    { PlayerFX.ItemFlashFX(player, 45, new PlayerFX.SoundData(25) { volumeScale = 0.5f }); }
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

        public static void UseItemHitboxCalculate(Player player, Item item, ref Rectangle hitbox, ref bool noHitbox, float delayStart, int height, int length, float hitboxDuration = 3)
        {
            // Lengthen the hitbox duration the longer it is
            // 1 Frame per 4 tiles after 12
            hitboxDuration += Math.Max(0, (length - 96) / 32);

            height = (int)(height * item.scale);
            length = (int)(length * item.scale);

            // Define when after first swinging the the hitbox becomes active
            int startFrame = (int)(player.itemAnimationMax * delayStart);

            // For faster attacks, the start frame must be at least the magic number
            if (startFrame < hitboxDuration) startFrame = (int)hitboxDuration;

            int activeFrame = startFrame - player.itemAnimation;
            if (activeFrame >= 0 && activeFrame < hitboxDuration + 1)
            {
                hitbox.Width = height;
                hitbox.Height = hitbox.Width;

                float invert = 0f;
                if (player.direction < 0) invert = MathHelper.Pi;
                float dist = Math.Max(0, length - height); // total distance covered by the moving hitbox

                Vector2 direction = new Vector2(
                    (float)Math.Cos(player.itemRotation + invert),
                    (float)Math.Sin(player.itemRotation + invert));
                Vector2 centre = player.MountedCenter - (hitbox.Size() / 2);
                Vector2 playerOffset = (player.Size.X * item.scale * direction);
                hitbox.Location = (centre
                    + direction * hitbox.Width / 2
                    - playerOffset
                    + (dist * direction / hitboxDuration * activeFrame)
                    ).ToPoint();

                player.attackCD = 0;

                // DEBUG hitbox
                for (int i = 0; i < 256; i++)
                { Dust d = Dust.NewDustDirect(hitbox.Location.ToVector2() - new Vector2(2, 2), hitbox.Width, hitbox.Height, 60, 0, 0, 0, default(Color), 0.75f); d.velocity = Vector2.Zero; d.noGravity = true; }
            }
            else
            {
                hitbox = player.Hitbox;
                noHitbox = true;
            }
        }
        
        public static void OnHitFX(Player player, Entity target, bool crit, Color colour, bool glow = false)
        {
            Vector2 source = player.MountedCenter + new Vector2(
                Main.rand.NextFloatDirection() * 16f,
                Main.rand.NextFloatDirection() * 16f
                );
            Vector2 dir = (target.Center - source).SafeNormalize(Vector2.Zero);
            Dust d = Dust.NewDustPerfect(target.Center - dir * 30f,
                WeaponOut.DustIDSlashFX, dir * 15f, 0, colour, (crit ? 1.5f : 1f));
            d.noLight = glow;
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
            if (player.dead || !player.active)
            {
                projectile.timeLeft = 0;
                return false;
            }

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
        /// Also takes over projectile.frame, so call this after a normal slash ai.
        /// </summary>
        /// <param name="dashFrameDuration">Number of frames to dash for</param>
        /// <param name="dashSpeed">SPeed of dash, eg. player.maxRunSpeed * 5f</param>
        /// <param name="freezeFrame">The frame to freeze at eg. 2</param>
        /// <param name="dashEndVelocity">The ending velocity, or null to use the dash velocity eg. preDashVelocity</param>
        /// <returns>True if currently in the dash</returns>
        public static bool AIDashSlash(Player player, Projectile projectile, float dashFrameDuration, float dashSpeed, int freezeFrame, ref Vector2? dashEndVelocity)
        {
            if (player.dead || !player.active)
            {
                projectile.timeLeft = 0;
                return false;
            }
            if (freezeFrame < 1) freezeFrame = 1;

            bool dashing = false;
            if ((int)projectile.ai[0] < dashFrameDuration)
            {
                // Fine-tuned tilecollision
                player.armorEffectDrawShadow = true;
                Vector2 projVel = projectile.velocity;
                if (player.gravDir < 0) projVel.Y = -projVel.Y;
                for (int i = 0; i < 4; i++)
                {
                    player.position += Collision.TileCollision(player.position, projVel * dashSpeed / 4,
                        player.width, player.height, false, false, (int)player.gravDir);
                }

                if (player.velocity.Y == 0)
                { player.velocity = new Vector2(0, (projectile.velocity * dashSpeed).Y); }
                else
                { player.velocity = new Vector2(0, player.gravDir * player.gravity); }

                // Prolong mid-slash player animation
                RecentreSlash(projectile, player);
                if (player.itemAnimation <= player.itemAnimationMax - freezeFrame)
                { player.itemAnimation = player.itemAnimationMax - freezeFrame; }

                // Set immunities
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 6);
                player.immuneNoBlink = true;

                dashing = true;
            }
            else if ((int)projectile.ai[0] >= dashFrameDuration && dashEndVelocity != new Vector2(float.MinValue, float.MinValue))
            {
                if (dashEndVelocity == null)
                {
                    Vector2 projVel = projectile.velocity.SafeNormalize(Vector2.Zero);
                    if (player.gravDir < 0) projVel.Y = -projVel.Y;
                    float speed = dashSpeed / 4f;
                    if (speed < player.maxFallSpeed)
                    { player.velocity = projVel * speed; }
                    else
                    { player.velocity = projVel * player.maxFallSpeed; }

                    // Reset fall damage
                    player.fallStart = (int)(player.position.Y / 16f);
                    player.fallStart2 = player.fallStart;
                }
                else
                {
                    player.velocity = (Vector2)dashEndVelocity;
                }

                // Set the vector to a "reset" state
                dashEndVelocity = new Vector2(float.MinValue, float.MinValue);
            }

            // Trigger lerp by offsetting camera
            if (projectile.timeLeft == 60)
            {
                Main.SetCameraLerp(0.1f, 10);
                Main.screenPosition -= projectile.velocity * 2;
            }

            // Set new projectile frame
            projectile.frame = (int)Math.Max(0, projectile.ai[0] - dashFrameDuration);

            return dashing;
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
                projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
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
               (float)Math.Cos(projectile.rotation) * ((projectile.width / 2) - player.Size.X / projectile.scale),
               (float)Math.Sin(projectile.rotation) * ((projectile.width / 2) - player.Size.X / projectile.scale)
                );
            if (player.gravDir < 0) offset.Y = -offset.Y;
            projectile.position += offset;

            if (player.direction < 0) projectile.position.X -= projectile.width;
        }

        public static bool PreDrawSlashAndWeapon(SpriteBatch spriteBatch, Projectile projectile, int weaponItemID, Color weaponColor, Texture2D slashTexture, Color slashColor, int slashFramecount, float slashDirection = 1f, bool shadow = false)
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
                if(shadow)
                {
                    // Draw slashes
                    for (int i = 1; i <= 6; i++)
                    {
                        int iter = i / 2;
                        float itef = i / 2f;

                        if (projectile.frame + iter >= slashFramecount) break;
                        spriteBatch.Draw(slashTexture,
                            projectile.Center - (player.position - player.oldPosition) * itef - Main.screenPosition,
                            slashTexture.Frame(1, slashFramecount, 0, projectile.frame + iter),
                            slashColor * (0.5f - 0.1f * itef),
                            player.itemRotation,
                            new Vector2(slashTexture.Width / 2, slashTexture.Height / (2 * slashFramecount)),
                            projectile.scale,
                            spriteEffect,
                            1f);
                    }
                }

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
