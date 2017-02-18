using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.GameInput;
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

        // Keep track of attacks initiated with controlDown
        public ushort specialMove = 0;

        public FistStyle(Item item, int maxCombo = 0)
        {
            this.item = item;
            punchComboMax = maxCombo;
            punchCombo = 0;
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

            // Display combo counter
            Rectangle rect = player.getRect();
            if (!isDramatic) rect.Y += (int)(rect.Height * player.gravDir);
            CombatText.NewText(rect,
                comboColour, string.Concat(punchCombo), isDramatic);

            // Manage player combo movement
            if (specialMove == 0)
            {
                #region Normal Punch
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
                    if (follow) player.velocity += new Vector2(player.direction * -2f + target.velocity.X * -1.5f, player.gravDir * -2f + target.velocity.Y * 2);
                }
                #endregion
            }
            else if (specialMove == 2)
            {
                // Grant extra immune whilst disengaging
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax / 2);

                //disengage
                int direction = 1;
                if (player.Center.X < target.Center.X) direction = -1;
                player.velocity = new Vector2(direction * 3f, player.gravDir * -1f);
            }

            return punchCombo;
        }

        public void ModifyTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            tooltips.Add(new TooltipLine(mod, "comboPower", punchComboMax + " combo power"));
        }

        public int ExpendCombo(Player player, bool dontConsumeCombo = false)
        {
            if (!dontConsumeCombo && player.itemAnimation > (item.autoReuse ? 1 : 0)) return 0;

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

        #region UseItem
        public void UseItemFrame(Player player)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            if (specialMove == 0)
            {
                #region Normal Punch
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
                #endregion
            }
            else
            {
                #region Special
                if (specialMove == 1)
                {
                    if (anim > 0.8)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                    }
                    else if (anim > 0.6)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 3;
                    }
                    else if (anim > 0.2)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                    }
                    else
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 17;
                    }
                }
                else if (specialMove == 2)
                {
                    if (anim > 0.6)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 1;
                    }
                    else if (anim > 0.5)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                    }
                    else if (anim > 0.2)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                    }
                    else
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 17;
                    }
                }
                #endregion
            }

            UseItemComboStop(player);
        }

        private bool incrementCountInHitFirst = false;
        private void UseItemComboStop(Player player)
        {
            //Main.NewText(player.itemAnimation + " | " + player.itemTime);
            // Increase count per punch, but only if method didn't already
            if (player.itemAnimation == player.itemAnimationMax - 1 && !incrementCountInHitFirst)
            {
                punchCount++;
            }
            incrementCountInHitFirst = false;

            // Reset frame
            if (player.itemAnimation == 1)
            {
                // Stop special punch move
                specialMove = 0;

                // Reset if no hit during swing (autoswinger)
                if (punchCount > punchCombo)
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

        public bool UseItemHitbox(Player player, ref Rectangle hitbox, int distance, float jumpSpeed, float fallSpeedX = 8f, float fallSpeedY = 8f)
        {
            // Special attack assign on Start frame
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                if (player.controlDown && jumpSpeed > 0)
                {
                    if (player.velocity.Y == 0 && player.oldVelocity.Y == 0)
                    {
                        specialMove = 1;
                        player.itemRotation = -(float)(player.direction * Math.PI / 2);
                        player.velocity.Y = -jumpSpeed * player.gravDir;
                    }
                    else
                    {
                        specialMove = 2;
                        player.itemRotation = (float)(player.direction * Math.PI / 2);
                        player.velocity.X = player.direction * ((player.velocity.X + fallSpeedX * 5) / 6);
                        if(player.gravDir > 0)
                        {
                            if (player.velocity.Y < fallSpeedY) player.velocity.Y = fallSpeedY;
                        }
                        else
                        {
                            if (player.velocity.Y > -fallSpeedY) player.velocity.Y = -fallSpeedY;
                        }
                        player.jump = 0;
                    }
                }
                else
                {
                    //get rotation at use
                    #region Attack Rotation
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
                    #endregion
                }

                if (Main.myPlayer == player.whoAmI)
                {

                    NetMessage.SendData(MessageID.PlayerControls, -1, -1, "", player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    NetMessage.SendData(MessageID.ItemAnimation, -1, -1, "", player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            hitbox = new Rectangle();
            if (specialMove == 0)
            {
                // Calculate hitbox for normal punch
                #region Standard Punch
                if (anim > 0.3f)
                {
                    // Provide immunity for 2/3
                    provideImmunity(player);

                    // Start as player hitbox
                    if (anim > 0.7f)
                    {
                        hitbox = player.getRect();
                    }
                    // Move to pointed direction
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
                #endregion
            }
            else
            {
                // Calculate hitbox for rising/falling
                #region Special Attack
                if (anim > 0.2f)
                {
                    // Provide immunity for 2/3
                    provideImmunity(player);

                    // Size is relative to distance past default size
                    hitbox.Width = Player.defaultWidth + distance;
                    hitbox.Height = Player.defaultHeight + distance;

                    // Work out which way to go
                    if (player.direction < 0)
                    {
                        hitbox.Location = (player.Right - new Vector2(hitbox.Width, hitbox.Height / 2f)).ToPoint();
                    }
                    else
                    {
                        hitbox.Location = (player.Left - new Vector2(0, hitbox.Height / 2f)).ToPoint();
                    }
                }
                else
                {
                    // No hitbox during last bit
                    return true;
                }
                #endregion
            }

            return false;
        }
        #endregion

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

        #region Parry Setup - see PlayerFX

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mod"></param>
        /// <param name="parryWindow"></param>
        /// <param name="parryCooldown">Determines time between parries. Success triples, effect lasts double</param>
        /// <returns></returns>
        public bool AtlFunctionParry(Player player, Mod mod, int parryWindow, int parryCooldown)
        {
            parryCooldownSave = parryCooldown;

            PlayerFX pfx = player.GetModPlayer<PlayerFX>(mod);
            if (player.itemAnimation == 0 && pfx.parryTime == 0)
            {
                pfx.parryTimeMax = parryWindow + parryCooldown;
                pfx.parryTime = pfx.parryTimeMax;
                pfx.parryActive = pfx.parryTimeMax - parryWindow;

                WeaponOut.NetUpdateParry(mod, pfx);
                return true;
            }
            return false;
        }
        private int parryCooldownSave;

        public bool HoldItemOnParryFrame(Player player, Mod mod, bool OnlyFirstFrame = false)
        {
            int parryIndex = player.FindBuffIndex(mod.BuffType<Buffs.ParryActive>());
            if (parryIndex >= 0)
            {
                if(player.buffTime[parryIndex] == parryCooldownSave * 2 || !OnlyFirstFrame)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called by ModPlayer in manageBodyFrame (PostUpdate)
        /// </summary>
        /// <param name="pfx"></param>
        public static void ParryBodyFrame(PlayerFX pfx)
        {
            Player player = pfx.player;
            float positiveTimeMax = pfx.parryTimeMax - pfx.parryActive;
            if (positiveTimeMax < 1) positiveTimeMax = 1;
            float anim = (pfx.parryTime - pfx.parryActive) / positiveTimeMax;
            if (anim > 0.6)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else if (anim > 0.3)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 3;
            }
            else if (anim > 0)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 2;
            }
            else
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 1;
            }
        }

        #endregion
    }
}
