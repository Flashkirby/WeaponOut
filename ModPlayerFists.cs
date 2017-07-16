using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.World.Generation;

// using static WeaponOut.ProjFX;
// uses ReflectProjectilePlayer(Projectile projectile, Player player)

// using static WeaponOut.WeaponOut;
// NetUpdateParry()
// NetUpdateDash()

namespace WeaponOut
{

    public class ModPlayerFists : ModPlayer
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        private const bool DEBUG_PARRYFISTS = false;
        public const int useStyle = 102115116; //http://www.unit-conversion.info/texttools/ascii/ with fst to ASCII numbers

        public const int ComboResetTime = 2 * 60;

        /// <summary> Colour for active combo counter </summary>
        private static Color highColour = Color.Cyan;
        /// <summary> Colour for inactive combo counter </summary>
        private static Color lowColour = Color.DarkCyan;
        private Color comboColour
        {
            get
            {
                if (IsComboActive) return highColour;
                return lowColour;
            }
        }

        /// <summary> Dictionary to keep track of all items qualified as fists. Add your item type here from SetStaticDefaults, with their combo power. </summary>
        public static Dictionary<int, uint> FistItem
        {
            get
            {
                if (fistItem == null) fistItem = new Dictionary<int, uint>();
                return fistItem;
            }
        }
        private static Dictionary<int, uint> fistItem;

        /// <summary>
        /// The type of fist attack initiated.
        /// 0 = Default (dash forward)
        /// 1 = Uppercut (rising strike)
        /// 2 = Dive (falling stomp)
        /// </summary>
        public int specialMove;

        /// <summary> Keep track of the number of hits from fists. </summary>
        protected int comboCounter;
        /// <summary> The "minimum" required combo get the bonus effects. </summary>
        public int comboCounterMax;
        /// <summary> Time since last combo hit </summary>
        protected int comboTimer;
        /// <summary> Time until combo is reset </summary>
        public int comboTimerMax;
        /// <summary> Active when combo counter reaches the combo max </summary>
        public bool IsComboActive { get { return comboCounter >= comboCounterMax; } }

        /// <summary> Ticks of current parry. 
        /// Non zero positive whilst in effect (count down), negative whilst on cooldown. </summary>
        protected int parryTime;
        /// <summary> Maximum time for current parry, used for swing animation. </summary>
        public int parryTimeMax;
        /// <summary> Number of active frames for the parry, also used for swing animation. </summary>
        public int parryWindow;
        /// <summary> Frame that parry is active until. </summary>
        public int ParryActiveFrame {get { return Math.Max(parryTimeMax - parryWindow, 1); } }
        /// <summary> Active while parry time greater/equal to parry time   </summary>
        public bool IsParryActive { get { return parryTime >= ParryActiveFrame && parryTime > 0; } }
        /// <summary> Provided by a buff, is the parry bonus active. </summary>
        public bool parryBuff;

        /// <summary> The initial speed of a dash. 
        ///<para /> * Normal: 3
        ///<para /> * Aglet/Anklet: 3.15, 3.3
        ///<para /> * Hermes: 6
        ///<para /> * Lightning: 6.75
        ///<para /> * Fishron Air: 8
        ///<para /> * Solar Wings: 9</summary>
        public float dashSpeed;
        /// <summary> The speed at which the dash will slow down towards. </summary>
        public float dashMaxSpeedThreshold;
        /// <summary> Friction applied when above the maxSpeed threshold. </summary>
        public float dashMaxFriction;
        /// <summary> Friction applied when below the maxSpeed threshold. </summary>
        public float dashMinFriction;

        #region overrides
        public override void Initialize()
        {
            specialMove = 0;
            comboCounter = 0;
            comboCounterMax = 0;
            comboTimer = -1;
            comboTimerMax = ComboResetTime;

            parryTime = 0;
            parryTimeMax = 0;
            parryWindow = 0;
            parryBuff = false;

            dashSpeed = 0f;
            dashMaxSpeedThreshold = 0f;
            dashMaxFriction = 0f;
            dashMinFriction = 0f;
        }
        public override void ResetEffects()
        {
            ResetVariables();
        }

        public override bool PreItemCheck()
        {
            if (ModConf.enableFists)
            {
                // Don't use the item whilst a parry is in progress
                if (ItemCheckParry())
                {
                    return false;
                }
            }
            return true;
        }

