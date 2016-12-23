using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

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
        public int punchComboMax2
        {
            get { return punchComboMax; }
        }
        public int punchCombo; //keeps track of every successful punch
        public int punchCount; //keeps track of every punch thrown
        public bool isDramatic { get { return punchCount % punchComboMax2 == 0; } }

        public FistStyle(Item item, int maxCombo = 0)
        {
            this.item = item;
            punchComboMax = maxCombo;
            punchCombo = 0;
        }
        
        public void UseItemFrameComboStop(Player player)
        {
            //Main.NewText(player.itemAnimation + " | " + player.itemTime);
            if (player.itemAnimation == 0)
            {
                if (punchCombo != 0) comboReset(player);
            }
            else
            {
                if (player.itemAnimation == player.itemAnimationMax - 1) punchCount++;
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
        /// <returns></returns>
        public bool OnHitNPC(Player player, NPC target, bool follow = false)
        {
            if (target.immortal) return false; //don't trigger on dummy

            //C-C-Combo!
            punchCombo++;
            Rectangle rect = player.getRect();
            if (!isDramatic) rect.Y += (int)(rect.Height * player.gravDir);
            CombatText.NewText(rect,
                comboColour, string.Concat(punchCombo), isDramatic);
            if (punchCount % punchComboMax2 != 0)
            {
                player.itemAnimation = 2 * player.itemAnimation / 3;
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax);

                if (target.life > 0 && follow)
                {
                    //follow target
                    player.velocity = target.velocity;
                    if (target.noGravity) player.velocity.Y -= player.gravDir * 4f;
                }
            }
            if (punchCount % punchComboMax2 == 0)
            {
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax);
                //disengage
                if (follow) player.velocity += new Vector2(target.velocity.X * -2, target.velocity.Y * 2);
            }
            return false;
        }

        public static void UseItemFramePauseCharge(Player player, Item item)
        {
            // Ignore freeze timer in hand (also, using fists inhand - hahah get it)
            if (player.selectedItem == 58) return;
            // Stop charging charge shot
            if (player.itemTime < item.useTime - 1) //if less than max
            {
                player.itemTime = item.useTime - 1; //freeze at at this point until player stops attacking
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

        /// <summary>
        /// Generates a hitbox
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hitbox"></param>
        /// <param name="boxSize"></param>
        /// <returns>True if no hitbox (so no dust)</returns>
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
            if (anim > 0.8f || anim <= 0.6f)
            {
                hitbox = new Rectangle();
                return true;
            }

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
