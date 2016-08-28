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
        /* //disabled for now 
        private const int shieldDelayReset = 120; //delay after attacking or losing
        private const int shieldDelayPause = 60; //delay after recieving a hit
        private const int shieldCounterBase = 4; //tick counter base
        private const int shieldAlphaDelay = 30; //stay time
        private const int shieldAlphaDown = 5; //tick alpha
        private const int heartsPerDefence = 10; //number of shield points per heart
        */

        private bool wasDead; //used to check if player just revived
        private int openFist; //item ID of Fist Weapon
        private int fireFistType;

        public Vector2 localTempSpawn;

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

        public int weaponFrame;

        public override void Initialize()
        {
            localTempSpawn = new Vector2();
            //shieldGraphicAlpha = 0;

            openFist = mod.ItemType("Fist");
            fireFistType = mod.ItemType("FistsOfFury");
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
                    //probably the shield is giving knockback immunity so assume its this
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
                if (itemT == fireFistType)
                {
                    Main.PlaySound(25, -1, -1, 1);
                    for (int i = 0; i < 5; i++)
                    {
                        int d = Dust.NewDust(
                            player.position, player.width, player.height, 45, 0f, 0f, 255, 
                            default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
                        Main.dust[d].noLight = true;
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 0.5f;
                    }
                }
            }
        }
        /*
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref string deathText)
        {
            int originalDmg = damage;
            damage = generateBlockDamage(damage);
            if (originalDmg != damage)
            {
                if (shieldBlock == 0)
                { Main.PlaySound(4, player.position, 15); } //BREAK SHIELD 
                else
                {
                    if (originalDmg < shieldBlockMax / 3)
                    {
                        if (damage == 1)
                        { Main.PlaySound(3, player.position, 15); } //scratch
                        else { Main.PlaySound(3, player.position, 16); } //light
                    } 
                    else
                    { Main.PlaySound(3, player.position, 17); }//heavy
                    playSound = false;
                }
                //Main.NewText("Shield: " + shieldBlock, 255, 0, 100);
            }
            return true;
        }
        
        /// <summary>
        /// Reduces damage taken, and removes it from shield health
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        private int generateBlockDamage(int damage)
        {
            if (shieldBlock >= damage)
            {
                shieldBlock -= damage;
                damage = 1;
                shieldRegenDelay = shieldDelayPause;
                shieldRegenCounter = shieldCounterBase;
            }
            else
            {
                damage -= shieldBlock;
                if (shieldBlock > 0) //if first time block, create delay
                {
                    shieldRegenDelay = shieldDelayReset;
                    shieldRegenCounter = shieldCounterBase;
                }
                shieldBlock = 0;
            }
            return damage;
        }
        */
        public override bool PreItemCheck()
        {
            preItemTransformCheck();
            createBareFistInInv();
            return true;
        }
        private void preItemTransformCheck()
        {
            if (player.itemAnimation == 0)
            {
                Item item = player.inventory[player.selectedItem];
                try //effectively using this as a custom hook before ItemCheck for items
                {
                    if (item.modItem.mod.Name == mod.Name) { item.modItem.CanUseItem(player); }
                }
                catch { }
            }
        }
        private void createBareFistInInv()
        {
            if (player.inventory[player.selectedItem].type == 0 && player.controlUseItem)
            {
                player.inventory[player.selectedItem].SetDefaults(openFist);
            }
        }

        public override void PostItemCheck()
        {
            emptyBareFistFromInv();
        }
        private void emptyBareFistFromInv()
        {
            if (player.inventory[player.selectedItem].type == openFist && player.itemAnimation == 0)
            {
                player.inventory[player.selectedItem] = new Item();
            }
        }

        public override void PostUpdate()
        {
            manageBodyFrame();
            tentScript();
        }
        private void manageBodyFrame()
        {
            //change idle pose for player using a heavy weapon
            //copypasting from drawPlayerItem
            Item heldItem = player.inventory[player.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0) return; //no item so nothing to show
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
                && !player.hideVisual[3] //accessory1 must be visible
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

        public static void drawMagicCast(Player player, SpriteBatch spriteBatch, int frame)
        {
            Texture2D textureCasting = Main.extraTexture[51];
            Vector2 origin = player.Bottom + new Vector2(0f, player.gfxOffY + 4f);
            if (player.gravDir < 0) origin.Y -= player.height + 8f;
            Rectangle rectangle = textureCasting.Frame(1, 4, 0, Math.Max(0, Math.Min(3, frame)));
            Vector2 origin2 = rectangle.Size() * new Vector2(0.5f, 1f);
            if (player.gravDir < 0) origin2.Y = 0f;
            spriteBatch.Draw(
                textureCasting, new Vector2((float)((int)(origin.X - Main.screenPosition.X)), (float)((int)(origin.Y - Main.screenPosition.Y))),
                new Rectangle?(rectangle), player.GetHairColor(false), 0f, origin2, 1f,
                player.gravDir >= 0f ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
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
        private static void drawPlayerItem(PlayerDrawInfo drawInfo, bool drawOnBack)
        {
            //don't draw when not ingame
            if (Main.gameMenu) return;

            //get this player
            Player drawPlayer = drawInfo.drawPlayer;
            if (drawPlayer.itemAnimation > 0 //do nothing if player is doing something
                || drawPlayer.hideVisual[3]) return; //also hide if accessory 1 is hidden

            //this player's held item
            Item heldItem = drawPlayer.inventory[drawPlayer.selectedItem];
            if (heldItem == null || heldItem.type == 0 || heldItem.holdStyle != 0) return; //no item so nothing to show

            //ignore boomerangs with a projectile out
            bool isYoyo = false; ;
            if (heldItem.shoot != 0)
            {
                Projectile p = new Projectile();
                p.SetDefaults(heldItem.shoot);
                if (p.aiStyle == 3)
                {
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        if (!Main.projectile[i].active) continue;
                        if (Main.projectile[i].owner == heldItem.owner
                            && Main.projectile[i].aiStyle == 3)
                        {
                            return;
                        }
                    }
                }
                //  YOYO is aiStyle 99
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

            //items that GLOOOW
            if (heldItem.glowMask != -1)
            {
                DrawData glowData = new DrawData(
                   Main.glowMaskTexture[(int)heldItem.glowMask],
                   new Vector2(drawX, drawY),
                   sourceRect,
                   new Microsoft.Xna.Framework.Color(250, 250, 250, heldItem.alpha),
                   0f,
                   new Vector2(gWidth / 2f, gHeight / 2f),
                   scale,
                   spriteEffects,
                   0);
            }


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
                //Items, daggers and other throwables lie below 28 and are easily held in the hand
                if ((larger < 28 && !heldItem.magic) || //nonmagic weapons
                    (larger <= 32 && heldItem.shoot != 0) || //larger for throwing weapons
                    (larger <= 24 && heldItem.magic)) //only smallest magic weapons
                {
                    if (drawPlayer.grapCount > 0) return; // can't see while grappling
                    //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(hand): " + itemWidth + " x " + itemHeight);
                    if (drawOnBack) return;
                    data = modDraw_HandWeapon(data, drawPlayer, larger, lesser);
                }
                //Broadsword weapons are swing type weapons between 28 - 48
                //They are worn on the waist, and react to falling!
                else if (larger <= 48)
                {
                    //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(waist): " + itemWidth + " x " + itemHeight);
                    if (!drawOnBack) return;
                    data = modDraw_WaistWeapon(data, drawPlayer, larger);
                }
                //Great weapons are swing type weapons past 36 in size and slung on the back
                else
                {
                    //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(back): " + itemWidth + " x " + itemHeight);
                    if (!drawOnBack) return;
                    data = modDraw_BackWeapon(data, drawPlayer, larger);
                }
                //Add the weapon to the draw layers
                Main.playerDrawData.Add(data);
                drawGlowLayer(data, heldItem.glowMask, heldItem.alpha);
            }

            if (heldItem.useStyle == 4 || //hold up
                heldItem.useStyle == 5)   //hold out
            {
                bool isAStaff = Item.staff[heldItem.type];
                //staves, guns and bows
                if (gHeight >= gWidth * 1.2f && !isAStaff)
                {
                    //bows
                    if (drawPlayer.grapCount > 0) return; // can't see while grappling
                    //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(bow): " + itemWidth + " x " + itemHeight);
                    if (drawOnBack) return;
                    data = modDraw_ForwardHoldWeapon(data, drawPlayer, lesser);
                }
                else if (gWidth >= gHeight * 1.2f && !isAStaff)
                {
                    if (heldItem.noUseGraphic && heldItem.melee)
                    {
                        //drills, chainsaws
                        if (drawPlayer.grapCount > 0) return; // can't see while grappling
                        //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(drill): " + itemWidth + " x " + itemHeight);
                        if (drawOnBack) return;
                        data = modDraw_DrillWeapon(data, drawPlayer, larger);
                    }
                    else
                    {
                        if (larger < 45)
                        {
                            if (drawPlayer.grapCount > 0) return; // can't see while grappling
                            //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(pistol): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            //small aimed weapons (like handgun/aquasceptre) held halfway down, 1/3 back
                            data = modDraw_AimedWeapon(data, drawPlayer, larger);
                        }
                        else
                        {
                            //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(gun): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            //large guns (rifles, launchers, etc.) held with both hands
                            data = modDraw_HeavyWeapon(data, drawPlayer, lesser);
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
                            //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(polearm): " + itemWidth + " x " + itemHeight);
                            if (drawOnBack) return;
                            if (isYoyo)
                            {
                                //sam
                                data = modDraw_HandWeapon(data, drawPlayer, larger, lesser, isYoyo);
                            }
                            else
                            {
                                //spears are held facing to the floor, maces generally held
                                data = modDraw_PoleWeapon(data, drawPlayer, larger);
                            }
                        }
                        else
                        {
                            //nebula blaze, flairon, solar eruption (too inconsistent)
                            //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(uhh): " + itemWidth + " x " + itemHeight);
                            if (larger <= 48)
                            {
                                if (!drawOnBack) return;
                                data = modDraw_WaistWeapon(data, drawPlayer, larger);
                            }
                            else
                            {
                                if (!drawOnBack) return;
                                data = modDraw_BackWeapon(data, drawPlayer, larger);
                            }
                        }
                    }
                    else
                    {
                        if (drawPlayer.grapCount > 0) return; // can't see while grappling
                        //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "(staff): " + itemWidth + " x " + itemHeight);
                        if (drawOnBack) return;
                        //staves
                        data = modDraw_MagicWeapon(data, drawPlayer, larger);
                    }
                }
                //Add the weapon to the draw layers
                Main.playerDrawData.Add(data);
                drawGlowLayer(data, heldItem.glowMask, heldItem.alpha);
                //largestaves are held straight up
            }

            //if (drawPlayer.controlHook) Main.NewText(heldItem.useStyle + "[]: " + itemWidth + " x " + itemHeight, 100,200,150);
            
        }
        /*
        private static void drawShieldOver(PlayerDrawInfo drawInfo)
        {
            //go away if disappeared
            //get this
            PlayerFX p = drawInfo.drawPlayer.GetModPlayer<PlayerFX>
                (ModLoader.GetMod("WeaponOut"));
            if (p.shieldGraphicAlpha >= 255) return;
            //get shield accessory texture
            Texture2D shieldTex;
            if (p.shieldItem.type == 216)
            {
                shieldTex = Main.itemTexture[156]; //for some mysterious reason cobalt shield thinks its a shackle...
            }
            else
            {
                shieldTex = Main.itemTexture[p.shieldItem.type];
            }

            Vector2 drawPos = new Vector2(
            (int)(drawInfo.drawPlayer.MountedCenter.X - Main.screenPosition.X) + drawInfo.drawPlayer.direction * 16,
            (int)(drawInfo.drawPlayer.MountedCenter.Y - Main.screenPosition.Y + drawInfo.drawPlayer.gfxOffY) + drawInfo.drawPlayer.gravDir * 4);
            //get the lighting on the player's tile
            Color lighting = Lighting.GetColor(
                    (int)((drawInfo.position.X + drawInfo.drawPlayer.width / 2f) / 16f),
                    (int)((drawInfo.position.Y + drawInfo.drawPlayer.height / 2f) / 16f));
            float alphaMult = (float)(255 - p.shieldGraphicAlpha) / 255f;
            lighting.R = (byte)((float)lighting.R * alphaMult);
            lighting.G = (byte)((float)lighting.G * alphaMult);
            lighting.B = (byte)((float)lighting.B * alphaMult);
            lighting.A = (byte)(lighting.A - p.shieldGraphicAlpha);
            if (lighting.A < 0) lighting.A = 0;
            if (lighting.A > 255) lighting.A = 255;

            int amount = 2 * ((int)(shieldTex.Height * (p.shieldBlock / (float)p.shieldBlockMax)) / 2);
            Rectangle sourceRect = new Rectangle(
                0, 
                shieldTex.Height - amount, 
                shieldTex.Width,
                amount);
            Vector2 shieldCentre = new Vector2(shieldTex.Width / 2, shieldTex.Height / 2);


            DrawData data = new DrawData(
                    shieldTex,
                    drawPos + new Vector2(0, shieldTex.Height - amount),
                    sourceRect,
                    lighting,
                    0f,
                    shieldCentre,
                    Vector2.One,
                    drawInfo.drawPlayer.gravDir >= 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
                    0);
            //apply shield dye, because why not
            data.shader = drawInfo.shieldShader;
            Main.playerDrawData.Add(data);
        }
        */
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
        
        public static void modifyPlayerItemLocation(Player player, float X, float Y)
        {
            float cosRot = (float)Math.Cos(player.itemRotation);
            float sinRot = (float)Math.Sin(player.itemRotation);
            //Align
            player.itemLocation.X = player.itemLocation.X + (X * cosRot * player.direction) + (Y * sinRot * player.gravDir);
            player.itemLocation.Y = player.itemLocation.Y + (X * sinRot * player.direction) - (Y * cosRot * player.gravDir);
        }




        //=========================================!        !==================================================
        //=======================================!!          !!================================================
        //======================================! lots of code !===============================================
        //=======================================!!          !!================================================
        //=========================================!        !==================================================








        #region Weapon positions

        private static DrawData modDraw_WalkCycle(DrawData data, Player p)
        {
            if ((p.bodyFrame.Y / p.bodyFrame.Height >= 7 && p.bodyFrame.Y / p.bodyFrame.Height <= 9)
                || (p.bodyFrame.Y / p.bodyFrame.Height >= 14 && p.bodyFrame.Y / p.bodyFrame.Height <= 16))
            {
                data.position.Y -= 2 * p.gravDir;
            }
            switch (p.bodyFrame.Y / p.bodyFrame.Height)
            {
                case 7: data.position.X -= p.direction * 2; break;
                case 8: data.position.X -= p.direction * 2; break;
                case 9: data.position.X -= p.direction * 2; break;
                case 10: data.position.X -= p.direction * 2; break;
                case 14: data.position.X += p.direction * 2; break;
                case 15: data.position.X += p.direction * 4; break;
                case 16: data.position.X += p.direction * 4; break;
                case 17: data.position.X += p.direction * 2; break;
            }
            return data;
        }
        private static float rotationWalkCycle(int FrameNum)
        {
            //6 - 19
            //furthest left 9
            //furthest right 16
            float rot = 0;
            switch (FrameNum)
            {
                case 6: rot = -0.6f; break;
                case 7: rot = -0.8f; break;
                case 8: rot = -1; break;
                case 9: rot = -1; break;
                case 10: rot = -0.8f; break;
                case 11: rot = -0.4f; break;
                case 12: rot = 0; break;
                case 13: rot = 0.6f; break;
                case 14: rot = 0.8f; break;
                case 15: rot = 1; break;
                case 16: rot = 1; break;
                case 17: rot = 0.8f; break;
                case 18: rot = 0.4f; break;
                case 19: rot = 0; break;
            }

            return rot;
        }
        private static void drawGlowLayer(DrawData data, int glowMask, int alpha)
        {
            //items that GLOOOW
            if (glowMask != -1)
            {
                DrawData glowData = new DrawData(
                   Main.glowMaskTexture[glowMask],
                   data.position,
                   data.sourceRect,
                   new Microsoft.Xna.Framework.Color(250, 250, 250, alpha),
                   data.rotation,
                   data.origin,
                   data.scale,
                   data.effect,
                   0);

                Main.playerDrawData.Add(glowData);
            }
        }

        private static DrawData modDraw_HandWeapon(DrawData data, Player p, float length, float width)
        {
            return modDraw_HandWeapon(data, p, length, width, false);
        }
        private static DrawData modDraw_HandWeapon(DrawData data, Player p, float length, float width, bool isYoyo)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (isYoyo)
            {
                length /= 2;
                width /= 2;
            }
            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.5d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((4 - length * 0.1f) * p.direction, (width * 0.3f - 4 + 13) * p.gravDir); //back and down;
                if (isYoyo) data.position.X -= 8 * p.direction;
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.25d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((width * 0.5f - 8 - length / 2) * p.direction, -14 * p.gravDir); //back and down;
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.2d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((-5 + width * 0.5f) * p.direction, (width * 0.5f - 8 + 14 - length / 2) * p.gravDir);
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        private static DrawData modDraw_WaistWeapon(DrawData data, Player p, float length)
        {
            float maxFall = p.velocity.Y * p.gravDir;
            if (p.velocity.Y == 0) maxFall = p.velocity.X * p.direction;
            data.rotation = (float)(Math.PI * 1 + Math.PI * (0.1f + maxFall * 0.01f) * p.direction) * p.gravDir; //rotate just over 180 clockwise
            data.position.X -= (length * 0.5f - 20) * p.direction; //back
            data.position.Y += (16 - maxFall / 2) * p.gravDir; //down
            return data;
        }
        private static DrawData modDraw_BackWeapon(DrawData data, Player p, float length)
        {
            data.rotation = (float)((Math.PI * 1.1f + Math.PI * (length * -0.001f)) * p.direction) * p.gravDir; //rotate just over 180 clockwise
            data.position.X -= 8 * p.direction; //back
            data.position.Y -= (length * 0.2f - 16) * p.gravDir; //up

            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum == 7 || playerBodyFrameNum == 8 || playerBodyFrameNum == 9
            || playerBodyFrameNum == 14 || playerBodyFrameNum == 15 || playerBodyFrameNum == 16)
            {
                data.position.Y -= 2 * p.gravDir; //up
            }
            return data;
        }
        private static DrawData modDraw_PoleWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.4d - (length * 0.002d)) * p.direction * p.gravDir; //clockwise
                data.position.X += 8 * p.direction; //forward
                data.position.Y += (length * 0.1f + 14) * p.gravDir; //down
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * 0.1d + (length * 0.002d)) * p.direction * p.gravDir; //clockwise
                data.position.X += 6 * p.direction; //forward
                data.position.Y -= (10 + length * 0.1f) * p.gravDir; //up
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.3d) * p.direction * p.gravDir; //rotate 90 clockwise
                data.position.X += 10 * p.direction; //forward
                data.position.Y += (length * 0.1f + 6) * p.gravDir; //down
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        private static DrawData modDraw_DrillWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;

            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.1d * p.direction) * p.gravDir;
                data.position.X += (8 - (length * 0.1f)) * p.direction; //back
                data.position.Y += 14 * p.gravDir; //down
                
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.5d * p.direction) * p.gravDir;
                data.position.X -= 7 * p.direction; //back
                data.position.Y -= (24 - (length * 0.2f)) * p.gravDir; //down
                
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.05d * p.direction) * p.gravDir;
                data.position.X += (10 - (length * 0.1f)) * p.direction; //back
                data.position.Y += 10 * p.gravDir; //down
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        private static DrawData modDraw_ForwardHoldWeapon(DrawData data, Player p, float width)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;

            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.4d) * p.direction * p.gravDir;
                data.position += new Vector2(-6 * p.direction, (16 - width * 0.1f) * p.gravDir);
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.35d) * p.direction * p.gravDir;
                data.position += new Vector2(-8 * p.direction, (width * 0.1f - 12) * p.gravDir);
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.2d) * p.direction * p.gravDir;
                data.position += new Vector2(-2 * p.direction, (10 - width * 0.1f) * p.gravDir);
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        private static DrawData modDraw_AimedWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;

            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.5d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2(-2 * p.direction, (length * 0.5f) * p.gravDir); //back and down;
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.75d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((-10 - length * 0.2f) * p.direction, (10 - length / 2) * p.gravDir); //back and down;
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.1d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2(6 * p.direction, 8 * p.gravDir);
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        private static DrawData modDraw_HeavyWeapon(DrawData data, Player p, float width)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum < 5 || (playerBodyFrameNum == 10 && p.velocity.X == 0)) //standing
            {
                data.rotation = (float)(Math.PI * 0.005d) * width * p.direction * p.gravDir;
                data.position.X += 4 * p.direction; //forward
                data.position.Y += (width * 0.1f) + 6 * p.gravDir; //down
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.002d) * width * p.direction * p.gravDir;
                data.position.X += 2 * p.direction; //forward
                data.position.Y -= (width * 0.1f) + 10 * p.gravDir; //up
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.008d) * (width * 0.2f + rotationWalkCycle(playerBodyFrameNum) * 6) * p.direction * p.gravDir;
                data.position.X += 8 * p.direction; //forward
                data.position.Y += (width * 0.15f) * p.gravDir; //down
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        private static DrawData modDraw_MagicWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.2d) * p.direction * p.gravDir;
                data.position.X += (length * 0.1f + 4) * p.direction; //forward
                data.position.Y += (length * 0.1f + 6) * p.gravDir; //down
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.45d - (length * 0.002d)) * p.direction; //clockwise
                data.position.X -= (length * 0.1f + 16) * p.direction; //back
                data.position.Y -= (length * 0.16f + 14) * p.gravDir; //up
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * -0.2d - (length * 0.002d)) * p.direction; //anticlockwise
                data.position.X -= 2 * p.direction; //back
                data.position.Y -= (length * 0.4f - 12) * p.gravDir; //up
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }

        #endregion
    }
}
