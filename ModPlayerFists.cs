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
// NetUpdateDash()
// NetUpdateParry()
// NetUpdateCombo()

// Also requires in WeaponOut.WeaponOut:
// HandlePacketDash
// HandlePacketParry
// HandlePacketCombo

namespace WeaponOut
{
    /// <summary>
    /// Version 1.4 by Flashkirby99
    /// This class provides almost all the methods required for fist type weapons.
    /// </summary>
    public class ModPlayerFists : ModPlayer
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        private const bool DEBUG_FISTBOXES = true;
        private const bool DEBUG_DASHFISTS = false;
        private const bool DEBUG_PARRYFISTS = false;

        public static ModPlayerFists Get(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>();
        }

        private const bool DEBUG_COMBOFISTS = false;
        public const int useStyle = 102115116; //http://www.unit-conversion.info/texttools/ascii/ with fst to ASCII numbers

        /// <summary> Default combo is held for 2 seconds. </summary>
        public const int ComboResetTime = 2 * 60;
        /// <summary> Flat combo time modifier </summary>
        public int comboResetTimeBonus;

        /// <summary> Colour for active combo counter </summary>
        private static Color highColour = Color.Cyan;
        /// <summary> Colour for inactive combo counter </summary>
        private static Color lowColour = Color.DarkCyan;
        /// <summary> Colour for combo tooltip </summary>
        private static Color tooltipColour = Color.LightCyan;
        private Color comboColour
        {
            get
            {
                if (IsComboActive) return highColour;
                return lowColour;
            }
        }

        /// <summary> Reset every hit, allows 1 combo bounce and invincibility during each punch. </summary>
        bool allowComboBounce = false;
        /// <summary>
        /// The type of fist attack initiated.
        /// 0 = Default (dash forward)
        /// 1 = Uppercut (rising strike)
        /// 2 = Dive (falling stomp)
        /// </summary>
        public int specialMove;
        /// <summary> Allows uppercuts without staying grounded (requires air hits) </summary>
        public bool jumpAgainUppercut = false;

        // Bonuses
        public float uppercutDamage = 1f;
        public float uppercutKnockback = 1.5f;
        public float divekickDamage = 1f;
        public float divekickKnockback = 1f;
        public float parryLifesteal = 0f;

        #region Combo Counter Vars
        /// <summary> Keep track of the number of hits from fists. </summary>
        protected int comboCounter;
        protected int oldComboCounter;
        public int ComboCounter { get { return comboCounter; } }
        public int OldComboCounter { get { return oldComboCounter; } }
        /// <summary> The "minimum" required combo get the bonus effects. Must be at least 2. </summary>
        public int comboCounterMax;
        /// <summary> Flat bonus to combo counter max. Typically negative. </summary>
        public int comboCounterMaxBonus;
        /// <summary> Time since last combo hit. </summary>
        public int comboTimer;
        /// <summary> Time until combo is reset. </summary>
        public int comboTimerMax;
        /// <summary> The real combo counter max, including bonus. </summary>
        public int ComboCounterMaxReal { get { return comboCounterMax + comboCounterMaxBonus; } }
        /// <summary> Active when combo counter reaches the combo max. </summary>
        public bool IsComboActive { get { return comboCounter >= ComboCounterMaxReal && comboCounter > 1; } }
        /// <summary> Active when combo counter reaches the combo max. Call this in the item because ItemLoader method is called before PlayerHooks. </summary>
        public bool IsComboActiveItemOnHit { get { return comboCounter >= ComboCounterMaxReal - 1 && comboCounter > 0; } }
        #endregion

        #region Parry Vars
        /// <summary> Ticks of current parry. 
        /// Non zero positive whilst in effect (count down), negative whilst on cooldown. </summary>
        protected int parryTime;
        /// <summary> Maximum time for current parry, used for swing animation. </summary>
        public int parryTimeMax;
        /// <summary> Number of active frames for the parry, also used for swing animation. </summary>
        public int parryWindow;
        /// <summary> Flat bonus on parry window. </summary>
        public int parryWindowBonus;
        public int ParryTimeMaxReal { get { return parryTimeMax + parryWindowBonus; } }
        public int ParryWindowReal { get { return parryWindow + parryWindowBonus; } }
        /// <summary> Frame that parry is active until. </summary>
        public int ParryActiveFrame { get { return Math.Max(parryTimeMax - parryWindow, 2); } }
        /// <summary> Active while parry time greater/equal to parry time   </summary>
        public bool IsParryActive { get { return parryTime >= ParryActiveFrame && parryTime > 0; } }
        /// <summary> Provided by a buff, is the parry bonus active. </summary>
        public bool parryBuff;

        // Bonuses
        public float parryDamage = 1f;
        public bool longParry = false;
        #endregion

        #region Dash Vars 
        /// <summary> normally 20 but custom dash seems to end sooner for some reason </summary>
        public const int dashCooldownDelay = 35;

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
        /// <summary> ID for dash effect. Register these with RegisterDashEffect. Starts at 1 </summary>
        public int dashEffect;
        
        private static List<Action<Player>> dashEffectsMethods;
        /// <summary> Void method for dash effects. </summary>
        protected static List<Action<Player>> DashEffectsMethods
        {
            get
            {
                if (dashEffectsMethods == null) { dashEffectsMethods = new List<Action<Player>>(); }
                return dashEffectsMethods;
            }
        }
        #endregion

