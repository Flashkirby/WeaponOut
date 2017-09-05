using System;
using System.Collections.Generic;
using System.Reflection;

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

namespace WeaponOut
{
    public class PlayerFX : ModPlayer
    {

        private const bool DEBUG_WEAPONHOLD = false;
        private const bool DEBUG_BOOMERANGS = false;
        private static Mod itemCustomizer;

        public bool weaponVisual = true;

        private bool wasDead; //used to check if player just revived
        public Vector2 localTempSpawn;//spawn used by tent

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

        public bool reflectingProjectiles;
        public int reflectingProjectileDelay;
        public bool CanReflectProjectiles
        { get { return reflectingProjectiles && reflectingProjectileDelay <= 0; } }

        public bool lunarRangeVisual;
        public bool lunarMagicVisual;
        public bool lunarThrowVisual;

        #region Dual Weapon Values
        /// <summary> Multiplier for the item use animation </summary>
        public float dualItemAnimationMod;
        /// <summary> Multiplier for the item use time </summary>
        public float dualItemTimeMod;
        /// <summary> Multiplayer sync variable for figuring out if a weapon is being alt-func used. </summary>
        public bool dualItemCanUse;
        #endregion

        #region Armour Effects
        public bool taekwonCounter;
        public bool rapidRecovery;
        public float diveKickHeal;
        public bool millstone;
        public bool barbariousDefence;

        public float patienceDamage;
        private float patienceBonus;
        private const float patiencePerFrame = 0.0125f;
        private const float patienceCooldown = -patiencePerFrame * 120;

        public float yomiEndurance;
        public bool yomiFinishedAttack;

        public int sashLifeLost;
        public int sashLastLife;
        public float sashMaxLifeRecoverMult;
        public bool recordLifeLost;

        public bool buildMomentum;
        public const float momentumDashSpeed = 16f;
        protected float momentum;
        protected int momentumMax;
        public bool momentumActive;
        public bool momentumDash;
        public int momentumDashTime;

        public bool secondWind;
        public int secondWindLifeTax;

        private int yin;
        private int yang;
        public bool yinyang;

        #endregion

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
            itemCustomizer = ModLoader.GetMod("ItemCustomizer");

            lastSelectedItem = 0;

            itemSkillDelay = 0;

            localTempSpawn = new Vector2();

            // Update visuals
            WeaponOut.NetUpdateWeaponVisual(mod, this);
        }

        public override void Initialize()
        {
            patienceBonus = patienceCooldown;
        }
        public override void ResetEffects()
        {
            damageKnockbackThreshold = 0;
            frontDefence = 0;
            frontNoKnockback = false;

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

            if (ModConf.enableDualWeapons)
            {
                if (player.itemAnimation <= 1) dualItemAnimationMod = 1f;
                if (player.itemTime <= 1) dualItemTimeMod = 1f;
            }

            if (ModConf.enableFists)
            {
                taekwonCounter = false;
                rapidRecovery = false;
                diveKickHeal = 0f;
                millstone = false;
                momentumDash = false;
                momentumDashTime = Math.Max(0, momentumDashTime - 1);

                if (patienceBonus > 0) player.meleeDamage += patienceBonus;

                // 
                //  ================ Sash LIfe ================
                //
                if (!recordLifeLost)
                {
                    sashLastLife = player.statLife;
                    sashLifeLost = 0;
                }
                sashMaxLifeRecoverMult = 0;
                recordLifeLost = false;


                // 
                //  ================ Momentum ================
                //
                if (buildMomentum) //at least 15mph
                { momentum = Math.Max(0, momentum + Math.Min(9, Math.Abs(player.velocity.X)) - 3f); }
                else
                { momentum = 0; }

                int buffID = mod.BuffType<Buffs.Momentum>();
                if (momentum >= momentumMax && buildMomentum)
                {
                    momentum = momentumMax;
                    if (player.FindBuffIndex(buffID) < 0) player.AddBuff(buffID, 2, false);
                }
                else if (player.FindBuffIndex(buffID) >= 0) { player.AddBuff(buffID, 0, false); }
                momentumMax = 180;
                buildMomentum = false;
                momentumActive = false;
                
                // 
                //  ================ Second Wind ================
                //
                secondWind = false;
                if (player.FindBuffIndex(mod.BuffType<Buffs.SecondWind>()) < 0)
                { secondWindLifeTax = 0; }
                secondWindLifeTax = Math.Max(0, secondWindLifeTax);

                // 
                //  ================ Yinyang ================
                //
                yinyang = false;
            }
        }

