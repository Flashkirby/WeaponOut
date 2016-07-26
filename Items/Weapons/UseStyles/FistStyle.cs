using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Items.Weapons.UseStyles
{
    public static class FistStyle
    {
        public static void UseItemFrame(Player player)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            //wind up animation
            if (anim > 0.9f) player.bodyFrame.Y = player.bodyFrame.Height * 10; 
            else if (anim > 0.7f)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 17;
                provideImmunity(player);
            }
            //punch (gives invulnerability)
            else if (anim > 0.3f)
            {
                if (Math.Abs(player.itemRotation) > Math.PI / 8 && Math.Abs(player.itemRotation) < 7 * Math.PI / 8)
                {
                    if (player.itemRotation * player.direction * player.gravDir > 0)
                    {
                        //Down low
                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                    }
                    else
                    {
                        //Up high
                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                    }
                }
                else
                {
                    //along the middle
                    player.bodyFrame.Y = player.bodyFrame.Height * 3;
                }
                provideImmunity(player);
            }
            //wind back
            else player.bodyFrame.Y = player.bodyFrame.Height * 17;
        }

        public static bool UseItemHitbox(Player player, ref Rectangle hitbox, int boxSize)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;
            //get rotation at use
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
                    Vector2 value = Vector2.UnitX.RotatedBy((double)player.fullRotation, default(Vector2));
                    Vector2 vector3 = Main.MouseWorld - vector2;
                    if (vector3 != Vector2.Zero)
                    {
                        vector3.Normalize();
                    }
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
                        Vector2 vector3 = Main.MouseWorld - vector2;
                        if (vector3 != Vector2.Zero)
                        {
                            vector3.Normalize();
                        }
                        float num79 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
                        float num80 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
                        if (player.gravDir == -1f)
                        {
                            num80 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector2.Y;
                        }
                        player.itemRotation = (float)Math.Atan2((double)(num80 * (float)player.direction), (double)(num79 * (float)player.direction)) - player.fullRotation;
                    }
                }

                if (Main.myPlayer == player.whoAmI)
                {
                    NetMessage.SendData(13, -1, -1, "", player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    NetMessage.SendData(41, -1, -1, "", player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }
            //no hitbox during winding
            hitbox = new Rectangle();
            if (anim > 0.8f || anim <= 0.6f) return true;

            //set player direction/hitbox
            Vector2 centre = new Vector2();
            float swing = 1 - (anim - 0.6f) * 5f;//0.8 -> 0.6
            if (Math.Abs(player.itemRotation) > Math.PI / 8 && Math.Abs(player.itemRotation) < 7 * Math.PI / 8)
            {
                if (player.itemRotation * player.direction > 0)
                {
                    //Up high
                    centre = new Vector2(
                        player.Center.X - (player.width * 0.4f * player.direction) + player.width * 1.1f * swing * player.direction,
                        player.Center.Y + (player.height * 0.7f * swing));
                }
                else
                {
                    //Down low
                    centre = new Vector2(
                        player.Center.X - (player.width * 0.4f * player.direction) + player.width * 1.1f * swing * player.direction,
                        player.Center.Y - (player.height * 0.7f * swing));
                }
            }
            else
            {
                //along the middle
                centre = new Vector2(
                    player.Center.X - (player.width * 0.5f * player.direction) + player.width * 1.6f * swing * player.direction, 
                    player.Center.Y);
            }
            hitbox.X = (int)centre.X - boxSize/2;
            hitbox.Y = (int)centre.Y - boxSize/2;
            hitbox.Width = boxSize;
            hitbox.Height = boxSize;

            if (player.direction > 0)
            {
                hitbox.X += player.width / 2;
            }
            else
            {
                hitbox.X -= player.width / 2;
            }

            return false;
        }

        public static Vector2 GetFistVelocity(Player player)
        {
            if (Math.Abs(player.itemRotation) > Math.PI / 8 && Math.Abs(player.itemRotation) < 7 * Math.PI / 8)
            {
                if (player.itemRotation * player.direction > 0)
                {
                    //Up high
                    return new Vector2(player.direction * 0.7f, 0.7f);
                }
                else
                {
                    //down low
                    return new Vector2(player.direction * 0.7f, -0.7f);
                }
            }
            //along the middle
            return new Vector2(player.direction, -0.1f);
        }

        public static void provideImmunity(Player player)
        {
            provideImmunity(player, 1);
        }
        public static void provideImmunity(Player player, int immune)
        {
            player.immune = true;
            if (player.immuneTime <= immune)
            {
                player.immuneTime = immune;
                player.immuneAlpha = 0;
                player.immuneAlphaDirection = -1;
            }
        }
    }
}