        public override void PostUpdate()
        {
            // Set up parry frames
            if (ModConf.enableFists)
            {
                if(specialMove == 2)
                {
                    // Increase speed whilst diving
                    player.maxFallSpeed *= 1.5f;
                }

                FistBodyFrame();
                ParryBodyFrame();
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            CustomDashMovement();
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            return ParryPreHurt(damageSource);
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            OnHitComboLogic(item);
        }
        #endregion

        public void ResetVariables()
        {
            if (comboTimer >= 0 && comboTimer < comboTimerMax) comboTimer++;
            else
            {
                // Show the total number of hits, and disable count until next hit
                if(comboTimer == comboTimerMax)
                {
                    CombatText.NewText(player.getRect(),
                        highColour, comboCounter + " hit", false, false);

                    comboCounter = 0;
                    comboTimer = -1;
                }
            }

            comboTimerMax = ComboResetTime; // 2 seconds count

            // Reset special move when set to 0
            if(player.itemAnimation <= (player.HeldItem.autoReuse ? 1 : 0))
            {
                specialMove = 0;
            }

            parryBuff = false;
        }

        #region FistStyle Item-calls
        
        public static void ModifyTooltips(List<TooltipLine> tooltips, Item item)
        {
            // Fist Items only
            uint comboPower;
            if (FistItem.TryGetValue(item.type, out comboPower))
            {
                tooltips.Add(new TooltipLine(item.modItem.mod, "FistComboPower", comboPower + " combo power"));
            }
        }

        /// <summary>
        /// Assign special moves, attack rotation and hitboxes, called in the ModItem
        /// </summary>
        /// <param name="distance">Pixels extended outwards</param>
        /// <param name="jumpSpeed">Jump provided from uppercut. Default jump roughly 9-10</param>
        /// <param name="fallSpeedX"></param>
        /// <param name="fallSpeedY">Set dive Y speed, fall speed is auto increased by 1.5 (velY = 15)</param>
        /// <returns></returns>
        public bool UseItemHitbox(Player player, ref Rectangle hitbox, int distance, float jumpSpeed, float fallSpeedX = 8f, float fallSpeedY = 8f)
        {
            // Special attack assign on Start frame
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                SetSpecialMove(player, jumpSpeed, fallSpeedX, fallSpeedY);

                if (specialMove == 0)
                {
                    SetAttackRotation(player);
                }

                if (Main.myPlayer == player.whoAmI)
                {
                    NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    NetMessage.SendData(MessageID.ItemAnimation, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            return SetHitBox(player, out hitbox, distance);
        }


        private void SetSpecialMove(Player player, float jumpSpeed, float fallSpeedX, float fallSpeedY)
        {
            // Ground available counts includes the start of a jump
            bool canUseGrounded = (player.velocity.Y == 0 && player.oldVelocity.Y == 0) || (player.jump > 0);

            // Set uppercut jump
            if ((player.controlDown || player.controlUp)
                && canUseGrounded
                && jumpSpeed > 0)
            {
                specialMove = 1;
                player.itemRotation = -(float)(player.direction * Math.PI / 2);
                player.velocity.Y = -jumpSpeed * player.gravDir;
                player.jump = 0;
            }
            // Set dive trajectory
            else if (player.controlDown
                && fallSpeedX > 0)
            {
                specialMove = 2;
                player.itemRotation = (float)(player.direction * Math.PI / 2);
                player.velocity.X = player.direction * ((player.velocity.X + fallSpeedX * 5) / 6);
                if (player.gravDir > 0)
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
        private bool SetHitBox(Player player, out Rectangle hitbox, int distance)
        {
            // Animation normal
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            hitbox = new Rectangle();
            if (specialMove == 0)
            {
                // Calculate hitbox for normal punch
                #region Standard Punch
                if (anim > 0.15f)
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
                        // Size an extension of the player
                        hitbox.Width = Player.defaultWidth;
                        hitbox.Height = Player.defaultHeight;

                        // Work out which way to go
                        float xDir = player.direction;
                        float yDir = 1f;

                        if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
                        {
                            xDir /= 2;
                            if (player.itemRotation * player.direction > 0)
                            {
                                // Up high
                                yDir *= -1f;
                            }
                            else
                            {
                                // Down low
                                yDir *= 1f;
                            }
                        }
                        else
                        {
                            // Too slow
                            yDir = 0;
                        }

                        hitbox.Width += (int)(Math.Abs(xDir) * distance);
                        hitbox.Height += (int)(Math.Abs(yDir) * distance);

                        hitbox.Location = (player.Center + new Vector2(
                             hitbox.Width + xDir * distance,
                             hitbox.Height + yDir * distance
                            ) / 2).ToPoint();
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

        const float maxShow = 0.8f;
        const float minShow = 0.4f;
        /// <summary> Generates a fisticuffs rectangle </summary>
        /// <returns> True if no hitbox (so no dust) </returns>
        public static Rectangle UseItemGraphicbox(Player player, int boxSize, float distanceFactor = 1f)
        {
            Rectangle box = new Rectangle();
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            //no show during winding
            if (anim > maxShow || anim <= minShow)
            {
                return new Rectangle();
            }

            //set player direction/hitbox
            Vector2 centre = new Vector2();
            float swing = 1 - (anim - minShow) / (maxShow - minShow); //0.8 -> 0.4
            if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
            {
                if (player.itemRotation * player.direction > 0)
                {
                    //Up high
                    centre = new Vector2(
                        player.Center.X - (player.width * 0.6f * player.direction) + player.width * 1.1f * distanceFactor * swing * player.direction,
                        player.Center.Y + (player.height * 0.9f * distanceFactor * swing));
                }
                else
                {
                    //Down low
                    centre = new Vector2(
                        player.Center.X - (player.width * 0.6f * player.direction) + player.width * 1.1f * distanceFactor * swing * player.direction,
                        player.Center.Y - (player.height * 0.9f * distanceFactor * swing));
                }
            }
            else
            {
                //along the middle
                centre = new Vector2(
                    player.Center.X - (player.width * 0.5f * player.direction) + player.width * 1f * distanceFactor * swing * player.direction,
                    player.Center.Y);
            }
            box.X = (int)centre.X - boxSize / 2;
            box.Y = (int)centre.Y - boxSize / 2;
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

        #endregion

        #region FistStyle imported methods

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

        private void OnHitComboLogic(Item item)
        {
            // Fist Items only
            if (!FistItem.ContainsKey(item.type)) return;

            // Add up that combo, reset timer
            comboCounter++;
            comboTimer = 0;

            // Display the combo counter, lower if not combo active
            Rectangle rect = player.getRect();
            if (!IsComboActive) rect.Y += (int)(rect.Height * player.gravDir);
            CombatText.NewText(rect,
                comboColour, string.Concat(comboCounter), IsComboActive);

            // Set time to match, to sync up projectiles
            player.itemTime = player.itemAnimation;
        }
        private void ManagePlayerComboMovement(NPC target)
        {
            if (specialMove == 0)
            {
                #region Normal Punch
                // Allow punches to be combo'd together quickly by reducing time between attacks
                player.itemAnimation = 2 * player.itemAnimation / 3;
                provideImmunity(player, player.itemAnimationMax / 2);

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

                // Partially follow the target if it's in the air
                if (aerial)
                {
                    player.velocity = target.velocity;
                    player.velocity.X -= player.direction; // Some bounce off
                    player.velocity.Y -= player.gravDir * 4f; // Try to preserve Y velo
                }
                else
                {
                    // Bounce off
                    player.velocity += new Vector2(
                        player.direction * -1.5f + target.velocity.X * -1f,
                        player.gravDir * -2f + target.velocity.Y * 2);
                }
                #endregion
            }
            else if (specialMove == 1)
            {
                // Uppercut grants 2/3 immunity, without bounces etc.
                provideImmunity(player, 2 * player.itemAnimationMax / 3);
            }
            else if (specialMove == 2)
            {
                //disengage
                int direction = 1;
                if (player.Center.X < target.Center.X) direction = -1;
                player.velocity = new Vector2(direction * 3f, player.gravDir * -1f);
            }
        }

        public void FistBodyFrame()
        {
            // Don't apply to non fists
            if (!FistItem.ContainsKey(player.HeldItem.type)) return;

            // Animation normal
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            if (specialMove == 0)
            {
                #region Normal Punch
                //wind up animation
                if (anim > 0.7f) player.bodyFrame.Y = player.bodyFrame.Height * 10;
                else if (anim > 0.66f)
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
                // Uppercut
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
                // Dive
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
        }
        
        #endregion

        #region Parry

        /// <summary> Create item flash and sound. Client only </summary>
        public void ItemFlashFX(int dustType = 45)
        {
            if (Main.LocalPlayer != player) return;
            Main.PlaySound(25, -1, -1, 1);
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(
                    player.position, player.width, player.height, dustType, 0f, 0f, 255,
                    default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
                Main.dust[d].noLight = true;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.5f;
            }
        }
        
        /// <summary> Logic for managing parry timers </summary>
        /// <returns> True if the parry animation is active </returns>
        private bool ItemCheckParry()
        {
            // No parry when 0
            if (parryTime == 0) return false;

            // Parry noise
            if (parryTime == parryTimeMax) Main.PlaySound(2, player.Center, 32);

            if (DEBUG_PARRYFISTS) Main.NewText(string.Concat("Parrying: ", parryTime, "/", parryWindow, "/", parryTimeMax));

            // WHILE ACTIVE
            if (parryTime > 0)
            {
                player.itemAnimation = 1; // prevent switching
                parryTime--;

                if (parryTime == 0)
                {
                    player.itemAnimation = 0; // release lock
                    parryTime = 0;
                    parryTimeMax = 0;
                    parryWindow = 0;
                }

                return true;
            }

            // Cooldown
            if (parryTime < 0)
            {
                parryTime++;

                if (parryTime == 0)
                {
                    ItemFlashFX();
                    parryTime = 0;
                    parryTimeMax = 0;
                    parryWindow = 0;
                }

                return false;
            }
            return false;
        }

        /// <summary> Called by ModPlayer in manageBodyFrame (PostUpdate) </summary>
        public void ParryBodyFrame()
        {
            if (parryTime <= 0) return;

            // count down to 1 -> 0 -> -x
            float anim = (parryTime - ParryActiveFrame) / Math.Max(parryWindow, 1);

            // Start low, swing upwards in reverse
            if (anim > 0.6)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else if (anim > 0.2)
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

        /// <summary>  </summary>
        /// <returns> True when a received attack is parried </returns>
        private bool ParryPreHurt(PlayerDeathReason damageSource)
        {
            // Cannot parry non-standard damage sources
            if (damageSource.SourceNPCIndex == -1 && 
                damageSource.SourcePlayerIndex == -1 && 
                damageSource.SourceProjectileIndex == -1)
            { return false; }

            // Cannot parry when not in the active window
            if(!IsParryActive)
            { return false; }

            // Parrying also counts as a combo
            comboCounter++;
            comboTimer = 0;

            player.itemAnimation = 0; //release item lock
            
            // Set cooldown before next parry is active, relative to parry non-active frames
            parryTime = ParryActiveFrame * -3;
            parryWindow = 0;
            parryTimeMax = 0;

            // Strike the NPC away slightly
            if (damageSource.SourceNPCIndex >= 0)
            {
                NPC npc = Main.npc[damageSource.SourceNPCIndex];

                // Damage is based on the NPC's attack, plus player melee multiplier
                int damage = (int)(npc.defense * player.meleeDamage);
                // Knockback of weapon, with scaling
                float knockback = player.GetWeaponKnockback(player.HeldItem, player.HeldItem.knockBack);
                // Parried attacks have double crit
                bool crit = Main.rand.Next(100) < player.meleeCrit * 2f;
                int hitDirection = player.direction;

                if (npc.knockBackResist > 0)
                {
                    knockback /= 0.5f + (npc.knockBackResist * 0.5f);
                }
                knockback += 1f;

                // Already a client only method so no need to check for whoAmI
                player.ApplyDamageToNPC(npc, damage, knockback, hitDirection, crit);
            }
            else
            {
                Main.PlaySound(SoundID.NPCHit3, player.position);
                if (damageSource.SourceProjectileIndex >= 0)
                {
                    ProjFX.ReflectProjectilePlayer(
                        Main.projectile[damageSource.SourceProjectileIndex], player);
                }
            }

            // Add 5 sec parry buff and short invincibility
            provideImmunity(player, 20);
            player.AddBuff(mod.BuffType<Buffs.ParryActive>(), 300, false);

            if (DEBUG_PARRYFISTS) Main.NewText(string.Concat("Parried! : ", parryTime, "/", ParryActiveFrame, "/", parryTimeMax));

            // Send information
            if (Main.netMode == 1 && player.whoAmI == Main.myPlayer)
            {
                ModPacket message = mod.GetPacket();
                message.Write(2);
                message.Write(Main.myPlayer);
                message.Write(parryTimeMax);
                message.Write(parryWindow);
                message.Send();
            }

            return true;
        }

        /// <summary> Sets the values etc. for parrying </summary>
        /// <param name="parryCooldown">Determines time between parries. Parry cooldown is tripled on successful parry. </param>
        /// <returns> True to allow parrying input (AltFunction) </returns>
        public bool AtlFunctionParry(Player player, int parryWindow, int parryCooldown)
        {
            return AltFunctionParryMax(player, parryWindow, parryWindow + parryCooldown);
        }
        public bool AltFunctionParryMax(Player player, int parryWindow, int parryTimeMax)
        {
            if (player.itemAnimation == 0 && parryTime == 0)
            {
                this.parryWindow = parryWindow;
                this.parryTimeMax = parryTimeMax;

                WeaponOut.NetUpdateParry(this);
            }
            return false;
        }

        #endregion

        #region Dashing

        /// <summary>
        /// Set the dash for the player.
        /// </summary>
        /// <returns> If set dash is possible (player.dashDelay) </returns>
        public bool SetDash(float dashSpeed = 14.5f, float dashMaxSpeedThreshold = 12f, float dashMaxFriction = 0.992f, float dashMinFriction = 0.96f, bool forceDash = false)
        {
            if (forceDash)
            {
                player.dashDelay = 0;
            }

            if (player.dashDelay == 0)
            {
                this.dashSpeed = dashSpeed;
                this.dashMaxSpeedThreshold = dashMaxSpeedThreshold;
                this.dashMaxFriction = dashMaxFriction;
                this.dashMinFriction = dashMinFriction;
            }
            return player.dashDelay == 0;
        }

        private void CustomDashMovement()
        {
            // dash = player equipped dash type
            // dashTime = timeWindow for double tap
            // dashDelay = -1 during active, counts down to 0 after dash ends (30 for SoC, 20 for tabi)
            // eocDash = EoC active frame time, 15 until dash ends, then count down (still active during deccel)
            // eocHit = registers the hit NPC for 8 frames

            // Reset here because reasons.
            if (player.pulley || player.grapCount > 0)
            {
                dashSpeed = 0;
                dashMaxSpeedThreshold = 0;
                dashMaxFriction = 0;
                dashMinFriction = 0;
            }

            // When dashing is set up
            if (dashSpeed != 0 || dashMaxSpeedThreshold != 0 || dashMaxFriction != 0 || dashMinFriction != 0)
            {
                if (player.dashDelay == 0)
                {
                    #region Dash Stats
                    /*
                    switch (weaponDash)
                    {
                        case 1: // Fists of fury
                            dashSpeed = 10f;
                            break;
                        case 2: // Caestus
                            dashSpeed = 15f;
                            break;
                        case 3: // Boxing Gloves
                            dashSpeed = 12f;
                            Gore g;
                            if (player.velocity.Y == 0f)
                            {
                                g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f)];
                            }
                            else
                            {
                                g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 14f), default(Vector2), Main.rand.Next(61, 64), 1f)];
                            }
                            g.velocity.X = (float)Main.rand.Next(-50, 51) * 0.01f;
                            g.velocity.Y = (float)Main.rand.Next(-50, 51) * 0.01f;
                            g.velocity *= 0.4f;
                            break;
                        case 4: // Spiked Gauntlets
                            dashSpeed = 13f;
                            break;
                        case 5: // Apocafist
                            dashSpeed = 13f;
                            break;
                    }
                    */
                    #endregion

                    float dSpeed = this.dashSpeed;
                    float direction = 0;

                    direction = player.direction;
                    player.velocity.X = dSpeed * direction;

                    Point point3 = (player.Center + new Vector2((float)(player.direction * player.width / 2 + 2), player.gravDir * -(float)player.height / 2f + player.gravDir * 2f)).ToTileCoordinates();
                    Point point4 = (player.Center + new Vector2((float)(player.direction * player.width / 2 + 2), 0f)).ToTileCoordinates();
                    if (WorldGen.SolidOrSlopedTile(point3.X, point3.Y) || WorldGen.SolidOrSlopedTile(point4.X, point4.Y))
                    {
                        player.velocity.X = player.velocity.X / 2f;
                    }

                    // Set dash to active
                    player.dashDelay = -1;
                    WeaponOut.NetUpdateDash(this);
                }

                // Apply movement during the actual dash, 
                // movement during delay is managed already in DashMovement()
                float maxAccRunSpeed = Math.Max(player.accRunSpeed, player.maxRunSpeed);
                if (player.dashDelay < 0)
                {
                    #region Dash Stats
                    /*
                    float dashMaxSpeedThreshold = 12f;
                    float dashMaxFriction = 0.992f;
                    float dashMinFriction = 0.96f;
                    int dashSetDelay = 30; // normally 20 but given that his ends sooner...
                    switch (weaponDash)
                    {
                        case 1: // Normal short-ish dash
                            dashMaxSpeedThreshold = 8f;
                            dashMaxFriction = 0.98f;
                            dashMinFriction = 0.94f;
                            for (int i = 0; i < 3; i++)
                            {
                                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                                    DustID.Fire, 0, 0, 100, default(Color), 1.8f)];
                                d.velocity = d.velocity * 0.5f + player.velocity * -0.4f;
                                d.noGravity = true;
                                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                            }
                            break;
                        case 2: // Super quick ~12 tile dash
                            dashMaxSpeedThreshold = 6f;
                            dashMaxFriction = 0.8f;
                            dashMinFriction = 0.94f;
                            dashSetDelay = 20;
                            break;
                        case 3: // Boxing Gloves ~ 4.5 tile step
                            dashMaxSpeedThreshold = 3f;
                            dashMaxFriction = 0.8f;
                            for (int j = 0; j < 2; j++)
                            {
                                Dust d;
                                if (player.velocity.Y == 0f)
                                {
                                    d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)player.height - 4f), player.width, 8, 31, 0f, 0f, 100, default(Color), 1.4f)];
                                }
                                else
                                {
                                    d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 8f), player.width, 16, 31, 0f, 0f, 100, default(Color), 1.4f)];
                                }
                                d.velocity *= 0.1f;
                                d.scale *= 1f + (float)Main.rand.Next(20) * 0.01f;
                                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                            }
                            break;
                        case 4: // Spiked Gauntlets
                            dashMaxSpeedThreshold = 10f;
                            dashMaxFriction = 0.985f;
                            dashMinFriction = 0.95f;
                            for (int k = 0; k < 2; k++)
                            {
                                Dust d;
                                if (player.velocity.Y == 0f)
                                {
                                    d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)player.height - 8f), player.width, 16, 39, player.velocity.X, 0f, 0, default(Color), 1.4f)];
                                }
                                else
                                {
                                    d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 10f), player.width, 20, 40, player.velocity.X, 0f, 0, default(Color), 1.4f)];
                                }
                                d.velocity *= 0.1f;
                                d.scale *= 1f + (float)Main.rand.Next(20) * 0.01f;
                                d.noGravity = true;
                                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                            }
                            break;
                        case 5: // Long range
                            dashMaxSpeedThreshold = 7f;
                            dashMaxFriction = 0.99f;
                            dashMinFriction = 0.8f;
                            for (int i = 0; i < 4; i++)
                            {
                                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                                    DustID.Fire, 0, 0, 100, default(Color), 2f)];
                                d.velocity = d.velocity * 0.5f + player.velocity * -0.4f;
                                d.noGravity = true;
                                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                                d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                                    DustID.Smoke, 0, 0, 100, default(Color), 0.4f)];
                                d.fadeIn = 0.7f;
                                d.velocity = d.velocity * 0.1f + player.velocity * -0.2f;
                                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                            }
                            break;
                    }
                    */
                    #endregion

                    // normally 20 but this dash seems to end sooner for some reason
                    int dashCooldownDelay = 30; 

                    // Prevent vanilla dash movement
                    player.dash = 0; 
                    // Unstealth
                    player.vortexStealthActive = false;

                    // Dashing logic
                    if (player.velocity.X > dashMaxSpeedThreshold || player.velocity.X < -dashMaxSpeedThreshold)
                    {
                        player.velocity.X = player.velocity.X * dashMaxFriction;
                    }
                    else if (player.velocity.X > maxAccRunSpeed || player.velocity.X < -maxAccRunSpeed)
                    {
                        player.velocity.X = player.velocity.X * dashMinFriction;
                    }
                    // Stop the active part of the dash after reaching the max normal run speed
                    else
                    {
                        player.dashDelay = dashCooldownDelay;
                        if (player.velocity.X < 0f)
                        {
                            player.velocity.X = -maxAccRunSpeed;
                        }
                        else if (player.velocity.X > 0f)
                        {
                            player.velocity.X = maxAccRunSpeed;
                        }
                    }

                }
            }

            // Reset dash values (disable) as it comes off cooldown
            if (player.dashDelay == 1)
            {
                dashSpeed = 0;
                dashMaxSpeedThreshold = 0;
                dashMaxFriction = 0;
                dashMinFriction = 0;
            }
        }

        #endregion
    }
}
