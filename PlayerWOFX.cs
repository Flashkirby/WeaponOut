using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

using Terraria.ModLoader.IO;
using System.IO;
//using Terraria.Graphics.Shaders;
//vs collapse all Ctrl-M-O

namespace WeaponOut//Lite
{
    public class PlayerWOFX : ModPlayer
    {
        private const bool DEBUG_WEAPONHOLD = false;
        private const bool DEBUG_BOOMERANGS = false;
        private static Mod itemCustomizer;

        #region Weapon Holding
        public bool weaponVisual = true;
        public bool WeaponVisual { get { return weaponVisual || ModConf.ForceShowWeaponOut; } }
        public int weaponFrame;//frame of weapon...
        #endregion

        public override void PostUpdate() {
            manageBodyFrame();
        }
        private void manageBodyFrame() {
            //if (WeaponOutLite.Disabled) return;
            if (Main.netMode == 2) return; // Oh yeah, server calls this so don't pls

            //change idle pose for player using a heavy weapon
            //copypasting from drawPlayerItem
            Item heldItem = player.inventory[player.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0 || !ModConf.ShowWeaponOut) return; //no item so nothing to show
            Texture2D weaponTex = weaponTex = Main.itemTexture[heldItem.type];
            if (weaponTex == null) return; //no texture to item so ignore too
            float itemWidth = weaponTex.Width * heldItem.scale;
            float itemHeight = weaponTex.Height * heldItem.scale;
            if (heldItem.modItem != null) {
                if (Main.itemAnimations[heldItem.type] != null) {
                    itemHeight /= Main.itemAnimations[heldItem.type].FrameCount;
                }
            }
            float larger = Math.Max(itemWidth, itemHeight);
            int playerBodyFrameNum = player.bodyFrame.Y / player.bodyFrame.Height;
            // Is a long gun
            HoldType customHold = getCustomHoldType(heldItem);
            if (customHold == HoldType.LargeGun
                ||
                ( customHold == HoldType.None // only when none set
                && (heldItem.useStyle == 5
                && weaponTex.Width >= weaponTex.Height * 1.2f
                && (!heldItem.noUseGraphic || !heldItem.melee)
                && larger >= 45
                && WeaponVisual)) //toggle with accessory1 visibility, or forceshow is on
            ) {
                if (playerBodyFrameNum == 0) player.bodyFrame.Y = 10 * player.bodyFrame.Height;
            }
        }

        public override void OnEnterWorld(Player player) {
            //if (WeaponOutLite.Disabled) return;
            itemCustomizer = ModLoader.GetMod("ItemCustomizer");

            // Update visuals
            WeaponOut.NetUpdateWeaponVisual(mod, this);
        }

        #region Save and Load
        public override TagCompound Save() {
            //if (WeaponOutLite.Disabled) return base.Save();
            return new TagCompound
            {
                { "weaponVisual", weaponVisual }
            };
        }
        public override void Load(TagCompound tag) {
            //if (WeaponOutLite.Disabled) return;
            weaponVisual = tag.GetBool("weaponVisual");
        }
        #endregion

        #region Player Layers
        public static readonly PlayerLayer HeldItem = new PlayerLayer("WeaponOut", "HeldItem", PlayerLayer.HeldItem, delegate (PlayerDrawInfo drawInfo) {
            if (drawInfo.shadow != 0f) {
                return;
            }
            try {
                drawPlayerItem(drawInfo, false);
            }
            catch { }
        });
        public static readonly PlayerLayer HairBack = new PlayerLayer("WeaponOut", "HairBack", PlayerLayer.HairBack, delegate (PlayerDrawInfo drawInfo) {
            if (drawInfo.shadow != 0f) {
                return;
            }
            try {
                drawPlayerItem(drawInfo, true);
            }
            catch { }
        });

