using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

using ItemCustomizer;
using Terraria.ModLoader.IO;
//using Terraria.Graphics.Shaders;
//vs collapse all Ctrl-M-O

namespace WeaponOut
{
    public class PlayerFX : ModPlayer
    {
        private const bool DEBUG_WEAPONHOLD = false;
        private const bool DEBUG_BOOMERANGS = false;
        private const bool DEBUG_PARRYFISTS = false;

        public bool weaponVisual = true;

        private bool wasDead; //used to check if player just revived
        public Vector2 localTempSpawn;//spawn used by tent

        private static int openFist; //item ID of Fist Weapon
        private static int fireFistType;

        public int weaponFrame;//frame of weapon...

        private int damageKnockbackThreshold;
        public int DamageKnockbackThreshold
        {
            get { return damageKnockbackThreshold; }
            set
            {
                if (value > damageKnockbackThreshold) damageKnockbackThreshold = value;
            }
        }
        
        private int frontDefence;
        public int FrontDefence
        {
            get { return frontDefence; }
            set
            {
                if (value > frontDefence) frontDefence = value;
            }
        }
        public bool frontNoKnockback;

        public int lastSelectedItem;
        public int itemSkillDelay;

        public int dashingSpecialAttack;
        public const int dashingSpecialAttackOnsoku = 1;

        public bool reflectingProjectiles;
        public int reflectingProjectileDelay;
        public bool CanReflectProjectiles
        { get { return reflectingProjectiles && reflectingProjectileDelay <= 0; } }

        public bool lunarRangeVisual;
        public bool lunarMagicVisual;
        public bool lunarThrowVisual;

        public int parryTime;
        public int parryTimeMax;
        public int parryActive;
        public bool IsParryActive { get { return parryTime >= parryActive && parryTime > 0; } }

