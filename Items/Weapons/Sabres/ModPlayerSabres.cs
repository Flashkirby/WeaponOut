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
    /// </summary>
    public static class ModPlayerSabres
    {

        /// <summary> 
        /// Charge item time faster when not moving, or grounded (or both!)
        /// Also handles the item.useStyle to allow for custom animation
        /// </summary>
        public static void HoldItemManager(Player player, Item item, int slashProjectileID, int dust = 45, float ai1 = 1f)
        {
            if(player.itemAnimation > 0)
            {
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    if (ai1 == 1f || ai1 == -1f)
                    {
                        ai1 = item.isBeingGrabbed ? 1f : -1f;
                        item.isBeingGrabbed = !item.isBeingGrabbed;
                    }

                    // First frame of attack
                    Vector2 mouse = new Vector2(Main.screenPosition.X + Main.mouseX, Main.screenPosition.Y + Main.mouseY);
                    SetAttackRotation(player);
                    Projectile.NewProjectile(
                        player.Center,
                        (mouse - player.Center).SafeNormalize(new Vector2(player.direction, 0)),
                        slashProjectileID,
                        (int)(item.damage * player.meleeDamage),
                        item.scale,
                        player.whoAmI,
                        0f, ai1);
                }

                item.useStyle = 0;
            }
            else
            {
                item.useStyle = 1;
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
                else
                {
                    for (int i = 0; i < reduction; i++)
                    {
                        // Charging dust
                        Vector2 vector = player.Center;
                        vector.X += (float)Main.rand.Next(-2048, 2048) * (0.015f + 0.0003f * player.itemTime);
                        vector.Y += (float)Main.rand.Next(-2048, 2048) * (0.015f + 0.0003f * player.itemTime);
                        Dust d = Main.dust[Dust.NewDust(
                            vector, 1, 1,
                            235, 0f, 0f, 0,
                            Color.White, 1f)];

                        d.velocity *= 0f;
                        d.scale = Main.rand.Next(70, 85) * 0.01f;
                        // This dust uses fadeIn for homing into players
                        d.fadeIn = player.whoAmI + 1;
                    }
                }


                // reduce
                player.itemTime -= (reduction - 1);

                // flash and correct
                if (player.itemTime <= 1)
                {
                    player.itemTime = 1;
                    PlayerFX.ItemFlashFX(player, 45);
                }
            }
        }

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
        private static void SetAttackRotation(Player player)
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
    }
}
