using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.World.Generation;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.UseStyles
{
    public class FistStyle
    {
        public const int useStyle = 102115116; //http://www.unit-conversion.info/texttools/ascii/ with fst to ASCII numbers
        private static Color highColour = Color.Cyan;
        private static Color lowColour = Color.DarkCyan;
        private Color comboColour
        {
            get
            {
                if (isDramatic) return highColour;
                return lowColour;
            }
        }

        Item item;
        public int punchComboMax;
        public int punchCombo; //keeps track of every successful punch
        public int punchCount; //keeps track of every punch thrown
        public bool isDramatic { get { return punchCount % punchComboMax == 0 && punchCount > 0; } }

        public FistStyle(Item item, int maxCombo = 0)
        {
            this.item = item;
            punchComboMax = maxCombo;
            punchCombo = 0;
        }

        private bool incrementCountInHitFirst = false;
        public void UseItemFrameComboStop(Player player)
        {
            //Main.NewText(player.itemAnimation + " | " + player.itemTime);
            if (player.itemAnimation == 0)
            {
                // Reset if no hit during swing
                if (punchCombo != 0)
                {
                    comboReset(player);
                }
            }
            else
            {
                // Increase count per punch, but only if method didn't already
                if (player.itemAnimation == player.itemAnimationMax - 1 && !incrementCountInHitFirst)
                {
                    punchCount++;
                }
                incrementCountInHitFirst = false;

                // Reset if no hit during swing (autoswinger)
                if (player.itemAnimation == 1 && punchCount > punchCombo)
                {
                    comboReset(player);
                }
            }
            //Main.NewText(punchCombo + " / " + punchCount);
        }
        private void comboReset(Player player)
        {
            if (punchCombo > 2)
            {
                CombatText.NewText(player.getRect(),
                    highColour, punchCombo + " hit", false, false);
            }
            //Main.NewText("punch reset");
            punchCombo = 0;
            punchCount = 0;
        }

        /// <summary>
        /// Hits the NPC and generates client message, and returns combo completion
        /// </summary>
        /// <param name="player"></param>
        /// <param name="target"></param>
        /// <returns>Hit number, or -1</returns>
        public int OnHitNPC(Player player, NPC target, bool follow = false)
        {
           // if (target.immortal) return -1; //don't trigger on dummy TODO: remember to enable before publish

            //C-C-Combo!
            punchCombo++;

            // Check to keep up, will call before other increase if hit on first frame
            if (punchCombo > punchCount)
            {
                if (player.itemAnimation == player.itemAnimationMax - 1) incrementCountInHitFirst = true;
                punchCount++;
            }

            Rectangle rect = player.getRect();
            if (!isDramatic) rect.Y += (int)(rect.Height * player.gravDir);
            CombatText.NewText(rect,
                comboColour, string.Concat(punchCombo), isDramatic);
            if (!isDramatic)
            {

                player.itemAnimation = 2 * player.itemAnimation / 3;
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax);

                if (target.life > 0 && follow)
                {
                    // Check if NPC is in air
                    bool aerial = target.noGravity;
                    Point origin = target.Center.ToTileCoordinates();
                    Point point;
                    if (!WorldUtils.Find(origin, Searches.Chain(new Searches.Down(4), new GenCondition[]
                    {
                        new Conditions.IsSolid()
                    }), out point))
                    {
                        aerial = true;
                    }

                    //follow target
                    player.velocity = target.velocity;
                    if (aerial) player.velocity.Y -= player.gravDir * 4f;
                }
            }
            else
            {
                // Grant extra immune whilst disengaging
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax);

                //disengage
                if (follow) player.velocity += new Vector2(player.direction * -3f + target.velocity.X * -1.5f, player.gravDir * -2f + target.velocity.Y * 2);
            }

            return punchCombo;
        }

        public void ModifyTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            tooltips.Add(new TooltipLine(mod, "comboPower", punchComboMax + " combo power"));
        }

        public int ExpendCombo(Player player, bool dontConsumeCombo = false)
        {
            if(punchCount >= punchComboMax)
            {
                int charge = punchCount / punchComboMax;
                if (!dontConsumeCombo) comboReset(player);
                return charge;
            }
            else
            {
                return 0;
            }
        }

        public static void UseItemFrame(Player player)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            //wind up animation
            if (anim > 0.9f) player.bodyFrame.Y = player.bodyFrame.Height * 10; 
            else if (anim > 0.7f)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 17;
            }
            //punch
            else if (anim > 0.3f)
            {
                if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
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
            }
            //wind back
            else player.bodyFrame.Y = player.bodyFrame.Height * 17;
        }

        public static bool UseItemHitbox(Player player, ref Rectangle hitbox, int distance)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            if (anim > 0.3f)
            {
                // Provide immunity for 2/3
                provideImmunity(player);

                //get rotation at use
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    #region Attack Rotation
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

                        NetMessage.SendData(MessageID.PlayerControls, -1, -1, "", player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                        NetMessage.SendData(MessageID.ItemAnimation, -1, -1, "", player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    }
                    #endregion
                }

                // Starts as player box
                if (anim > 0.7f)
                {
                    hitbox = player.getRect();
                }
                // Moves outwards to direction
                else
                {
                    // Size is relative to distance past default size
                    hitbox.Width = Player.defaultWidth + (int)(distance - 14);
                    hitbox.Height = hitbox.Width;

                    // Work out which way to go
                    float xDir = player.direction;
                    float yDir = 1.5f;

                    if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
                    {
                        xDir /= 2;
                        if (player.itemRotation * player.direction > 0)
                        {
                            //Up high
                            yDir *= 1f;
                        }
                        else
                        {
                            //Down low
                            yDir *= -1f;
                        }
                    }
                    else
                    {
                        //along the middle
                        yDir = 0;
                    }

                    hitbox.Location = (player.Center + new Vector2(
                         xDir * distance - (hitbox.Width / 2),
                         yDir * distance - (hitbox.Height / 2)
                        )).ToPoint();
                }
            }
            else
            {
                // No hitbox during last third
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a fisticuffs rectangle
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hitbox"></param>
        /// <param name="boxSize"></param>
        /// <returns>True if no hitbox (so no dust)</returns>
        public static Rectangle UseItemGraphicbox(Player player, int boxSize)
        {
            Rectangle box = new Rectangle();
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            //no show during winding
            if (anim > 0.8f || anim <= 0.6f)
            {
                return new Rectangle();
            }

            //set player direction/hitbox
            Vector2 centre = new Vector2();
            float swing = 1 - (anim - 0.6f) * 5f;//0.8 -> 0.6
            if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
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
            box.X = (int)centre.X - boxSize/2;
            box.Y = (int)centre.Y - boxSize/2;
            box.Width = boxSize;
            box.Height = boxSize;

            if (player.direction > 0)
            {
                box.X += player.width / 2;
            }
            else
            {
                box.X -= player.width / 2;
            }

            return box;
        }

        public static Vector2 GetFistVelocity(Player player)
        {
            if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
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