        #region Combo Special Vars
        /// <summary> ID for combo effect. Register these with RegisterComboEffect. Uses negative as post initialised. </summary>
        private int comboEffect;
        public int ComboEffectAbs { get { return Math.Abs(comboEffect); } }

        private static List<Action<Player, bool>> comboEffectsMethods;
        /// <summary> Void method for combo effects. </summary>
        protected static List<Action<Player, bool>> ComboEffectsMethods
        {
            get
            {
                if (comboEffectsMethods == null) { comboEffectsMethods = new List<Action<Player, bool>>(); }
                return comboEffectsMethods;
            }
        }
        /// <summary> ID for combo effect. Register these with RegisterComboEffect. Starts at 1 </summary>
        #endregion


        #region overrides
        public override void Initialize()
        {
            specialMove = 0;
            comboResetTimeBonus = 0;
            comboCounter = 0;
            oldComboCounter = 0;
            comboCounterMax = 0;
            comboCounterMaxBonus = 0;
            comboTimer = -1;
            comboTimerMax = ComboResetTime + comboResetTimeBonus;

            parryTime = 0;
            parryTimeMax = 0;
            parryWindow = 0;
            parryWindowBonus = 0;
            parryBuff = false;

            comboEffect = 0;

            ResetDashVars();
            player.dashDelay = 0;
            player.dash = 0;
        }
        public override void UpdateDead()
        {
            this.Initialize();
        }
        public override void ResetEffects()
        {
            oldComboCounter = comboCounter;
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

                // Reset comboEffect at the end of animation
                ManageComboMethodCall();

                // Make dashing effects hit everything it passes through
                if(dashEffect != 0 || specialMove == 1)
                {
                    player.attackCD = 0;
                }
            }
            return true;
        }

        public override void PreUpdate()
        {
            if (ModConf.enableFists)
            {
                comboCounterMax = player.HeldItem.tileBoost;

                // Jump again uppercut cannot be used from ground, only after attack resets
                if (player.velocity.Y == 0)
                {
                    // But upgraded fists can
                    Item i = new Item(); i.SetDefaults(player.HeldItem.type);
                    if (i.rare >= 4)
                    { jumpAgainUppercut = true; }
                    else
                    { jumpAgainUppercut = false; }
                }

                // Reset dash here when grappling
                if (player.pulley || player.grapCount > 0)
                {
                    if (player.dashDelay == -1) player.dashDelay = dashCooldownDelay;
                    ResetDashVars();
                }

                if (specialMove == 2)
                {
                    // Increase speed whilst diving
                    player.maxFallSpeed *= 1.5f;
                }
            }
        }

        public override void PostUpdate()
        {
            // Set up parry frames
            if (ModConf.enableFists)
            {
                SetComboEffectLogic();

                FistBodyFrame();
                ParryBodyFrame();

                ShowFistHandOn();
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            CustomDashMovement();
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // All fists have a global damage reduction on projectiles, because they're all about closing
            // in and doing big DPS. Due to the nature of the weapon, dodging projectiles is less important
            // than positioning between the player and enemies. Therefore this defence buff is in place to
            // reduce the pressure of projectiles on an already heavily stacked against usestyle.
            if(player.HeldItem.useStyle == useStyle && damageSource.SourceProjectileIndex >= 0)
            { damage = (int)(damage * 0.75f); }

            return !ParryPreHurt(damageSource);
        }

        // This gets called last in the hook stack, after Item, then NPC
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (specialMove == 1) // Uppercut
            {
                damage = (int)(damage * uppercutDamage);
                knockback *= uppercutKnockback;
            }
            else if (specialMove == 2) // Divekick
            {
                damage = (int)(damage * divekickDamage);
                knockback *= divekickKnockback;
            }
        }
        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            if (specialMove == 1)
            {
                damage = (int)(damage * uppercutDamage);
            }
            else if (specialMove == 2)
            {
                damage = (int)(damage * divekickDamage);
            }
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            OnHitComboLogic(item, target);
        }
        
        #endregion

        private void ShowFistHandOn()
        {
            if (player.itemAnimation > 0)
            {
                if (player.HeldItem.useStyle == useStyle)
                {
                    if (player.HeldItem.handOnSlot > 0)
                    {
                        player.handon = player.HeldItem.handOnSlot;
                        player.cHandOn = 0;
                    }
                    if (player.HeldItem.handOffSlot > 0)
                    {
                        player.handoff = player.HeldItem.handOffSlot;
                        player.cHandOff = 0;
                    }
                }
            }
        }

        public void ResetVariables()
        {
            comboCounterMaxBonus = 0;
            comboTimerMax = ComboResetTime + comboResetTimeBonus; // 2? seconds count
            comboResetTimeBonus = 0; // reset

            // Count up, otherwise reset to -1
            if (comboTimer >= 0 && comboTimer < comboTimerMax) comboTimer++;
            else
            {
                // Show the total number of hits, and disable count until next hit
                if (comboTimer == comboTimerMax)
                {
                    if (comboCounter > 1)
                    {
                        CombatText.NewText(player.getRect(),
                            highColour, comboCounter + " hits", false, false);
                    }
                    
                    comboCounter = 0;
                    comboTimer = -1;
                }
            }

            // Reset special move when set to 0
            if (player.itemAnimation <= (player.HeldItem.autoReuse ? 1 : 0))
            {
                specialMove = 0;
            }

            parryBuff = false;

            parryDamage = 1f;
            longParry = false;
            uppercutDamage = 1f;
            uppercutKnockback = 1.5f;
            divekickDamage = 1f;
            divekickKnockback = 1f;
            parryLifesteal = 0f;
        }
        
        #region Fist Hitboxes

        /// <summary>
        /// Assign special moves, attack rotation and hitboxes, called in the ModItem
        /// </summary>
        /// <param name="distance">Pixels extended outwards</param>
        /// <param name="jumpSpeed">Jump provided from uppercut. Default jump roughly 9-10</param>
        /// <param name="fallSpeedX"></param>
        /// <param name="fallSpeedY">Set dive Y speed, fall speed is auto increased by 1.5 (velY = 15)</param>
        /// <returns></returns>
        public static bool UseItemHitbox(Player player, ref Rectangle hitbox, int distance, float jumpSpeed = 9f, float fallSpeedX = 2f, float fallSpeedY = 8f)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf == null) return false;

            // Special attack assign on Start frame
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                mpf.allowComboBounce = true;
                mpf.SetSpecialMove(player, jumpSpeed, fallSpeedX, fallSpeedY);

                if (mpf.specialMove == 0)
                {
                    SetAttackRotation(player);
                }

                if (Main.netMode == 1 && Main.myPlayer == player.whoAmI)
                {
                    NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    NetMessage.SendData(MessageID.ItemAnimation, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            return mpf.SetHitBox(player, out hitbox, distance);
        }


        private void SetSpecialMove(Player player, float jumpSpeed, float fallSpeedX, float fallSpeedY)
        {
            // Ground available counts includes the start of a jump
            bool canUseGrounded = (player.velocity.Y == 0 && player.oldVelocity.Y == 0) || (player.jump > 0);

            // Choose special move
            if ((player.controlDown || player.controlUp)
                && canUseGrounded
                && jumpSpeed > 0)
            {   // Uppercut from ground or start of jump
                specialMove = 1;
            }
            else if (player.controlUp && !canUseGrounded && jumpAgainUppercut)
            {   // Uppercut from jumpagain
                specialMove = 1;
                jumpAgainUppercut = false;
            }
            else if (player.controlDown
                && fallSpeedX > 0)
            {   // Divekick
                specialMove = 2;
            }


            if (specialMove == 1 || specialMove == 2)
            {
                if (player.controlLeft && !player.controlRight)
                { player.direction = -1; }
                if (player.controlRight && !player.controlLeft)
                { player.direction = 1; }
            }

            // Set uppercut jump
            if (specialMove == 1)
            {
                player.itemRotation = -(float)(player.direction * Math.PI / 2);
                player.velocity.Y = -jumpSpeed * player.gravDir;
                player.jump = 0;
                player.fallStart = (int)(player.position.Y / 16f); // Reset fall
            }
            // Set dive trajectory
            else if (specialMove == 2)
            {
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
            // If set above the max, no hitbox yet (see combo attacks)
            if (player.itemAnimation > player.itemAnimationMax) return false;

            if (specialMove == 0)
            {
                // Calculate hitbox for normal punch
                #region Standard Punch
                if (anim > 0.15f)
                {
                    // Start as player hitbox
                    if (anim > 0.7f)
                    {
                        // Provide immunity for 1/3
                        provideImmunity(player);

                        hitbox = player.getRect();
                        hitbox.X += (int)player.velocity.X;
                        hitbox.Y += (int)player.velocity.Y;
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
                                yDir *= 1f;
                            }
                            else
                            {
                                // Down low
                                yDir *= -1f;
                            }
                        }
                        else
                        {
                            // Too slow
                            yDir = 0;
                        }

                        hitbox.Width += (int)(Math.Abs(xDir) * distance);
                        hitbox.Height += (int)(Math.Abs(yDir) * distance);

                        hitbox.Location = (player.MountedCenter + new Vector2(
                             -hitbox.Width + xDir * distance,
                             -hitbox.Height + yDir * distance
                            ) / 2).ToPoint();
                    }
                }
                else
                {
                    // No hitbox during last third
                    return false;
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

                    Vector2 centreLeft = new Vector2(player.position.X, player.MountedCenter.Y);

                    // Work out which way to go
                    if (player.direction < 0)
                    {
                        hitbox.Location = (centreLeft - new Vector2(
                            distance,
                            hitbox.Height / 2f)).ToPoint();
                    }
                    else
                    {
                        hitbox.Location = (centreLeft - new Vector2(
                            0,
                            hitbox.Height / 2f)).ToPoint();
                    }
                }
                else
                {
                    // No hitbox during last bit
                    return false;
                }
                #endregion
            }

            if (DEBUG_FISTBOXES)
            {
                Dust.NewDustPerfect(hitbox.TopLeft(), 213);
                Dust.NewDustPerfect(hitbox.TopRight(), 213);
                Dust.NewDustPerfect(hitbox.BottomLeft(), 213);
                Dust.NewDustPerfect(hitbox.BottomRight(), 213);
            }

            return true;
        }

        #endregion

        #region Fist Combo Logic

        private void OnHitComboLogic(Item item, NPC target)
        {
            // Fist Items only
            if (item.useStyle != useStyle) return;

            // Set time to match, to sync up projectiles
            player.itemTime = player.itemAnimation;

            // Add up that combo, reset timer, Display the combo counter, lower if not combo active
            // Combo specials don't grant more combo
            if (comboEffect == 0) ModifyComboCounter(1, true);
            
            // Manage player bump
            if (DEBUG_DASHFISTS) Main.NewText(string.Concat("COmbo dash: ", dashEffect, " - alt: ", player.altFunctionUse));
            if (dashEffect == 0 && comboEffect == 0 && (allowComboBounce || specialMove == 2))
            {
                ManagePlayerComboMovement(target);
                allowComboBounce = false;
            }
            else if (dashEffect != 0)
            {
                // Dash attacks provide a short immunity
                if (allowComboBounce)
                {
                    provideImmunity(player, 20);
                    allowComboBounce = false;
                }
            }
            else
            {
                // Combo specials provide immunity over duration
                if (allowComboBounce)
                {
                    provideImmunity(player, player.itemAnimation + 1);
                    allowComboBounce = false;
                }
            }
        }

        public void ModifyComboCounter(int amount, bool resetTimer = true)
        {
            comboCounter += amount;
            if (comboCounter == ComboCounterMaxReal) ItemFlashFX();
            if (resetTimer) comboTimer = 0;

            // Don't bother showing spent combo
            if (comboCounter <= 0) return;

            Rectangle rect = player.getRect();
            if (ComboCounterMaxReal == 0) return; // avoid div by 0
            bool comboMatchMax = comboCounter % ComboCounterMaxReal == 0;
            if (!comboMatchMax) rect.Y += (int)(rect.Height * player.gravDir * 1f);
            CombatText.NewText(rect,
                comboColour, comboCounter, comboMatchMax, !IsComboActive || !comboMatchMax);
        }

        private void ManagePlayerComboMovement(NPC target)
        {
            if (target.aiStyle == 28) // WOF eye follows head position
            { target = Main.npc[Main.wof]; }

            Vector2 cappedVelocity = target.velocity;
            if (target.aiStyle == 27)
            {
                // Keep normal "velocity" since WoF doesn't update old position for some reason. 
            }
            else if (target.knockBackResist == 0f ||
                (cappedVelocity.X == 0 && cappedVelocity.Y == 0))
            {
                cappedVelocity = target.position - target.oldPosition;
            }
            // Double knockback if target is moving towards player
            if ((player.direction < 0f && cappedVelocity.X > 0f) ||
                (player.direction > 0f && cappedVelocity.X < 0f))
            { cappedVelocity.X *= 2f; }
            cappedVelocity = new Vector2(
                  Math.Min(8f + player.accRunSpeed, Math.Max(-8f - player.accRunSpeed, cappedVelocity.X)),
                  Math.Min(8f + player.accRunSpeed, Math.Max(-8f - player.accRunSpeed, cappedVelocity.Y)));

            if (target.velocity.X == 0f && target.velocity.Y == 0f)
            {
                cappedVelocity = new Vector2(
               Math.Min(10f, Math.Max(-10f, (target.oldPosition.X - target.position.X) * 1.5f)),
               Math.Min(10f, Math.Max(-10f, target.oldPosition.Y - target.position.Y)));
            }

            if (specialMove != 1)
            {
                // Punches and stomps reset uppercut
                jumpAgainUppercut = true;
            }

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
                if (!aerial && !WorldUtils.Find(origin, Searches.Chain(new Searches.Down(4), new GenCondition[]
                {
                    new Conditions.IsSolid()
                }), out point))
                {
                    aerial = true;
                }

                // Partially follow the target if it's in the air
                if (aerial)
                {
                    player.velocity = cappedVelocity;
                    player.velocity.X -= player.direction * 3f + player.direction * player.HeldItem.knockBack * 0.1f; // Some bounce off
                    player.velocity.Y -= player.gravDir * 0.125f * player.itemAnimationMax; // Try to preserve Y velo, based on attack speed (claws do less, fists do more)
                    player.fallStart = (int)(player.position.Y / 16f); // Reset fall
                }
                else
                {
                    // Bounce off
                    player.velocity = new Vector2(
                        -player.direction * (2f + player.HeldItem.knockBack * 0.5f) + cappedVelocity.X * 0.5f,
                        cappedVelocity.Y * 1.5f * target.knockBackResist);
                    player.fallStart = (int)(player.position.Y / 16f); // Reset fall
                }
                #endregion
            }
            else if (specialMove == 1)
            {
                // Uppercut grants full immunity for rest of uppercut but no bounces etc.
                provideImmunity(player, player.itemAnimation - 1);
            }
            else if (specialMove == 2)
            {
                #region Divekick
                // Grant mostly invulnerable divekicks on hit
                provideImmunity(player, player.itemAnimation - 1);

                // Bounce off
                int direction = 1;
                if (player.Center.X < target.Center.X) direction = -1;
                float bounceY = player.gravDir * -2f + Math.Min(0, cappedVelocity.Y * 1.5f);
                player.velocity = new Vector2(direction * 4f, bounceY);

                // Reset fall and restore jumps
                player.wingTime = (float)player.wingTimeMax;
                player.rocketTime = Math.Max(player.rocketTime, player.rocketTimeMax);
                player.rocketDelay = 0;
                player.fallStart = (int)(player.position.Y / 16f);
                player.jumpAgainCloud = player.doubleJumpCloud;
                player.jumpAgainSandstorm = player.doubleJumpSandstorm;
                player.jumpAgainBlizzard = player.doubleJumpBlizzard;
                player.jumpAgainFart = player.jumpAgainBlizzard;
                player.jumpAgainSail = player.doubleJumpSail;
                player.jumpAgainUnicorn = player.doubleJumpUnicorn;
                #endregion
            }

            // Combo hits reset dash
            ResetDashVars();
            player.dashDelay = 0;
            player.dash = 0;
        }

        private void FistBodyFrame()
        {
            // Don't show when not attacking
            if (player.itemAnimation == 0) return;

            // Don't apply to non fists
            if (player.HeldItem.useStyle != useStyle) return;

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
                        player.bodyFrame.Y = player.bodyFrame.Height * 17;
                    }
                    else if (anim > 0.1)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                    }
                    else
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 6;
                    }
                }
                #endregion
            }
        }

        #endregion

        #region Fist Item Methods

        public static void ModifyTooltips(List<TooltipLine> tooltips, Item item)
        {
            // Fist Items only
            if (item.useStyle == useStyle)
            {
                int index = 0;
                foreach (TooltipLine tooltip in tooltips)
                {
                    if (tooltip.Name.Equals("TileBoost")) break;
                    index++;
                }
                int comboBonus = Main.LocalPlayer.GetModPlayer<ModPlayerFists>().comboCounterMaxBonus;
                tooltips.RemoveAt(index);
                TooltipLine tt = new TooltipLine(item.modItem.mod, "FistComboPower",
                    Math.Max(2, item.tileBoost + comboBonus) +
                    " combo power cost");
                tt.overrideColor = tooltipColour;
                tooltips.Insert(index, tt);
            }
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

        const float maxShow = 1f;
        const float minShow = 0.4f;
        /// <summary> Generates a fisticuffs rectangle for use with dusts and such. </summary>
        /// <returns> True if no hitbox (so no dust) </returns>
        public static Rectangle UseItemGraphicbox(Player player, int boxSize, int distance)
        {
            Rectangle box = new Rectangle();

            #region Above animation range?
            // If set above the max (for first frame), probably a combo special, give the hand box
            if ( player.itemAnimation >= player.itemAnimationMax - 1)
            {
                return GetPlayerOnHandRectangle(player, boxSize);
            }
            #endregion

            box.Width = boxSize;
            box.Height = boxSize;
            float anim = player.itemAnimation / (float)player.itemAnimationMax;

            int special = player.GetModPlayer<ModPlayerFists>().specialMove;
            if (special == 1)
            {
                #region Uppercut
                // Convert anim to move up only up to 0.6
                anim = (anim - 0.4f) / 0.6f;
                if (anim < 0) { return new Rectangle(); }
                box.Location = player.Center.ToPoint();
                // Value from 0->1->0
                float xNormal = player.direction * (float)Math.Sin(anim * Math.PI);
                float xDistance = player.width / 2 + distance * 0.75f;
                float yDistance = player.height + distance * 2f;
                box.X += (int)(xNormal * xDistance);
                box.Y += (int)((yDistance * anim - yDistance / 2) * player.gravDir);
                #endregion
            }
            else if (special == 2 && player.velocity.Y != 0)
            {
                #region Divekick
                box.Location = player.TopLeft.ToPoint();
                if (player.direction > 0)
                { box.X += player.width; }
                if (player.gravDir > 0)
                { box.Y += player.height; }
                box.X += player.direction * boxSize;
                box.Y += (int)player.gravDir * boxSize;
                #endregion
            }
            else
            {
                #region Standard Punch
                //no show during winding
                if (anim > maxShow || anim <= minShow) { return new Rectangle(); }

                //set player direction/hitbox
                Vector2 centre = new Vector2();
                float swing = 1 - (anim - minShow) / (maxShow - minShow); //0.8 -> 0.4
                if (Math.Abs(player.itemRotation) > Math.PI / 4 && Math.Abs(player.itemRotation) < 3 * Math.PI / 4)
                {
                    float cX = (player.width * 0.5f + distance) * 0.5f * swing * player.direction
                        - (player.width * 0.5f * player.direction);
                    float cY = (player.height * 0.5f + distance) * swing;
                    if (player.itemRotation * player.direction < 0)
                    {
                        //Up high
                        centre = new Vector2(player.Center.X + cX, player.Center.Y - cY);
                    }
                    else
                    {
                        //Down low
                        centre = new Vector2(player.Center.X + cX, player.Center.Y + cY);
                    }
                }
                else
                {
                    //along the middle
                    centre = new Vector2(
                        player.Center.X - (player.width * 0.5f * player.direction) + (player.width * 0.5f + distance) * swing * player.direction,
                        // Down a bit to match player hand
                        player.Center.Y + 5 * player.gravDir);
                }
                box.X = (int)centre.X;
                box.Y = (int)centre.Y;

                if (player.direction > 0)
                {
                    box.X += player.width / 2;
                }
                else
                {
                    box.X -= player.width / 2;
                }
                #endregion
            }
            // width/height and dust displacement
            box.X -= 2 + boxSize / 2 + (int)(player.velocity.X);
            box.Y -= 2 + boxSize / 2 + (int)(player.velocity.Y);
            return box;
        }

        public static Rectangle GetPlayerOnHandRectangle(Player player, int boxSize)
        {
            Vector2 hand = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
            if (player.direction != 1)
            {
                hand.X = (float)player.bodyFrame.Width - hand.X;
            }
            if (player.gravDir != 1f)
            {
                hand.Y = (float)player.bodyFrame.Height - hand.Y;
            }
            hand -= new Vector2((float)(player.bodyFrame.Width - player.width), (float)(player.bodyFrame.Height - 42)) / 2f;
            Vector2 dustPos = player.RotatedRelativePoint(player.position + hand, true) - player.velocity;
            return new Rectangle(
                (int)dustPos.X - (boxSize / 2 + 2),
                (int)dustPos.Y - (boxSize / 2 + 2),
                boxSize, boxSize);
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
            if (parryTime == ParryTimeMaxReal) Main.PlaySound(2, 
                (int)player.Center.X, (int)player.Center.Y, 39, 1f, -0.8f);

            if (DEBUG_PARRYFISTS) Main.NewText(string.Concat("Parrying: ", parryTime, "/", parryWindow, "/", ParryTimeMaxReal));

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
            float anim = (parryTime - ParryActiveFrame) / Math.Max(ParryWindowReal, 1f);

            // Start low, swing upwards in reverse
            if (anim > 0.6)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else if (anim > 0.25)
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
            if (!IsParryActive)
            { return false; }

            // Parrying also counts as a combo
            ModifyComboCounter(1, true);

            player.itemAnimation = 0; //release item lock

            // Set cooldown before next parry is active, relative to parry non-active frames and invinc frames
            parryTime = (int)(-1f * (ParryActiveFrame * 3f + 20 + player.itemAnimationMax));
            if (longParry) parryTime += 15;
            parryWindow = 0;
            parryTimeMax = 0;

            int stealLife = 0;
            // Strike the NPC away slightly
            if (damageSource.SourceNPCIndex >= 0)
            {
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);

                NPC npc = Main.npc[damageSource.SourceNPCIndex];
                if (!npc.immortal && !npc.dontTakeDamage)
                {
                    // Hardmode fists restore uppercut on parry
                    Item i = new Item(); i.SetDefaults(player.HeldItem.type);
                    if (i.rare >= 4)
                    { jumpAgainUppercut = true; }

                    // Damage is based on the NPC's attack, plus player melee multiplier
                    int damage = npc.defDefense + (int)(npc.damage * player.meleeDamage * parryDamage);
                    // Knockback of weapon, with scaling
                    float knockback = player.GetWeaponKnockback(player.HeldItem, player.HeldItem.knockBack);
                    // Parried attacks have double crit
                    bool crit = Main.rand.Next(100) < player.meleeCrit * 2f;
                    int hitDirection = player.direction;

                    // Damage cap based on weapon strength
                    damage = Math.Min(damage, player.HeldItem.damage * 5);

                    if (npc.knockBackResist > 0)
                    {
                        knockback /= 0.5f + (npc.knockBackResist * 0.5f);
                    }
                    knockback += 1f;
                    
                    // Already a client only method so no need to check for whoAmI
                    player.ApplyDamageToNPC(npc, damage, knockback, hitDirection, crit);

                    if (!player.moonLeech && parryLifesteal > 0f)
                    {
                        stealLife = (int)(damage * parryLifesteal);
                    }
                }
            }
            else
            {
                Main.PlaySound(SoundID.NPCHit3, player.position);
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                if (damageSource.SourceProjectileIndex >= 0)
                {
                    if(ProjFX.ReflectProjectilePlayer(
                        Main.projectile[damageSource.SourceProjectileIndex], player))
                    {
                        // Hardmode fists restore uppercut on parry
                        Item i = new Item(); i.SetDefaults(player.HeldItem.type);
                        if (i.rare >= 4)
                        { jumpAgainUppercut = true; }

                        if (!player.moonLeech && parryLifesteal > 0f)
                        {
                            stealLife = Math.Min(player.HeldItem.damage * 5,
                                (int)(Main.projectile[damageSource.SourceProjectileIndex].damage *
                                player.meleeDamage * parryLifesteal * 5));
                        }
                    }
                }
            }
            if (stealLife > 0 && player.lifeSteal > 0)
            {
                player.lifeSteal -= stealLife;
                player.HealEffect(stealLife, true);
                player.statLife += stealLife;
                player.statLife = Math.Min(player.statLife, player.statLifeMax2);
                if (Main.netMode == 1 && Main.myPlayer == player.whoAmI) NetMessage.SendData(MessageID.PlayerHealth, -1, -1, null, player.whoAmI);
            }

            // Add 5 sec parry buff and short invincibility
            provideImmunity(player, 20 + player.itemAnimationMax);
            player.AddBuff(mod.BuffType<Buffs.ParryActive>(), 300, false);

            if (DEBUG_PARRYFISTS) Main.NewText(string.Concat("Parried! : ", parryTime, "/", ParryActiveFrame, "/", ParryTimeMaxReal));

            // Send information
            WeaponOut.NetUpdateParry(this);

            return true;
        }

        /// <summary> Sets the values etc. for parrying </summary>
        /// <param name="parryCooldown">Determines time between parries. Parry cooldown is tripled on successful parry. </param>
        /// <returns> True to allow parrying input (AltFunction) </returns>
        public bool AltFunctionParry(Player player, int parryWindow, int parryCooldown)
        {
            return AltFunctionParryMax(player, parryWindow, parryWindow + parryCooldown);
        }
        public bool AltFunctionParryMax(Player player, int parryWindow, int parryTimeMax)
        {
            if (player.itemAnimation == 0 && parryTime == 0)
            {
                this.parryWindow = parryWindow;
                this.parryTimeMax = parryTimeMax;
                if (longParry)
                {
                    this.parryWindow += 20;
                    this.parryTimeMax += 20;
                }
                this.parryTime = ParryTimeMaxReal;


                if (DEBUG_PARRYFISTS) Main.NewText("parry: " + parryWindow + "/" + parryTime);

                WeaponOut.NetUpdateParry(this);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Special parry upgrade which allows dashing.
        /// </summary>
        public bool AltFunctionParryDash(Player player, int parryWindow, int parryCooldown, float jumpSpeed = 14.3f, float dashDiveSpeed = 14.5f, float dashMaxSpeedThreshold = 12f, float dashMaxFriction = 0.992f, float dashMinFriction = 0.96f)
        {
            if (AltFunctionParryMax(player, parryWindow, parryWindow + parryCooldown))
            {
                SetDashOnMovement(dashDiveSpeed, dashMaxSpeedThreshold, dashMaxFriction, dashMinFriction, true, 0);
                SetSpecialMove(player, jumpSpeed, 1f, dashDiveSpeed); // Get movement, but still normal
                specialMove = 0;
                return true;
            }
            return false;
        }

        public int GetParryBuff()
        {
            return player.FindBuffIndex(mod.BuffType<Buffs.ParryActive>());
        }
        public bool ClearParryBuff()
        {
            int i = GetParryBuff();
            if (i >= 0)
            {
                player.DelBuff(i); return true;
            }
            return false;
        }

        #endregion

        #region Dashing

        /// <summary>
        /// In SetStaticDefaults. Register the dust effect method: public static void DashEffects(Player player).
        /// </summary>
        /// <returns> The ID of the effects method. Save this and refer to it in SetDash. </returns>
        public static int RegisterDashEffectID(Action<Player> dashEffect)
        {
            DashEffectsMethods.Add(dashEffect);
            return DashEffectsMethods.Count;
        }

        /// <summary>
        /// Set the dash for the player.
        /// </summary>
        /// <param name="dashSpeed">The initial speed of a dash. 
        ///<para /> * Normal: 3
        ///<para /> * Aglet/Anklet: 3.15, 3.3
        ///<para /> * Hermes: 6
        ///<para /> * Lightning: 6.75
        ///<para /> * Fishron Air: 8
        ///<para /> * Solar Wings: 9</param>
        /// <param name="dashMaxSpeedThreshold">The speed at which the dash will slow down towards. Basically, the threshold for maxFriction and minFriction. </param>
        /// <param name="dashMaxFriction">Friction multiplier applied when above the maxSpeed threshold. </param>
        /// <param name="dashMinFriction">Friction multiplier applied when below the maxSpeed threshold. </param>
        /// <param name="forceDash">Reset dash delay to 0?</param>
        /// <param name="dashEffect">Give this the number from RegisterDashEffectID </param>
        /// <returns> If set dash is possible (player.dashDelay) </returns>
        public bool SetDash(float dashSpeed = 14.5f, float dashMaxSpeedThreshold = 12f, float dashMaxFriction = 0.992f, float dashMinFriction = 0.96f, bool forceDash = false, int dashEffect = 0)
        {
            if (forceDash) // for small dashes... also because multiplayer 
            {
                player.dashDelay = 0;
            }
            else
            {
                if (player.itemAnimation > 1) return false;
            }

            if (player.dashDelay == 0)
            {
                if (dashMaxSpeedThreshold < 4f)
                    dashMaxSpeedThreshold = 4f; // Cannot be less than this otherwise wierd behaviour

                float speed = Math.Abs(player.velocity.X);
                if (speed > dashSpeed)
                {
                    dashSpeed = speed;
                    dashMaxSpeedThreshold = Math.Max(player.accRunSpeed, speed - Math.Max(0, dashSpeed - dashMaxSpeedThreshold));
                }

                this.dashSpeed = dashSpeed;
                this.dashMaxSpeedThreshold = dashMaxSpeedThreshold;
                this.dashMaxFriction = dashMaxFriction;
                this.dashMinFriction = dashMinFriction;
                this.dashEffect = dashEffect;
            }
            return player.dashDelay == 0;
        }
        /// <summary>
        /// Set the dash for the player, but only when the player is using a directional movement key. 
        /// </summary>
        /// <param name="dashSpeed">The initial speed of a dash. 
        ///<para /> * Normal: 3
        ///<para /> * Aglet/Anklet: 3.15, 3.3
        ///<para /> * Hermes: 6
        ///<para /> * Lightning: 6.75
        ///<para /> * Fishron Air: 8
        ///<para /> * Solar Wings: 9</param>
        /// <param name="dashMaxSpeedThreshold">The speed at which the dash will slow down towards. Basically, the threshold for maxFriction and minFriction. </param>
        /// <param name="dashMaxFriction">Friction multiplier applied when above the maxSpeed threshold. </param>
        /// <param name="dashMinFriction">Friction multiplier applied when below the maxSpeed threshold. </param>
        /// <param name="forceDash">Reset dash delay to 0?</param>
        /// <param name="dashEffect">Give this the number from RegisterDashEffectID </param>
        /// <returns> If set dash is possible (player.dashDelay) </returns>
        public bool SetDashOnMovement(float dashSpeed = 14.5f, float dashMaxSpeedThreshold = 12f, float dashMaxFriction = 0.992f, float dashMinFriction = 0.96f, bool forceDash = false, int dashEffect = 0)
        {
            if(player.controlLeft || player.controlRight)
            {
                return SetDash(dashSpeed, dashMaxSpeedThreshold, dashMaxFriction, dashMinFriction, forceDash, dashEffect);
            }
            return false;
        }
        
        /// <summary>
        /// Doesn't get called while grappling
        /// </summary>
        private void CustomDashMovement()
        {
            // dash = player equipped dash type
            // dashTime = timeWindow for double tap
            // dashDelay = -1 during active, counts down to 0 after dash ends (30 for SoC, 20 for tabi)
            // eocDash = EoC active frame time, 15 until dash ends, then count down (still active during deccel)
            // eocHit = registers the hit NPC for 8 frames

            // When dashing is set up
            if (dashSpeed != 0 || dashMaxSpeedThreshold != 0 || dashMaxFriction != 0 || dashMinFriction != 0)
            {
                // Use the custom provided dash startup
                if(dashEffect > 0 && player.dashDelay <= 0)
                {
                    DashEffectsMethods[dashEffect - 1](player);
                }

                // Initial
                if (player.dashDelay == 0)
                {
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

                    // Send net update for start of dash
                    WeaponOut.NetUpdateDash(this);

                    // Air dashes to nearby targets
                    AerialDashManeuvre();
                }

                // Apply movement during the actual dash, 
                // movement during delay is managed already in DashMovement()
                float maxAccRunSpeed = Math.Max(player.accRunSpeed, player.maxRunSpeed);
                if (player.dashDelay < 0)
                {
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
                        dashEffect = 0;
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
                ResetDashVars();
            }
        }

        /// <summary>
        /// Auto-targets to nearby NPCs during a dash whilst wingtime is active, to give a small Y velo boost
        /// </summary>
        private void AerialDashManeuvre()
        {
            if (player.wingTime > 0 &&
                                    player.velocity.Y != 0 &&
                                    player.whoAmI == Main.myPlayer &&
                                    specialMove == 0)
            {
                NPC target = null;
                Vector2 pC = player.Center;
                Vector2 mC = Main.MouseWorld;
                float maxDist = 128 + dashSpeed * 5;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.life <= 0 || npc.friendly) continue;
                    Vector2 nC = npc.Center;
                    if (player.direction > 0 && nC.X < pC.X) continue;
                    if (player.direction < 0 && nC.X > pC.X) continue;
                    float mouseDistance = Math.Abs(nC.X - mC.X) + Math.Abs(nC.Y - mC.Y);
                    float playerDistance = Math.Abs(nC.X - pC.X) + Math.Abs(nC.Y - pC.Y);
                    if (playerDistance <= dashSpeed * 32)
                    {
                        if (mouseDistance < maxDist)
                        {
                            maxDist = mouseDistance;
                            target = npc;
                        }
                    }
                }
                if (target != null)
                {
                    Vector2 p2n = Vector2.Subtract(target.Center, pC);
                    float boost = p2n.Y / (0.5f + Math.Abs(p2n.X / 16));
                    player.velocity.Y += Math.Max(-dashSpeed, boost);
                    if (Main.netMode == 1)
                    {
                        NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
            }
        }

        private void ResetDashVars()
        {
            dashSpeed = 0;
            dashMaxSpeedThreshold = 0;
            dashMaxFriction = 0;
            dashMinFriction = 0;
            dashEffect = 0;

            player.dash = 0;
        }

        #endregion

        #region Combo Meter

        /// <summary>
        /// In SetStaticDefaults. Register the combo effect method: public static void ComboEffects(Player player).
        /// </summary>
        /// <returns> The ID of the effects method. Save this and refer to it in AltFunctionCombo. </returns>
        public static int RegisterComboEffectID(Action<Player, bool> comboEffect)
        {
            ComboEffectsMethods.Add(comboEffect);
            return ComboEffectsMethods.Count;
        }

        public bool AltFunctionCombo(Player player, int comboEffect = 0)
        {
            if (player.itemAnimation == 0 && comboCounter >= ComboCounterMaxReal)
            {
                this.comboEffect = comboEffect;

                ConsumeCombo(player);

                if (ComboEffectAbs > 0) WeaponOut.NetUpdateCombo(this);
                return true;
            }
            return false;
        }

        /// <summary> Consume comboMax </summary>
        /// <returns>equal to or more than 0 if able to and has consumed combo.</returns>
        public int ConsumeCombo(Player player)
        {
            if (comboCounter >= ComboCounterMaxReal)
            {
                int oldCombo = comboCounter;
                ModifyComboCounter(-ComboCounterMaxReal, true);

                // Show combo consume
                CombatText.NewText(player.getRect(),
                    lowColour, -ComboCounterMaxReal + " combo", false, true);
                return oldCombo;
            }
            return -1;
        }

        // Call the combo management pre-item check
        private void ManageComboMethodCall()
        {
            if (ComboEffectAbs > 0)
            {
                if (DEBUG_COMBOFISTS) Main.NewText(string.Concat("Calling: ", ComboEffectAbs, "/", comboEffect));
                // initial is true when set as such. See SetComboEffectLogic from PostUpdate
                ComboEffectsMethods[ComboEffectAbs - 1](player, comboEffect > 0);
            }
        }

        private void SetComboEffectLogic()
        {
            // Update other clients that the fist is being used. 
            if (Main.netMode == 1 && Main.myPlayer == player.whoAmI)
            {
                NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                NetMessage.SendData(MessageID.ItemAnimation, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
            }

            // First time it was set was positive, turn negative to indicate change.
            if (comboEffect > 0) { comboEffect = -comboEffect; }

            // At the end, reset to 0
            if (player.itemAnimation == 1)
            {
                if (DEBUG_COMBOFISTS) Main.NewText(string.Concat("Resetting combo: ", comboEffect));
                comboEffect = 0;
            }
        }

        #endregion
    }
}
