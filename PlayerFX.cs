using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
//using Terraria.Graphics.Shaders;
//vs collapse all Ctrl-M-O

namespace WeaponOut
{
    public class PlayerFX : ModPlayer
    {
        private const bool DEBUG_WEAPONHOLD = false;
        /* //disabled for now 
        private const int shieldDelayReset = 120; //delay after attacking or losing
        private const int shieldDelayPause = 60; //delay after recieving a hit
        private const int shieldCounterBase = 4; //tick counter base
        private const int shieldAlphaDelay = 30; //stay time
        private const int shieldAlphaDown = 5; //tick alpha
        private const int heartsPerDefence = 10; //number of shield points per heart
        */

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

        public bool lunarRangeVisual;
        public bool lunarMagicVisual;
        public bool lunarThrowVisual;

        /*
        public Item shieldItem;
        public int shieldLastBlock;
        public int shieldBlock; //current block points
        public int shieldBlockMax; //max block of equipped shield
        public int shieldRegenDelay; //at 0, shield can regen
        public int shieldRegenCounter; //at 0, shield goes up 1 point
        private int shieldGraphicDelay; //disappear at full charge
        private int shieldGraphicAlpha; //disappear at full charge
        */

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
        #endregion

        public override void Initialize()
        {
            lastSelectedItem = 0;

            itemSkillDelay = 0;
            dashingSpecialAttack = 0;

            localTempSpawn = new Vector2();
            //shieldGraphicAlpha = 0;

            if (ModConf.enableFists)
            {
                openFist = mod.ItemType("Fist");
                fireFistType = mod.ItemType("FistsOfFury");
            }
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
                    itemFlashFX(175);
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
        }

        public override void PreUpdate()
        {
            //setShieldBlock();
            //handleShieldBlockRecovery();
            itemCooldownFlash();
        }
        /*
        private void setShieldBlock()
        {
            //shieldBlockMax = 0;
            int shieldPower = 0;
            //accessory slots

            for (int i = 3; i < 8 + player.extraAccessorySlots; i++ )
            {
                Item check = player.armor[i];
                shieldPower = validateIsShield(check);
                //Main.NewText("shieldpower of " + check.name + ": " + shieldPower, 100, 0, 255);

                if (shieldPower > shieldBlockMax)
                {
                    shieldBlockMax = shieldPower;
                    shieldItem = check;
                }
            }
            if (shieldBlockMax == 0) shieldItem = new Item();

            //manage alpha and sound
            if (shieldBlock != shieldLastBlock)
            {
                shieldGraphicDelay = shieldAlphaDelay;
                shieldGraphicAlpha = 0;
                //play sound at max shield
                if (shieldBlock == shieldBlockMax && 
                    player.whoAmI == Main.myPlayer &&
                    shieldBlockMax > 0)
                {
                    Main.PlaySound(2, -1, -1, 53);
                }
            }
            else
            {
                //reached regen full/interrupted
                if (shieldGraphicAlpha < 255)
                {
                    if (shieldGraphicDelay <= 0)
                    {
                        shieldGraphicAlpha += shieldAlphaDown;
                    }
                    else
                    {
                        shieldGraphicDelay--;
                    }
                }
                else
                {
                    shieldGraphicAlpha = 255;
                }
            }
            shieldLastBlock = shieldBlock;
        }
        private int validateIsShield(Item check)
        {
            //give bonus from prefixes
            int prefixBonus = 0;
            if (check.prefix == 62) prefixBonus++;
            if (check.prefix == 63) prefixBonus += 2;
            if (check.prefix == 64) prefixBonus += 3;
            if (check.prefix == 65) prefixBonus += 4;

            //check vanilla shields first
            if (check.type == 156 || //cobalt shield
                check.type == 397 || //obsidian shield
                check.type == 938 || //paladin
                check.type == 1613   //ankh
                ) 
            {

                if (check.defense > 0) return heartsPerDefence * (check.defense + prefixBonus);
            }

            //check things that are called shields, and shield of cthulu
            if (check.name.Contains("Shield") || 
                check.type == 3097 || //shield of cthulu
                check.shieldSlot > 0) //anything else that sits in the shield slot
            {
                if (player.noKnockback)
                {
                    //probably the shield is giving knockback immunity so assume its player
                    return heartsPerDefence * (check.defense + prefixBonus);
                }
                else
                {
                    //weaker since they don't give kb immunity
                    return (heartsPerDefence * (check.defense + prefixBonus)) / 2;
                }
            }
            return 0;
        }
        private void handleShieldBlockRecovery()
        {
            if (player.itemAnimation > 0 || player.dead)
            {
                //delay when player makes an active move
                shieldRegenDelay = shieldDelayReset;
                shieldRegenCounter = shieldCounterBase;
            }
            else
            {
                if (shieldRegenDelay <= 0)
                {
                    //regen shield
                    shieldRegenDelay = 0;
                    if (shieldBlock < shieldBlockMax)
                    {
                        //count down to 0
                        shieldRegenCounter--;
                        if (shieldRegenCounter <= 0)
                        {
                            //reset counter
                            shieldRegenCounter = shieldCounterBase;
                            //replenish shield, double if below half
                            shieldBlock++;
                            if (shieldBlock < shieldBlockMax / 2) shieldBlock++;
                            //Main.NewText("Shield: " + shieldBlock, 100, 255, 100);
                        }
                    }
                    if (shieldBlock > shieldBlockMax)
                    {
                        shieldBlock = shieldBlockMax;
                    }
                }
                else
                {
                    //countdown
                    shieldRegenDelay--;
                }
            }
        }
        */ 