        public override void UpdateDead()
        {
            sashLifeLost = 0;
            sashLastLife = 0;
            recordLifeLost = false;

            player.ClearBuff(mod.BuffType<Buffs.SecondWind>());
            secondWindLifeTax = 0;
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

        public override void PostUpdateBuffs()
        {
            if (ModConf.enableBasicContent)
            {
                applyBannerBuff();
            }
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

                foreach (Player otherPlayer in Main.player)
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

        public override bool PreItemCheck()
        {
            if (ModConf.enableDualWeapons)
            {
                PreItemCheckDualSync();
            }

            return true;
        }

        public override void PostItemCheck()
        {
            if( ModConf.enableDualWeapons)
            {
                PostItemDualSyncAltFunction();
            }
        }

        #region Dual Item code

        private void PreItemCheckDualSync()
        {
            if (player.itemAnimation == 1 &&
                Main.netMode == 1 &&
                Main.myPlayer != player.whoAmI)
            {
                // Reset item can use checker at end of swing
                //Main.NewText("reset pre altfunc = " + player.altFunctionUse);
                player.altFunctionUse = 0;
                dualItemCanUse = false;
                //Main.NewText("reset dualItemCanUse = " + dualItemCanUse);
            }
        }

        private void PostItemDualSyncAltFunction()
        {
            if (player.itemAnimation > 1)
            {
                // Force attempt to make foreign clients use altfunc, if swinging an item without
                // having called the CanUseItem function, so it runs the normally local only code
                if (!dualItemCanUse &&
                    player.altFunctionUse == 0 &&
                    Main.netMode == 1 &&
                    Main.myPlayer != player.whoAmI)
                {
                    // Only items from this mod with altfunction enabled
                    if (player.HeldItem.modItem != null &&
                        player.HeldItem.modItem.mod == mod &&
                        player.HeldItem.modItem.AltFunctionUse(player))
                    {
                        // I don't even know anymore
                        if (player.itemAnimation == player.itemAnimationMax - 1)
                        {
                            player.altFunctionUse = 0;
                        }
                        else
                        {
                            player.altFunctionUse = 2;
                        }
                        // Apply the right click effect and play sound
                        player.HeldItem.modItem.CanUseItem(player);
                        Main.PlaySound(player.HeldItem.UseSound, player.position);
                    }
                    dualItemCanUse = true;
                }
            }
        }

        /// <summary>
        /// Manages code related to right click multiplayer syncing, returning true if right click is being used. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="mainAnimMult">Primary useAnimation * modifier</param>
        /// <param name="mainTimeDiv">Primary useTime 1/modifier, also affects mainAnimMult</param>
        /// <param name="altAnimMult">Alternate useAnimation * modifier</param>
        /// <param name="altTimeDiv">Alternate useTime 1/modifier, also affects altAnimMult</param>
        /// <returns>True if alternate click function</returns>
        public static bool DualItemCanUseItemAlt(Player player, ModItem item, float mainAnimMult = 1f, float mainTimeDiv = 1f, float altAnimMult = 1f, float altTimeDiv = 1f)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();
            pfx.dualItemCanUse = true;
            if (player.altFunctionUse == 2)
            {
                pfx.dualItemAnimationMod = altAnimMult;
                pfx.dualItemTimeMod = altTimeDiv;
                player.itemTime = 0; // gotta reset anytime we mess with item time divider
                return true;
            }
            else
            {
                pfx.dualItemAnimationMod = mainAnimMult;
                pfx.dualItemTimeMod = mainTimeDiv;
                player.itemTime = 0; // gotta reset anytime we mess with item time divider
                return false;
            }
        }

        #endregion

        public override float MeleeSpeedMultiplier(Item item)
        {
            return dualItemAnimationMod;
        }
        public override float UseTimeMultiplier(Item item)
        {
            return dualItemTimeMod;
        }

        public override void PostUpdateRunSpeeds()
        {
            if(player.inventory[player.selectedItem].type == mod.ItemType<Items.Weapons.Basic.Raiden>())
            {
                if (itemSkillDelay >= Items.Weapons.Basic.Raiden.focusTime)
                {
                    float setSpeed = player.maxRunSpeed / 4f;
                    player.maxRunSpeed -= setSpeed;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }

            if (ModConf.enableFists)
            {
                if (momentumDashTime > 0)
                {
                    player.velocity = momentumDashSpeed * (Main.MouseWorld - player.Center).SafeNormalize(new Vector2(player.direction, 0));

                    if (player.velocity.X >= 0)
                    { player.direction = 1; }
                    else
                    { player.direction = -1; }
                    
                    for (int i = 0; i < 4; i++)
                    {
                        Dust d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 8f), player.width, 16, 31, player.velocity.X, player.velocity.Y, 50, default(Color), 1.6f)];
                        d.velocity *= 0.04f * i * momentumDashTime;
                        d.shader = GameShaders.Armor.GetSecondaryShader(player.cBody, player);
                    }

                    if (momentumDashTime == 1)
                    {
                        Main.PlaySound(SoundID.Run, player.position);
                        if (Main.myPlayer == player.whoAmI && Main.netMode == 1)
                        {
                            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                }

                // Done here because last playerhook before collision checks
                if (barbariousDefence)
                {
                    player.statDefense += (player.statLifeMax2 - player.statLife) / 5;
                }
                barbariousDefence = false;
            }
        }

        public override void PostUpdate()
        {
            manageBodyFrame();
            tentScript();
            fistPostUpdate();
            sashRestoreLogic();
        }

        private void sashRestoreLogic()
        {
            if (!ModConf.enableFists || 
                sashMaxLifeRecoverMult <= 0f || 
                player.whoAmI != Main.myPlayer ) return;

            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.ComboCounter > 0)
            {
                recordLifeLost = true;

                // Lost health? Records lost amount
                if (player.statLife < sashLastLife)
                { sashLifeLost += sashLastLife - player.statLife; }
                sashLastLife = player.statLife;

                sashLifeLost = lifeRestorable(player);

                if (sashLifeLost == 0)
                { player.AddBuff(mod.BuffType<Buffs.FightingSpiritEmpty>(), 2); }
                else if (sashLifeLost >= (int)(player.statLifeMax2 * sashMaxLifeRecoverMult))
                {
                    if (player.FindBuffIndex(mod.BuffType<Buffs.FightingSpiritMax>()) < 0)
                    {
                        Main.PlaySound(SoundID.Tink, player.position);
                    }
                    player.AddBuff(mod.BuffType<Buffs.FightingSpiritMax>(), 2);
                }
                else
                { player.AddBuff(mod.BuffType<Buffs.FightingSpirit>(), 2); }
            }
            else if (!player.moonLeech && sashLifeLost > 0)
            {
                Main.PlaySound(2, -1, -1, 4, 0.3f, 0.2f); // mini heal effect
                player.HealEffect(sashLifeLost, false);
                player.statLife += sashLifeLost;
                player.statLife = Math.Min(player.statLife, player.statLifeMax2);
                if(Main.netMode == 1 && Main.myPlayer == player.whoAmI) NetMessage.SendData(MessageID.PlayerHealth, -1, -1, null, player.whoAmI);
            }
        }
        private int lifeRestorable(Player player)
        { return Math.Min((int)(player.statLifeMax2 * sashMaxLifeRecoverMult), sashLifeLost); }