        #endregion
        #region draw
        public bool hidden = false;
        public override void ModifyDrawLayers(List<PlayerLayer> layers) {
            //if (WeaponOutLite.Disabled) return;
            if (hidden) {
                hidden = false;
                foreach (PlayerLayer l in layers) {
                    l.visible = false;
                }
                return;
            }

            #region Show fists with weapon visuals

            if (WeaponVisual) {
                if (player.HeldItem.useStyle == ModPlayerFists.useStyle) {
                    if (player.HeldItem.handOnSlot > 0) {
                        player.handon = player.HeldItem.handOnSlot;
                        player.cHandOn = 0;
                    }
                    if (player.HeldItem.handOffSlot > 0) {
                        player.handoff = player.HeldItem.handOffSlot;
                        player.cHandOff = 0;
                    }
                    if (player.itemAnimation > 0) layers.Remove(PlayerLayer.HeldItem); // hide fist when attacking
                    return;
                }
            }

            #endregion
            
            if (player.itemAnimation > 0) return; // No show when swinging

            HeldItem.visible = true; // For items held in hand
            HairBack.visible = true; // For items behind the player (sheathed)
            //MiscEffectsFront.visible = !player.dead;
            try {
                int heldItemStack = layers.IndexOf(PlayerLayer.HeldItem);
                int hairBackStack = layers.IndexOf(PlayerLayer.HairBack);
                //int MiscEffectsFrontStack = layers.IndexOf(PlayerLayer.MiscEffectsFront);
                if (heldItemStack >= 0) layers.Insert(heldItemStack, HeldItem);
                if (hairBackStack >= 0) layers.Insert(hairBackStack, HairBack);
            }
            catch { }
            //layers.Insert(MiscEffectsFrontStack, MiscEffectsFront);
        }

        internal static HoldType getCustomHoldType(Item item)
        {
            int style;
            if (ModConfWeaponOutCustom.TryGetCustomHoldStyle(item.type, out style))
            {
                return setHoldTypeSafeWrapping(style);
            }
            return HoldType.None;
        }
        internal static HoldType setHoldTypeSafeWrapping(int style)
        {
            if (style > HoldTypeCount)
            { style = (int)HoldType.None; }
            if (style < (int)HoldType.None)
            { style = HoldTypeCount; }
            return (HoldType)style;
        }