        /// <summary>
        /// UseItem flash when itemTime is 0 to indicate charge
        /// </summary>
        private void itemCooldownFlash()
        {
            if (player.itemTime == 1 && player.whoAmI == Main.myPlayer)
            {
                int itemT = player.inventory[player.selectedItem].type;
                if (itemT == fireFistType && ModConf.enableFists)
                {
                    itemFlashFX();
                }
            }
        }
        private void itemFlashFX(int dustType = 45)
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

        public override bool PreItemCheck()
        {
            if(ModConf.enableBasicContent)
            {
                applyBannerBuff();
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
            foreach (Player p in Main.player)
            {
                int itemType = p.inventory[p.selectedItem].type;
                if (itemType != mod.ItemType<Items.RallyBannerBlue>() &&
                    itemType != mod.ItemType<Items.RallyBannerGreen>() &&
                    itemType != mod.ItemType<Items.RallyBannerRed>() &&
                    itemType != mod.ItemType<Items.RallyBannerYellow>()
                    ) continue; //only use these banner items

                if(p.team == player.team)
                {
                    bool noTeam = player.team == 0;
                    if (
                        // on my own team, only me
                        (noTeam && p.whoAmI == player.whoAmI)
                        || // or
                           // in range of team mate
                        (
                            !noTeam &&
                            p.position.X >= player.position.X - Buffs.RallyBanner.buffRadius &&
                            p.position.X <= player.position.X + Buffs.RallyBanner.buffRadius &&
                            p.position.Y >= player.position.Y - Buffs.RallyBanner.buffRadius &&
                            p.position.Y <= player.position.Y + Buffs.RallyBanner.buffRadius
                            )
                        )
                    {
                        player.AddBuff(mod.BuffType<Buffs.RallyBanner>(), 2);
                    }
                }
            }
        }

        public override void PostUpdateRunSpeeds()
        {
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
                !player.hideVisual[3] || ModConf.forceShowWeaponOut
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
        /*
        public static readonly PlayerLayer MiscEffectsFront = new PlayerLayer("WeaponOut", "MiscEffectsFront", PlayerLayer.MiscEffectsFront, delegate(PlayerDrawInfo drawInfo)
        {
            try
            {
                drawShieldOver(drawInfo);
            }
            catch { }
        });
        */ 
        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            HeldItem.visible = true;
            HairBack.visible = true;
            //MiscEffectsFront.visible = !player.dead;
            int heldItemStack = layers.IndexOf(PlayerLayer.HeldItem);
            int hairBackStack = layers.IndexOf(PlayerLayer.HairBack);
            int MiscEffectsFrontStack = layers.IndexOf(PlayerLayer.MiscEffectsFront);
            layers.Insert(heldItemStack, HeldItem);
            layers.Insert(hairBackStack, HairBack);
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

            if (drawPlayer.itemAnimation > 0 //do nothing if player is doing something
                || (drawPlayer.hideVisual[3] && !ModConf.forceShowWeaponOut)) return; //also hide if accessory 1 is hidden

            //player player's held item
            Item heldItem = drawPlayer.inventory[drawPlayer.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0) return; //no item so nothing to show

            //ignore projectile melee weapons
            bool isYoyo = false; ;
            // items work when checked at least once in singleplayer first...?
            if (heldItem.shoot != 0)
            {
                if (heldItem.melee && heldItem.noMelee)
                {
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
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

            //work out what type of weapon it is!
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

            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "[]: " + itemWidth + " x " + itemHeight, 100,200,150);
            
        }


        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            ShieldPreHurt(damage, crit, hitDirection);
            return true;
        }

        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            ShieldBounceNPC(npc);
        }

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            ModifyHitAny(item, ref damage, ref crit);
        }
        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            ModifyHitAny(item, ref damage, ref crit);
        }
        public void ModifyHitAny(Item item, ref int damage, ref bool crit)
        {
            if (item.type == openFist) damage += player.armor[1].defense;
        }

        private void ShieldPreHurt(int damage, bool crit, int hitDirection)
        {
            if (DamageKnockbackThreshold > 0)
            {
                if (crit) damage *= 2;
                damage = (int)Main.CalculatePlayerDamage(damage, player.statDefense);
                //Main.NewText("Took damage: " + damage + " vs " + DamageKnockbackThreshold);
                if (damage <= DamageKnockbackThreshold) player.noKnockback = true;
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
                && !player.immune && this.frontNoKnockback && !npc.dontTakeDamage)
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
    }
}