        public override void SetControls()
        {
            for (int i = 0; i < 4; i++)
            {
                bool JustPressed = false;
                switch (i)
                {
                    case 0:
                        JustPressed = (player.controlDown && player.releaseDown);
                        break;
                    case 1:
                        JustPressed = (player.controlUp && player.releaseUp);
                        break;
                    case 2:
                        JustPressed = (player.controlRight && player.releaseRight);
                        break;
                    case 3:
                        JustPressed = (player.controlLeft && player.releaseLeft);
                        break;
                }
                if (JustPressed && player.doubleTapCardinalTimer[i] > 0 && JustPressed && player.doubleTapCardinalTimer[i] < 15)
                {
                    KeyDoubleTap(i);
                }
            }
        }
        
        public void KeyDoubleTap(int keyDir)
        {
            int inputKey = 0;
            if (Main.ReversedUpDownArmorSetBonuses)
            {
                inputKey = 1;
            }
            if (keyDir == inputKey)
            {
                if (ModConf.enableFists)
                {
                    MomentumDashTorwardsMouse();
                }
            }
        }

        private void MomentumDashTorwardsMouse()
        {
            if (!momentumDash || momentum < momentumMax) return;
            momentum = 0;
            momentumDashTime = 6;

            player.AddBuff(mod.BuffType<Buffs.DamageUp>(), 90);
            player.immune = true;
            player.immuneTime = Math.Max(10, player.immuneTime);

            Main.PlaySound(SoundID.Run, player.position);
            for (int i = 0; i < 3; i++)
            {
                Gore g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 14f), default(Vector2), Main.rand.Next(61, 64), 1f)];
            }

            if (Main.myPlayer == player.whoAmI && Main.netMode == 1)
            {
                NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        #region Tent
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
                if (Main.itemAnimations[heldItem.type] != null)
                {
                    itemHeight /= Main.itemAnimations[heldItem.type].FrameCount;
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
        #endregion

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

            //fistPostUpdate();
        }
        #endregion
        #region draw
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
                if (Main.itemAnimations[heldItem.type] != null) // in the case of modded weapons with animations...
                {
                    //get local player frame counting
                    PlayerFX p = drawPlayer.GetModPlayer<PlayerFX>(ModLoader.GetMod("WeaponOut"));
                    int frameCount = Main.itemAnimations[heldItem.type].FrameCount;
                    int frameCounter = Main.itemAnimations[heldItem.type].TicksPerFrame * 2;

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
            if (itemCustomizer != null)
            {
                data.shader = ItemCustomizerGetShader(itemCustomizer, heldItem);
            }

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
                //They are worn on the waist, and react to falling! Except when disabled
                //This also amusingly applies to ducks, axes and rockfish
                //But shouldn't apply to pickaxes, except when they are also not pickaxes
                else if (larger <= 48 && 
                    (heldItem.pick <= 0 || 
                    (heldItem.pick > 0 && heldItem.axe > 0)))
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

        private int patienceDustUpdate = 0;
        private void fistPostUpdate()
        {
            if (ModConf.enableFists)
            {
                if (yomiEndurance > 0f)
                {
                    if (player.itemAnimation == 0)
                    {
                        if (yomiFinishedAttack)
                        {
                            // Buff manages setting/resetting
                            player.AddBuff(mod.BuffType<Buffs.YomiEndure>(), 60 * 3);
                            yomiFinishedAttack = false;
                        }
                    }
                    else
                    {
                        yomiFinishedAttack = true;
                    }
                }

                // ONLY DO THIS CLIENT SIDE
                if (Main.myPlayer == player.whoAmI)
                {
                    bool nearBoss = false;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && npc.life > 0 && npc.boss)
                        {
                            nearBoss = true;
                            break;
                        }
                    }
                    if (nearBoss)
                    {
                        if (player.itemAnimation == 0)
                        {
                            if (player.HeldItem.melee)
                            {
                                patienceBonus += patiencePerFrame;
                            }
                        }
                        // using ranged/magic will reduce effect drastically
                        else if (player.HeldItem.ranged || player.HeldItem.magic)
                        {
                            patienceBonus -= patiencePerFrame * 4f;
                        }
                        patienceBonus = Math.Min(patienceDamage, patienceBonus);
                        patienceDustUpdate = Math.Max(0, patienceDustUpdate + (int)(patienceBonus * 100f));

                        while (patienceDustUpdate > 1500)// 1 dust per 100% damage every 15 frames
                        {
                            patienceDustUpdate -= 1500;
                            float yVel = player.velocity.Y - player.gravDir * 0.2f * patienceBonus;
                            Dust d = Dust.NewDustPerfect(new Vector2(
                                player.position.X + player.width * Main.rand.NextFloat(0f, 1f),
                                player.Center.Y + player.height * player.gravDir / 2 - yVel),
                                254, new Vector2(player.velocity.X, yVel),
                                0, default(Color), 0.5f);
                            d.noGravity = true;
                        }
                    }
                    else
                    {
                        patienceBonus = patienceCooldown;
                        patienceDustUpdate = 0;
                    }
                    patienceDamage = 0f;
                }

                if (momentum >= momentumMax)
                {
                    player.armorEffectDrawOutlinesForbidden = true;
                }

                if (weaponVisual)
                {
                    if (player.HeldItem.useStyle == ModPlayerFists.useStyle)
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
        }

        private static int ItemCustomizerGetShader(Mod mod, Item item)
        {
            if (!Main.dedServ)
            {
                try
                {
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

        #region OnHit Methods

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            FistOnHitNPC(target, damage);
        }

        private void FistOnHitNPC(NPC target, int damage)
        {
            if (ModConf.enableFists)
            {
                if (target.immortal) return;
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if (mpf.specialMove == 2 && diveKickHeal > 0f)
                {
                    int heal = (int)(damage * diveKickHeal);
                    player.HealEffect(heal, false);
                    player.statLife += heal;
                    player.statLife = Math.Min(player.statLife, player.statLifeMax2);

                    if (Main.netMode == 1 && Main.myPlayer == player.whoAmI) NetMessage.SendData(MessageID.PlayerHealth, -1, -1, null, player.whoAmI);
                }

                // Punches heal
                if (secondWind) secondWindLifeTax -= Math.Min(5, mpf.ComboCounter / 4);
            }
        }

        #endregion

        #region Hurt & Hit Methods

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        { ModifyHit(item, player, target.life, target.lifeMax, ref damage, ref knockback, ref crit); }
        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(item, player, target.statLife, target.statLifeMax2, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Item item, Player player, int life, int lifeMax, ref int damage, ref float knockBack, ref bool crit)
        {
            if (ModConf.enableFists)
            {
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if (millstone && mpf.IsComboActive)
                {
                    damage += (int)(life * 0.002f);
                }

                patienceBonus = patienceCooldown;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            ModifyHitWithProj(proj, Main.player[proj.owner], target.life, target.lifeMax, ref damage, ref crit);
        }
        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            ModifyHitWithProj(proj, Main.player[proj.owner], target.statLife, target.statLifeMax2, ref damage, ref crit);
        }
        private void ModifyHitWithProj(Projectile proj, Player player, int life, int lifeMax, ref int damage, ref bool crit)
        {
            if (ModConf.enableFists)
            {
                if(proj.melee) patienceBonus = patienceCooldown;
            }
        }




        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            ShieldPreHurt(damage, crit, hitDirection);
            if (!FistPreHurt(damageSource)) return false;
            return true;
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
                if(!Main.expertMode)
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
                    NetMessage.SendData(28, -1, -1, null, npc.whoAmI, (float)hitDamage, (float)knockBack, (float)hitDirection, 0, 0, 0);
                }
            }
        }


        private bool FistPreHurt(PlayerDeathReason damageSource)
        {
            if (ModConf.enableFists)
            {
                if (momentumActive)
                {
                    if (damageSource.SourceProjectileIndex >= 0)
                    {
                        momentum /= 2;
                        ProjFX.ReflectProjectilePlayer(Main.projectile[damageSource.SourceProjectileIndex], player);
                        Main.PlaySound(SoundID.Item10, player.position);
                        player.AddBuff(mod.BuffType<Buffs.Momentum>(), 0, false);
                        return false;
                    }
                    else if (damageSource.SourceNPCIndex >= 0)
                    {
                        momentum = 0;
                        player.ApplyDamageToNPC(Main.npc[damageSource.SourceNPCIndex], player.statLife / 4, 3f + Math.Abs(player.velocity.X) * 2f, player.direction, false);
                        Main.PlaySound(SoundID.Item10, player.position);
                        player.AddBuff(mod.BuffType<Buffs.Momentum>(), 0, false);
                        player.immuneTime += 60;
                        return false;
                    }
                }
            }
            return true;
        }

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            FistPostHurt(damage);
        }

        private void FistPostHurt(double damage)
        {
            if (ModConf.enableFists)
            {
                if (taekwonCounter)
                {
                    player.AddBuff(mod.BuffType<Buffs.DamageUp>(), 60 + Math.Abs(player.statLife - player.statLifeMax2) / 2);
                }
                if (rapidRecovery)
                {
                    Buffs.RapidRecovery.HealDamage(player, mod, damage);
                }
            }
        }

        #endregion

        #region Pre-Dead

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (ModConf.enableFists)
            {
                if (secondWind && player.statLife <= 0)
                {
                    int overkill = 1;
                    if (damageSource.SourceOtherIndex >= 0)
                    { overkill = 1 - player.statLife; }
                    else
                    { overkill = (int)damage + 1 - player.statLife; }

                    if (player.statLifeMax2 - overkill > 20) // would still have 1 heart left?
                    {
                        if (secondWindLifeTax == 0)
                        {
                            player.AddBuff(mod.BuffType<Buffs.SecondWind>(), 3600 * 3); // 3 min
                        }
                        secondWindLifeTax += overkill;
                        player.statLife = Math.Max(player.statLife, 1);
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        // TODO: rebalance crate IDs
        public override void CatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
        {
            if (junk) return; // Don't do stuff if the catch is a junk catch
            bool common, uncommon, rare, veryrare, superrare, isCrate;
            calculateCatchRates(power, out common, out uncommon, out rare, out veryrare, out superrare, out isCrate);

            if (liquidType == 0) //Water
            {
                if (isCrate) // Crate catches
                { return; }

                // Catch anywhere
                if (superrare)
                {
                    if (superrare && Main.rand.Next(3) == 0)
                    { caughtType = mod.ItemType<Items.RustedBadge>(); return; }
                }

                if (worldLayer <= 1) //Surface or below
                {
                    if (player.ZoneBeach && poolSize > 1000) // Ocean
                    {   // If fancier items would be caught, they would replace lower tiers anyway.
                        if (superrare)
                        { return; }
                        if (veryrare && Main.rand.Next(2) == 0)
                        { return; }
                        if (rare && Main.rand.Next(2) == 0) // Same chance as swordfish
                        { caughtType = mod.ItemType<Items.Weapons.Whips.EelWhip>(); return; }
                        if (uncommon)
                        { return; }
                        if(common)
                        { return; }
                    }
                }
            }
            if (liquidType == 1 && ItemID.Sets.CanFishInLava[fishingRod.type])
            {
                if(isCrate) // Crate Catches
                { return; }
            }
        }

        /// <summary>
        /// Calculate the base catch rates for different tiers of fish. Parameter chances are shown at 50% fishing power. Examples of fish at each tier, plus individual catch rates:
        /// <para> Common: Neon Tetra, Crimson Tigerfish, Atlantic Cod, Red Snapper (1/2)</para>
        /// <para> Uncommon: Damselfish, Frost Minnow, Ebonkoi</para>
        /// <para> Rare: Honeyfin, Prismite, Purple Clubberfish</para>
        /// <para> Very Rare: Sawtooth Shark, Flarefin Koi, Golden Crate</para>
        /// <para> Extremely Rare: Obsidian Swordfish, Toxikarp (1/2),  Bladetongue (1/2), Balloon Pufferfish (1/5), Zephyr Fish (1/10)</para>
        /// If all else fails, Terraria rewards the player with a Bass (or Trout in the ocean).
        /// </summary>/
        /// <param name="power">The fishing skill. </param>
        /// <param name="common">33.3% = power:150 (capped 1:2). /</param>
        /// <param name="uncommon">16.7% = power:300 (capped 1:3). </param>
        /// <param name="rare">4.8% = power:1050 (capped 1:4). </param>
        /// <param name="veryrare">2.2% = power:2250 (capped 1:5). </param>
        /// <param name="superrare">1.1% = power:4500 (capped 1:6). </param>
        /// <param name="isCrate">1:10, 1:5 with crate potion. </param>
        public void calculateCatchRates(int power, out bool common, out bool uncommon, out bool rare, out bool veryrare, out bool superrare, out bool isCrate)
        {
            common = false;
            uncommon = false;
            rare = false;
            veryrare = false;
            superrare = false;
            isCrate = false;

            if (power <= 0) return;

            if (Main.rand.Next(Math.Max(2, 150 * 1 / power)) == 0)
            { common = true; }
            if (Main.rand.Next(Math.Max(3, 150 * 2 / power)) == 0)
            { uncommon = true; }
            if (Main.rand.Next(Math.Max(4, 150 * 7 / power)) == 0)
            { rare = true; }
            if (Main.rand.Next(Math.Max(5, 150 * 15 / power)) == 0)
            { veryrare = true; }
            if (Main.rand.Next(Math.Max(6, 150 * 30 / power)) == 0)
            { superrare = true; }
            if (Main.rand.Next(100) < (10 + (player.cratePotion ? 10 : 0)))
            { isCrate = true; }
        }
    }
}