        #region Utils
        public static void drawMagicCast(Player player, SpriteBatch spriteBatch, Color colour, int frame)
        {
            Texture2D textureCasting = Main.extraTexture[51];
            Vector2 origin = player.Bottom + new Vector2(0f, player.gfxOffY + 4f);
            if (player.gravDir < 0) origin.Y -= player.height + 8f;
            Rectangle rectangle = textureCasting.Frame(1, 4, 0, Math.Max(0, Math.Min(3, frame)));
            Vector2 origin2 = rectangle.Size() * new Vector2(0.5f, 1f);
            if (player.gravDir < 0) origin2.Y = 0f;
            spriteBatch.Draw(
                textureCasting, new Vector2((float)((int)(origin.X - Main.screenPosition.X)), (float)((int)(origin.Y - Main.screenPosition.Y))),
                new Rectangle?(rectangle), colour, 0f, origin2, 1f,
                player.gravDir >= 0f ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
        }
        public static void modifyPlayerItemLocation(Player player, float X, float Y)
        {
            float cosRot = (float)Math.Cos(player.itemRotation);
            float sinRot = (float)Math.Sin(player.itemRotation);
            //Align
            player.itemLocation.X = player.itemLocation.X + (X * cosRot * player.direction) + (Y * sinRot * player.gravDir);
            player.itemLocation.Y = player.itemLocation.Y + (X * sinRot * player.direction) - (Y * cosRot * player.gravDir);
        }

        public static bool SameTeam(Player player1, Player player2)
        {
            // Always affects self
            if (player1.whoAmI == player2.whoAmI) return true;
            // If on a team, must be sharding a team
            if (player1.team > 0 && player1.team != player2.team) return false;
            // Not on same team during PVP
            if (player1.hostile && player2.hostile && (player1.team == 0 || player2.team == 0)) return false;
            // Banner applies to all (See Nebula Buff mechanics)
            return true;
        }

        public static void ItemFlashFX(Player player, int dustType = 45)
        {
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
        #endregion

        public override void OnEnterWorld(Player player)
        {
            lastSelectedItem = 0;

            itemSkillDelay = 0;
            dashingSpecialAttack = 0;

            localTempSpawn = new Vector2();

            if (ModConf.enableFists)
            {
                openFist = mod.ItemType("Fist");
                fireFistType = mod.ItemType("FistsOfFury");
            }

            parryTime = 0;
            parryTimeMax = 0;
            parryActive = 0;

            // Update visuals
            WeaponOut.NetUpdateWeaponVisual(mod, this);
        }

        public override void ResetEffects()
        {
            damageKnockbackThreshold = 0;
            frontDefence = 0;
            frontNoKnockback = false;

            if (player.velocity.Y == 0 && player.itemTime == 0)
            {
                if(dashingSpecialAttack != 0)
                {
                    //bleep
                    ItemFlashFX(player, 175);
                }
                // Restore special dashing if grounded
                dashingSpecialAttack = 0;
            }

            // Manage item skills
            if (player.selectedItem != lastSelectedItem)
            {
                lastSelectedItem = player.selectedItem;

                itemSkillDelay = 0;
                //Main.NewText(String.Concat(player.selectedItem, " / ", player.oldSelectItem));
            }

            // Reset visuals
            lunarRangeVisual = false;
            lunarMagicVisual = false;
            lunarThrowVisual = false;

            // Handle reflecting timer
            reflectingProjectiles = false;
            if (reflectingProjectileDelay > 0) reflectingProjectileDelay = Math.Max(0, reflectingProjectileDelay - 1);
        }

        #region Save and Load
        public override TagCompound Save()
        {
            return new TagCompound
            {
                { "weaponVisual", weaponVisual }
            };
        }
        public override void Load(TagCompound tag)
        {
            weaponVisual = tag.GetBool("weaponVisual");
        }
        #endregion

        public override bool PreItemCheck()
        {
            if(ModConf.enableBasicContent)
            {
                applyBannerBuff();
            }

            if (ModConf.enableFists)
            {
                if(ItemCheckParry())
                {
                    return false;
                }
            }
            /// <summary>Adds the weaponswitch network-synced buff</summary>
            if (ModConf.enableDualWeapons)
            {
                Items.Weapons.HelperDual.PreItemCheckDualItem(player);
            }
            //createBareFistInInv();
            return true;
        }
        private void applyBannerBuff()
        {
            foreach (Player bannerPlayer in Main.player)
            {
                if (!bannerPlayer.active || bannerPlayer.dead) continue;

                int itemType = bannerPlayer.inventory[bannerPlayer.selectedItem].type;
                if (itemType != mod.ItemType<Items.RallyBannerBlue>() &&
                    itemType != mod.ItemType<Items.RallyBannerGreen>() &&
                    itemType != mod.ItemType<Items.RallyBannerRed>() &&
                    itemType != mod.ItemType<Items.RallyBannerYellow>()
                    ) continue; //only use these banner items

                foreach(Player otherPlayer in Main.player)
                {
                    if (SameTeam(otherPlayer, bannerPlayer))
                    {
                        // Within the 100ft box
                        if (
                                otherPlayer.position.X >= bannerPlayer.position.X - Buffs.RallyBanner.buffRadius &&
                                otherPlayer.position.X <= bannerPlayer.position.X + Buffs.RallyBanner.buffRadius &&
                                otherPlayer.position.Y >= bannerPlayer.position.Y - Buffs.RallyBanner.buffRadius &&
                                otherPlayer.position.Y <= bannerPlayer.position.Y + Buffs.RallyBanner.buffRadius)
                        {
                            otherPlayer.AddBuff(mod.BuffType<Buffs.RallyBanner>(), 2);
                        }
                    }
                }
            }
        }
        private bool ItemCheckParry()
        {
            if(parryTime != 0)
            {
                if(parryTime == parryTimeMax)
                {
                    Main.PlaySound(2, player.Center, 32);
                }

                if (DEBUG_PARRYFISTS) Main.NewText(string.Concat("Parrying: ", parryTime, "/", parryActive, "/", parryTimeMax));

                if (parryTime > 0)
                {
                    player.itemAnimation = 1; // prevent switching
                    parryTime--;

                    if (parryTime == 0)
                    {
                        player.itemAnimation = 0; // release lock
                        parryTime = 0;
                        parryTimeMax = 0;
                        parryActive = 0;
                    }

                    return true;
                }

                // Cooldown
                if(parryTime < 0)
                {
                    parryTime++;

                    if (parryTime == 0)
                    {
                        ItemFlashFX(player);
                        parryTime = 0;
                        parryTimeMax = 0;
                        parryActive = 0;
                    }

                    return false;
                }
            }
            return false;
        }

        public override void PostUpdateRunSpeeds()
        {
            CustomDashMovement();

            if(player.inventory[player.selectedItem].type == mod.ItemType<Items.Weapons.Raiden>())
            {
                if (itemSkillDelay >= Items.Weapons.Raiden.focusTime)
                {
                    float setSpeed = player.maxRunSpeed / 4f;
                    player.maxRunSpeed -= setSpeed;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
        }

        #region Dash

        public int weaponDash = 0;
        private void CustomDashMovement()
        {
            // dash = player equipped dash type
            // dashTime = timeWindow for double tap
            // dashDelay = -1 during active, counts down to 0 after dash ends (30 for SoC, 20 for tabi)
            // eocDash = SoC active frame time, 15 until dash ends, then count down (still active during deccel)
            // eocHit = registers the hit NPC for 8 frames

            // Reset here because reasons.
            if(weaponDash > 0)
            {
                if (player.dashDelay == 0)
                {

                    #region Dash Stats
                    /*
                     * Normal: 3
                     * Aglet/Anklet: 3.15, 3.3
                     * Hermes: 6
                     * Lightning: 6.75
                     * Fishron Air: 8
                     * Solar Wings: 9
                     */
                    float dashSpeed = 14.5f;
                    switch (weaponDash)
                    {
                        case 1: // Fists of fury
                            dashSpeed = 10;
                            break;
                        case 2: // Caestus
                            dashSpeed = 15;
                            break;
                        case 3: // Boxing Gloves
                            dashSpeed = 12;
                            break;
                    }
                    #endregion

                    #region Set Dash speed

                    // Set initial dash speed
                    player.velocity.X = dashSpeed * (float)player.direction;
                    Point point3 = (player.Center + new Vector2((float)(player.direction * player.width / 2 + 2), player.gravDir * -(float)player.height / 2f + player.gravDir * 2f)).ToTileCoordinates();
                    Point point4 = (player.Center + new Vector2((float)(player.direction * player.width / 2 + 2), 0f)).ToTileCoordinates();
                    if (WorldGen.SolidOrSlopedTile(point3.X, point3.Y) || WorldGen.SolidOrSlopedTile(point4.X, point4.Y))
                    {
                        player.velocity.X = player.velocity.X / 2f;
                    }
                    player.dashDelay = -1;

                    WeaponOut.NetUpdateDash(mod, this);

                    #endregion
                }

                // Apply movement during dash, delay is managed already in DashMovement()
                float maxAccRunSpeed = Math.Max(player.accRunSpeed, player.maxRunSpeed);
                if (player.dashDelay < 0)
                {
                    player.dash = 0; // Prevent vanilla dash movement

                    #region Dash Stats
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
                            break;
                    }
                    #endregion

                    #region Modify dash speeds

                    player.vortexStealthActive = false;
                    if (player.velocity.X > dashMaxSpeedThreshold || player.velocity.X < -dashMaxSpeedThreshold)
                    {
                        player.velocity.X = player.velocity.X * dashMaxFriction;
                    }
                    else if (player.velocity.X > maxAccRunSpeed || player.velocity.X < -maxAccRunSpeed)
                    {
                        player.velocity.X = player.velocity.X * dashMinFriction;
                    }
                    else
                    {
                        player.dashDelay = dashSetDelay;
                        if (player.velocity.X < 0f)
                        {
                            player.velocity.X = -maxAccRunSpeed;
                        }
                        else if (player.velocity.X > 0f)
                        {
                            player.velocity.X = maxAccRunSpeed;
                        }
                    }

                    #endregion
                }
            }
            
            if(player.dashDelay == 1)
            {
                weaponDash = 0;
            }
        }

        #endregion

        public override void PostUpdate()
        {
            manageBodyFrame();
            tentScript();
            /*foreach (Projectile p in Main.projectile)
            {
                if (p.active && p.owner == Main.myPlayer)
                {
                    Main.NewText(p.name + "> " + p.ai[0] + " | " + p.ai[1] + " || " + p.localAI[0] + " | " + p.localAI[1] + " : " + p.timeLeft);
                }
            }*/
        }
        private void manageBodyFrame()
        {
            if (Main.netMode == 2) return; // Oh yeah, server calls this so don't pls

            if (ModConf.enableFists && parryTime > 0)
            {
                Items.Weapons.UseStyles.FistStyle.ParryBodyFrame(this);
                return;
            }

            //change idle pose for player using a heavy weapon
            //copypasting from drawPlayerItem
            Item heldItem = player.inventory[player.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0 || !ModConf.showWeaponOut) return; //no item so nothing to show
            Texture2D weaponTex = weaponTex = Main.itemTexture[heldItem.type];
            if (weaponTex == null) return; //no texture to item so ignore too
            float itemWidth = weaponTex.Width * heldItem.scale;
            float itemHeight = weaponTex.Height * heldItem.scale;
            if (heldItem.modItem != null)
            {
                if (heldItem.modItem.GetAnimation() != null)
                {
                    itemHeight /= heldItem.modItem.GetAnimation().FrameCount;
                }
            }
            float larger = Math.Max(itemWidth, itemHeight);
            int playerBodyFrameNum = player.bodyFrame.Y / player.bodyFrame.Height;
            if (heldItem.useStyle == 5
                && weaponTex.Width >= weaponTex.Height * 1.2f
                && (!heldItem.noUseGraphic || !heldItem.melee)
                && larger >= 45
                && (
                weaponVisual || ModConf.forceShowWeaponOut
                ) //toggle with accessory1 visibility, or forceshow is on
            )
            {
                if (playerBodyFrameNum == 0) player.bodyFrame.Y = 10 * player.bodyFrame.Height;
            }
        }
        private void tentScript()
        {
            if (wasDead && !player.dead)
            {
                if (localTempSpawn != default(Vector2)) checkTemporarySpawn();
                wasDead = false;
            }
            if (player.dead)
            {
                wasDead = true;
            }
        }
        private void checkTemporarySpawn()
        {
            if (player.whoAmI == Main.myPlayer)
            {
                //int dID = Dust.NewDust(new Vector2((float)(localTempSpawn.X * 16), (float)(localTempSpawn.Y * 16)), 16, 16, 44, 0f, 0f, 0, default(Color), 4f);
                //Main.dust[dID].velocity *= 0f;

                if ((int)Main.tile[(int)localTempSpawn.X, (int)localTempSpawn.Y].type != mod.TileType("CampTent"))
                {
                    Main.NewText("Temporary spawn was removed, returned to normal spawn", 255, 240, 20, false);
                    localTempSpawn = default(Vector2);
                    return;
                }
                Main.BlackFadeIn = 255;
                Main.renderNow = true;

                player.position.X = (float)(localTempSpawn.X * 16 + 8 - player.width / 2);
                player.position.Y = (float)((localTempSpawn.Y + 1) * 16 - player.height);
                player.fallStart = (int)(player.position.Y / 16);

                Main.screenPosition.X = player.position.X + (float)(player.width / 2) - (float)(Main.screenWidth / 2);
                Main.screenPosition.Y = player.position.Y + (float)(player.height / 2) - (float)(Main.screenHeight / 2);
            }
        }

        #region Player Layers
        public static readonly PlayerLayer HeldItem = new PlayerLayer("WeaponOut", "HeldItem", PlayerLayer.HeldItem, delegate(PlayerDrawInfo drawInfo)
        {
            if (drawInfo.shadow != 0f)
            {
                return;
            }
            try
            {
                drawPlayerItem(drawInfo, false);
            }
            catch { }
        });
        public static readonly PlayerLayer HairBack = new PlayerLayer("WeaponOut", "HairBack", PlayerLayer.HairBack, delegate(PlayerDrawInfo drawInfo)
        {
            if (drawInfo.shadow != 0f)
            {
                return;
            }
            try
            {
                drawPlayerItem(drawInfo, true);
            }
            catch { }
        });
        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            HeldItem.visible = true; // For items held in hand
            HairBack.visible = true; // For items behind the player (sheathed)
            //MiscEffectsFront.visible = !player.dead;
            try
            {
                int heldItemStack = layers.IndexOf(PlayerLayer.HeldItem);
                int hairBackStack = layers.IndexOf(PlayerLayer.HairBack);
                int MiscEffectsFrontStack = layers.IndexOf(PlayerLayer.MiscEffectsFront);
                layers.Insert(heldItemStack, HeldItem);
                layers.Insert(hairBackStack, HairBack);
            }
            catch { }
            //layers.Insert(MiscEffectsFrontStack, MiscEffectsFront);
        }
        #endregion
        /// <summary>
        /// We gonna handle all the weapon identification and calls here
        /// </summary>
        /// <param name="drawInfo"></param>
        /// <param name="drawOnBack"></param>
        private static void drawPlayerItem(PlayerDrawInfo drawInfo, bool drawOnBack)
        {
            //don't draw when not ingame
            if (Main.gameMenu || !ModConf.showWeaponOut) return;

            //get player player
            Player drawPlayer = drawInfo.drawPlayer;

            //hide if dead, stoned etc.
            if (!drawPlayer.active || drawPlayer.dead || drawPlayer.stoned) return;

            try
            {
                if (drawPlayer.itemAnimation > 0 //do nothing if player is doing something
                    || !(drawPlayer.GetModPlayer<PlayerFX>(WeaponOut.mod).weaponVisual && !ModConf.forceShowWeaponOut)) return; //also hide if accessory 1 is hidden
            }
            catch { }

            //player player's held item
            Item heldItem = drawPlayer.inventory[drawPlayer.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0) return; //no item so nothing to show

            //ignore projectile melee weapons
            bool isYoyo = false;
            // items work when checked at least once in singleplayer first...?
            if (DEBUG_BOOMERANGS) Main.NewText("Shoot is " + heldItem.shoot + " (!=0)");
            if (heldItem.shoot != 0)
            {
                if (DEBUG_BOOMERANGS) Main.NewText("heldItem.melee = " + heldItem.melee);
                if (DEBUG_BOOMERANGS) Main.NewText("heldItem.noMelee = " + heldItem.noMelee);
                if (heldItem.melee && heldItem.noMelee)
                {
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        if (!Main.projectile[i].active) continue;
                        if (Main.projectile[i].owner == drawPlayer.whoAmI &&
                            Main.projectile[i].melee)
                        {
                            return;
                        }
                    }
                }

                //  YOYO is aiStyle 99
                Projectile p = new Projectile();
                p.SetDefaults(heldItem.shoot);
                if (p.aiStyle == 99)
                {
                    isYoyo = true;
                }
            }

            //item texture
            Texture2D weaponTex = weaponTex = Main.itemTexture[heldItem.type];
            if (weaponTex == null) return; //no texture to item so ignore too
            int gWidth = weaponTex.Width;
            int gHeight = weaponTex.Height;

            //does the item have an animation? No vanilla weapons do
            Rectangle? sourceRect = null;
            if (heldItem.modItem != null)
            {
                if (heldItem.modItem.GetAnimation() != null) // in the case of modded weapons with animations...
                {
                    //get local player frame counting
                    PlayerFX p = drawPlayer.GetModPlayer<PlayerFX>(ModLoader.GetMod("WeaponOut"));
                    int frameCount = heldItem.modItem.GetAnimation().FrameCount;
                    int frameCounter = heldItem.modItem.GetAnimation().TicksPerFrame * 2;

                    //add them up
                    if (Main.time % frameCounter == 0)
                    {
                        p.weaponFrame++;
                        if (p.weaponFrame >= frameCount)
                        {
                            p.weaponFrame = 0;
                        }
                    }

                    //set frame on source
                    gHeight /= frameCount;
                    sourceRect = new Rectangle(0, gHeight * p.weaponFrame, gWidth, gHeight);
                }
            }


            //get draw location of player
            int drawX = (int)(drawPlayer.MountedCenter.X - Main.screenPosition.X);
            int drawY = (int)(drawPlayer.MountedCenter.Y - Main.screenPosition.Y + drawPlayer.gfxOffY) - 3;
            //get the lighting on the player's tile
            Color lighting = Lighting.GetColor(
                    (int)((drawInfo.position.X + drawPlayer.width / 2f) / 16f),
                    (int)((drawInfo.position.Y + drawPlayer.height / 2f) / 16f));
            //get item alpha (like starfury) then player stealth and alpha (inviciblity etc.)
            lighting = drawPlayer.GetImmuneAlpha(heldItem.GetAlpha(lighting) * drawPlayer.stealth, 0);

            float scale = heldItem.scale;
            if (isYoyo) scale *= 0.6f;

            //standard items
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction < 0) spriteEffects = SpriteEffects.FlipHorizontally;
            if (drawPlayer.gravDir < 0)
            {
                drawY += 6;
                spriteEffects = SpriteEffects.FlipVertically | spriteEffects;
            }
            DrawData data = new DrawData(
                    weaponTex,
                    new Vector2(drawX, drawY),
                    sourceRect,
                    lighting,
                    0f,
                    new Vector2(gWidth / 2f, gHeight / 2f),
                    scale,
                    spriteEffects,
                    0);

            // Item customiser integration
            // https://github.com/gamrguy/ItemCustomizer
            try
            {
                ItemCustomizerIntegration(drawInfo, heldItem, ref data.shader);
            }
            catch { } // Mod not found/loaded

            //work out what type of weapon it is!
            #region Weapon Algorithm
            float itemWidth = gWidth * heldItem.scale;
            float itemHeight = gHeight * heldItem.scale;
            //not all items have width/height set the same, so use largest as "length" including weapon sizemod
            float larger = Math.Max(itemWidth, itemHeight);
            float lesser = Math.Min(itemWidth, itemHeight);
            if (heldItem.useStyle == 1 || //swing
                heldItem.useStyle == 2 || //eat
                heldItem.useStyle == 3)   //stab
            {
                //|       ######        
                //|       ##  ##        
                //|     ##########            
                //|       ##  ##    
                //|       ##  ##    
                //|       ##  ##    
                //|       ##  ##    
                //|         ##      
                //Items, daggers and other throwables lie below 28 and are easily held in the hand
                if ((larger < 28 && !heldItem.magic) || //nonmagic weapons
                    (larger <= 32 && heldItem.shoot != 0) || //larger for throwing weapons
                    (larger <= 24 && heldItem.magic)) //only smallest magic weapons
                {
                    if (drawPlayer.grapCount > 0) return; // can't see while grappling
                    if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(hand): " + itemWidth + " x " + itemHeight);
                    if (drawOnBack) return;
                    data = WeaponDrawInfo.modDraw_HandWeapon(data, drawPlayer, larger, lesser);
                }
                //|             ####
                //|           ##  ##
                //|         ##  ##   
                //|       ##  ##    
                //|   ####  ##      
                //|   ##  ##        
                //| ##  ####        
                //| ####            
                //Broadsword weapons are swing type weapons between 28 - 48
                //They are worn on the waist, and react to falling!
                else if (larger <= 48)
                {
                    if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(waist): " + itemWidth + " x " + itemHeight);
                    if (!drawOnBack) return;
                    data = WeaponDrawInfo.modDraw_WaistWeapon(data, drawPlayer, larger);
                }
                //|           ########
                //|           ##    ##
                //|         ##    ####
                //|   ##  ##    ##  
                //|   ####    ##    
                //|   ##  ####      
                //| ##  ########    
                //| ######          
                //Great weapons are swing type weapons past 36 in size and slung on the back
                else
                {
                    if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(back): " + itemWidth + " x " + itemHeight);
                    if (!drawOnBack) return;
                    data = WeaponDrawInfo.modDraw_BackWeapon(data, drawPlayer, larger);
                }
                //Add the weapon to the draw layers
                Main.playerDrawData.Add(data);
                WeaponDrawInfo.drawGlowLayer(data, drawPlayer, heldItem);
            }

            if (heldItem.useStyle == 4 || //hold up
                heldItem.useStyle == 5)   //hold out
            {
                bool isAStaff = Item.staff[heldItem.type];
                //staves, guns and bows
                if (gHeight >= gWidth * 1.2f && !isAStaff)
                {
                    //|    ######       
                    //|    ##  ######   
                    //|    ##    ##  ##  
                    //|    ##    ##  ## 
                    //|    ##    ##  ## 
                    //|    ##    ##  ## 
                    //|    ##  ######   
                    //|    ######       
                    //bows
                    if (drawPlayer.grapCount > 0) return; // can't see while grappling
                    if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(bow): " + itemWidth + " x " + itemHeight);
                    if (drawOnBack) return;
                    data = WeaponDrawInfo.modDraw_ForwardHoldWeapon(data, drawPlayer, lesser);
                }
                else if (gWidth >= gHeight * 1.2f && !isAStaff)
                {
                    if (heldItem.noUseGraphic && heldItem.melee)
                    {
                        //|                 
                        //|    ####         
                        //|  ##  ########## 
                        //|  ####    ##    ####
                        //|  ##  ##  ##        ####
                        //|  ##      ##  ######
                        //|    ############ 
                        //|                 
                        //drills, chainsaws
                        if (drawPlayer.grapCount > 0) return; // can't see while grappling
                        if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(drill): " + itemWidth + " x " + itemHeight);
                        if (drawOnBack) return;
                        data = WeaponDrawInfo.modDraw_DrillWeapon(data, drawPlayer, larger);
                    }
                    else
                    {
                        if (larger < 45)
                        {
                            //| ####        ####
                            //| ##  ########  ##
                            //|   ####        ##
                            //|   ##    ########
                            //|   ##  ##  ##      
                            //|   ##  ####        
                            //|   ######          
                            //|                 
                            if (drawPlayer.grapCount > 0) return; // can't see while grappling
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(pistol): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            //small aimed weapons (like handgun/aquasceptre) held halfway down, 1/3 back
                            data = WeaponDrawInfo.modDraw_AimedWeapon(data, drawPlayer, larger);
                        }
                        else
                        {
                            //|                 
                            //|               ##
                            //| ######################
                            //| ##  ##      ##  ##
                            //| ##  ############
                            //| ####  ##    ##  
                            //|     ####    ##  
                            //|                 
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(rifle): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            //large guns (rifles, launchers, etc.) held with both hands
                            data = WeaponDrawInfo.modDraw_HeavyWeapon(data, drawPlayer, lesser);
                        }
                    }
                }
                else
                {
                    if (heldItem.noUseGraphic && !isAStaff)
                    {
                        if (!heldItem.autoReuse)
                        {
                            if (drawPlayer.grapCount > 0) return; // can't see while grappling
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(polearm): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            if (isYoyo)
                            {
                                //sam (?why did i write sam? maybe same?)
                                data = WeaponDrawInfo.modDraw_HandWeapon(data, drawPlayer, larger, lesser, isYoyo);
                            }
                            else
                            {
                                //|             ####
                                //|         ####  ##
                                //|       ##    ##  
                                //|         ##  ##  
                                //|       ##  ##    
                                //|     ##          
                                //|   ##            
                                //| ##              
                                //spears are held facing to the floor, maces generally held
                                data = WeaponDrawInfo.modDraw_PoleWeapon(data, drawPlayer, larger);
                            }
                        }
                        else
                        {
                            //nebula blaze, flairon, solar eruption (too inconsistent)
                            if (larger <= 48)
                            {
                                if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(waist safe): " + itemWidth + " x " + itemHeight);
                                if (!drawOnBack) return;
                                data = WeaponDrawInfo.modDraw_WaistWeapon(data, drawPlayer, larger);
                            }
                            else
                            {
                                if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(back safe): " + itemWidth + " x " + itemHeight);
                                if (!drawOnBack) return;
                                data = WeaponDrawInfo.modDraw_BackWeapon(data, drawPlayer, larger);
                            }
                        }
                    }
                    else
                    {
                        if (larger + lesser <= 72) //only smallest magic weapons
                        {
                            //|         ######  
                            //|       ##  ##  ##
                            //|     ##      ##  ##
                            //|   ##        ######
                            //| ##        ##  ##
                            //| ##      ##  ##  
                            //|   ##  ##  ##    
                            //|     ######
                            if (drawPlayer.grapCount > 0) return; // can't see while grappling
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(hand magic): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            data = WeaponDrawInfo.modDraw_HandWeapon(data, drawPlayer, larger, lesser);
                        }
                        else if (lesser <= 42) //medium sized magic weapons, treated like polearms
                        {
                            if (drawPlayer.grapCount > 0) return; // can't see while grappling
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(polearm magic): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            data = WeaponDrawInfo.modDraw_PoleWeapon(data, drawPlayer, larger);
                        }
                        else
                        {
                            //|                 
                            //|             ####
                            //|   ############  ##
                            //| ##        ##      ##
                            //|   ############  ##
                            //|             ####
                            //|                 
                            //|                 
                            if (drawPlayer.grapCount > 0) return; // can't see while grappling
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(staff): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            //staves
                            data = WeaponDrawInfo.modDraw_MagicWeapon(data, drawPlayer, larger);
                        }
                    }
                }
                //Add the weapon to the draw layers
                Main.playerDrawData.Add(data);
                WeaponDrawInfo.drawGlowLayer(data, drawPlayer, heldItem);
                //largestaves are held straight up
            }
            #endregion

            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "[]: " + itemWidth + " x " + itemHeight, 100, 200, 150);

        }
        