        /// <summary>
        /// We gonna handle all the weapon identification and calls here
        /// </summary>
        /// <param name="drawInfo"></param>
        /// <param name="drawOnBack"></param>
        private static void drawPlayerItem(PlayerDrawInfo drawInfo, bool drawOnBack) {
            //don't draw when not ingame
            if (Main.gameMenu || !ModConf.ShowWeaponOut) return;

            //get player player
            Player drawPlayer = drawInfo.drawPlayer;

            //hide if dead, stoned etc.
            if (!drawPlayer.active || drawPlayer.dead || drawPlayer.stoned) return;

            try {
                if (//do nothing if player is doing something
                    drawPlayer.itemAnimation > 0 ||
                    //also hide if visual is disabled
                    (!ModConf.ForceShowWeaponOut && !drawPlayer.GetModPlayer<PlayerWOFX>().weaponVisual)) {
                    return;
                }
            }
            catch { }

            //player player's held item
            Item heldItem = drawPlayer.inventory[drawPlayer.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0) return; //no item so nothing to show

            //ignore projectile melee weapons
            bool isYoyo = false;
            // items work when checked at least once in singleplayer first...?
            if (DEBUG_BOOMERANGS) Main.NewText("Shoot is " + heldItem.shoot + " (!=0)");
            if (heldItem.shoot != 0) {
                if (DEBUG_BOOMERANGS) Main.NewText("heldItem.melee = " + heldItem.melee);
                if (DEBUG_BOOMERANGS) Main.NewText("heldItem.noMelee = " + heldItem.noMelee);
                if (heldItem.melee && heldItem.noMelee) {
                    for (int i = 0; i < Main.projectile.Length; i++) {
                        if (!Main.projectile[i].active) continue;
                        if (Main.projectile[i].owner == drawPlayer.whoAmI &&
                            Main.projectile[i].melee) {
                            return;
                        }
                    }
                }

                //  YOYO is aiStyle 99
                Projectile p = new Projectile();
                p.SetDefaults(heldItem.shoot);
                if (p.aiStyle == 99) {
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
            if (heldItem.modItem != null) {
                if (Main.itemAnimations[heldItem.type] != null) // in the case of modded weapons with animations...
                {
                    //get local player frame counting
                    PlayerWOFX p = drawPlayer.GetModPlayer<PlayerWOFX>();
                    int frameCount = Main.itemAnimations[heldItem.type].FrameCount;
                    int frameCounter = Main.itemAnimations[heldItem.type].TicksPerFrame * 2;

                    //add them up
                    if (Main.time % frameCounter == 0) {
                        p.weaponFrame++;
                        if (p.weaponFrame >= frameCount) {
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
            if (drawPlayer.gravDir < 0) {
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
            if (itemCustomizer != null) {
                data.shader = ItemCustomizerGetShader(itemCustomizer, heldItem);
            }

            //work out what type of weapon it is!
            #region Weapon Algorithm
            float itemWidth = gWidth * heldItem.scale;
            float itemHeight = gHeight * heldItem.scale;
            //not all items have width/height set the same, so use largest as "length" including weapon sizemod
            float larger = Math.Max(itemWidth, itemHeight);
            float lesser = Math.Min(itemWidth, itemHeight);

            PickItemDrawType(drawOnBack, drawPlayer, heldItem, isYoyo, gWidth, gHeight, ref data, itemWidth, itemHeight, larger, lesser);
            #endregion

            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "[]: " + itemWidth + " x " + itemHeight, 100, 200, 150);

        }

        public enum HoldType
        { None, Hand, Waist, Back, Spear, PowerTool, Bow, SmallGun, LargeGun, Staff }
        private static readonly bool[] holdTypeHideOnBack = new bool[] {
            true, true, false, false, true, true, true, true, true, true
        };
        public static readonly int HoldTypeCount = (int)HoldType.Staff;
        private static void PickItemDrawType(bool drawOnBack, Player drawPlayer, Item heldItem, bool isYoyo, int gWidth, int gHeight, ref DrawData data, float itemWidth, float itemHeight, float larger, float lesser) {
            HoldType holdType = HoldType.None;
            int style;
            if (ModConfWeaponOutCustom.TryGetCustomHoldStyle(heldItem.type, out style))
            {
                holdType = getCustomHoldType(heldItem);
            }
            else
            {
                #region AutoPicker
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
                        if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(hand): " + itemWidth + " x " + itemHeight);
                        holdType = HoldType.Hand;
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
                    //They are worn on the waist, and react to falling! Except when disabled
                    //This also amusingly applies to ducks, axes and rockfish
                    //But shouldn't apply to pickaxes, except when they are also not pickaxes
                    else if (larger <= 48 &&
                        (heldItem.pick <= 0 ||
                        (heldItem.pick > 0 && heldItem.axe > 0)))
                    {
                        if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(waist): " + itemWidth + " x " + itemHeight);
                        holdType = HoldType.Waist;
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
                        holdType = HoldType.Back;
                    }
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
                        if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(bow): " + itemWidth + " x " + itemHeight);
                        holdType = HoldType.Bow;
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
                            if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(drill): " + itemWidth + " x " + itemHeight);
                            holdType = HoldType.PowerTool;
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
                                //small aimed weapons (like handgun/aquasceptre) held halfway down, 1/3 back
                                holdType = HoldType.SmallGun;
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
                                //large guns (rifles, launchers, etc.) held with both hands
                                holdType = HoldType.LargeGun;
                            }
                        }
                    }
                    else
                    {
                        if (heldItem.noUseGraphic && !isAStaff)
                        {
                            if (!heldItem.autoReuse)
                            {
                                if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(polearm): " + itemWidth + " x " + itemHeight);
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
                                    holdType = HoldType.Spear;
                                }
                            }
                            else
                            {
                                //nebula blaze, flairon, solar eruption (too inconsistent)
                                if (larger <= 48)
                                {
                                    if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(waist safe): " + itemWidth + " x " + itemHeight);
                                    holdType = HoldType.Waist;
                                }
                                else
                                {
                                    if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(back safe): " + itemWidth + " x " + itemHeight);
                                    holdType = HoldType.Back;
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
                                if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(hand magic): " + itemWidth + " x " + itemHeight);
                                holdType = HoldType.Hand;
                            }
                            else if (lesser <= 42) //medium sized magic weapons, treated like polearms
                            {
                                if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(polearm magic): " + itemWidth + " x " + itemHeight);
                                holdType = HoldType.Spear;
                            }
                            else
                            { //largestaves are held straight up
                              //|                 
                              //|             ####
                              //|   ############  ##
                              //| ##        ##      ##
                              //|   ############  ##
                              //|             ####
                              //|                 
                              //|                 
                                if (DEBUG_WEAPONHOLD && drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(staff): " + itemWidth + " x " + itemHeight);
                                //staves
                                holdType = HoldType.Staff;
                            }
                        }
                    }
                }
                #endregion

                // Get any custom hold if no override was set
                foreach (var drawMethod in WeaponOut.mod.weaponOutCustomHoldMethods)
                {
                    holdType = setHoldTypeSafeWrapping(drawMethod(drawPlayer, heldItem, (int)holdType));
                }
            }


            // can't see non-backed while grappling
            if (holdTypeHideOnBack[(int)holdType] && drawPlayer.grapCount > 0) return;
            // Only draw back items when it is a back item hold type
            if (holdTypeHideOnBack[(int)holdType] == drawOnBack) return;

            switch (holdType) {
                case HoldType.Hand: data = WeaponDrawInfo.modDraw_HandWeapon(data, drawPlayer, larger, lesser); break;
                case HoldType.Waist: data = WeaponDrawInfo.modDraw_WaistWeapon(data, drawPlayer, larger); break;
                case HoldType.Spear: data = WeaponDrawInfo.modDraw_PoleWeapon(data, drawPlayer, larger); break;
                case HoldType.PowerTool: data = WeaponDrawInfo.modDraw_DrillWeapon(data, drawPlayer, larger); break;
                case HoldType.Back: data = WeaponDrawInfo.modDraw_BackWeapon(data, drawPlayer, larger); break;
                case HoldType.Bow: data = WeaponDrawInfo.modDraw_ForwardHoldWeapon(data, drawPlayer, lesser); break;
                case HoldType.SmallGun: data = WeaponDrawInfo.modDraw_AimedWeapon(data, drawPlayer, larger); break;
                case HoldType.LargeGun: data = WeaponDrawInfo.modDraw_HeavyWeapon(data, drawPlayer, lesser); break;
                case HoldType.Staff: data = WeaponDrawInfo.modDraw_MagicWeapon(data, drawPlayer, larger); break;
                default: return;
            }


            // Run custom draws

            bool allowDraw = true;
            foreach (var drawMethod in WeaponOut.mod.weaponOutCustomPreDrawMethods)
            {
                if (!drawMethod(drawPlayer, heldItem, data))
                { allowDraw = false; }
            }

            // Attempt standard draw if allowed 
            if (allowDraw)
            {
                //Add the weapon to the draw layers
                Main.playerDrawData.Add(data);
                WeaponDrawInfo.drawGlowLayer(data, drawPlayer, heldItem);
            }
        }

        private static int ItemCustomizerGetShader(Mod mod, Item item) {
            if (!Main.dedServ) {
                try {
                    GlobalItem cii = item.GetGlobalItem(mod, "CustomizerItem");

                    // The field we're looking for
                    var shaderIDInfo = cii.GetType().GetField("shaderID");

                    // Check this field on this class
                    int shaderID = (int)shaderIDInfo.GetValue(cii);

                    // We got this
                    return shaderID;
                }
                catch { }
            }
            return 0;
        }
        #endregion

    }
}