        /// <summary>
        /// Weak reference, must wrap in try catch exception becase won't catch FileNotFoundException
        /// </summary>
        /// <param name="drawInfo"></param>
        /// <param name="item"></param>
        /// <param name="shader"></param>
        private static void ItemCustomizerIntegration(PlayerDrawInfo drawInfo, Item item, ref int shader)
        {
            if (!Main.dedServ)
            {
                Mod itemCustomizer = itemCustomizer = ModLoader.GetMod("ItemCustomizer");
                if (itemCustomizer != null)
                {
                    CustomizerItemInfo cii = item.GetModInfo<CustomizerItemInfo>(itemCustomizer);
                    shader = cii.shaderID;
                }
            }
        }

        #region Hurt Methods

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            ShieldPreHurt(damage, crit, hitDirection);

            if (ModConf.enableFists)
            {
                if (ParryPreHurt(damageSource)) return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="damageSource"></param>
        /// <returns>True when attack is parried</returns>
        private bool ParryPreHurt(PlayerDeathReason damageSource)
        {
            // Caused by normal damage?
            if (damageSource.SourceNPCIndex >= 0 || damageSource.SourcePlayerIndex >= 0 || damageSource.SourceProjectileIndex >= 0)
            {
                // Stop this attack and parry with cooldown
                if (IsParryActive)
                {
                    player.itemAnimation = 0; //release item lock

                    int timeSet = parryActive;

                    //set cooldown to prevent spam
                    parryTime = timeSet * -3;
                    parryActive = 0;
                    parryTimeMax = 0;

                    // Strike the NPC away slightly
                    if(damageSource.SourceNPCIndex >= 0)
                    {
                        NPC npc = Main.npc[damageSource.SourceNPCIndex];
                        int hitDirection = player.direction;
                        float knockback = 4f;
                        if (npc.knockBackResist > 0)
                        {
                            knockback /= npc.knockBackResist;
                        }
                        npc.StrikeNPC(npc.defense, knockback, player.direction, false, false, false);
                        if (Main.netMode != 0)
                        {
                            NetMessage.SendData(28, -1, -1, "", npc.whoAmI, (float)npc.defense, (float)knockback, (float)hitDirection, 0, 0, 0);
                        }
                    }
                    else
                    {
                        Main.PlaySound(SoundID.NPCHit3, player.position);
                        if(damageSource.SourceProjectileIndex >= 0)
                        {
                            ProjFX.ReflectProjectilePlayer(
                                Main.projectile[damageSource.SourceProjectileIndex],
                                player,
                                this,
                                false);
                        }
                    }

                    // Add 5 sec parry buff and short invincibility
                    Items.Weapons.UseStyles.FistStyle.provideImmunity(player, 20);
                    player.AddBuff(mod.BuffType<Buffs.ParryActive>(), 300, false);

                    if (DEBUG_PARRYFISTS) Main.NewText(string.Concat("Parried! : ", parryTime, "/", parryActive, "/", parryTimeMax));
                    WeaponOut.NetUpdateParry(mod, this);
                    return true;
                }
            }
            return false;
        }

        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            ShieldBounceNPC(npc);
        }

        private void ShieldPreHurt(int damage, bool crit, int hitDirection)
        {
            if (DamageKnockbackThreshold > 0)
            {
                if (crit) damage *= 2;
                damage = (int)Main.CalculatePlayerDamage(damage, player.statDefense);
                //Main.NewText("Took damage: " + damage + " vs " + DamageKnockbackThreshold);
                if(Main.expertMode)
                { if (damage <= DamageKnockbackThreshold) player.noKnockback = true; }
                else
                { if (damage <= DamageKnockbackThreshold * Main.expertNPCDamage) player.noKnockback = true; }
               
            }

            if (player.direction != hitDirection)
            {
                if (FrontDefence > 0) player.statDefense += FrontDefence;
                //Main.NewText("DEF " + player.statDefense + " | " + FrontDefence);
                if (frontNoKnockback) player.noKnockback = true;
            }
        }
        private void ShieldBounceNPC(NPC npc)
        {
            //ignore if not facing
            if (player.direction == 1 && npc.Center.X < player.Center.X) return;
            if (player.direction == -1 && npc.Center.X > player.Center.X) return;

            //bump if not attacking
            if (player.whoAmI == Main.myPlayer && player.itemAnimation == 0
                && !player.immune && frontNoKnockback && !npc.dontTakeDamage)
            {
                int hitDamage = 1;
                float knockBack = (Math.Abs(player.velocity.X * 2) + 2f) / (0.2f + npc.knockBackResist); //sclaing knockback with kbr
                int hitDirection = player.direction;
                npc.StrikeNPC(hitDamage, (float)knockBack, hitDirection, false, false, false);
                if (Main.netMode != 0)
                {
                    NetMessage.SendData(28, -1, -1, "", npc.whoAmI, (float)hitDamage, (float)knockBack, (float)hitDirection, 0, 0, 0);
                }
            }
        }

        #endregion
    }
}