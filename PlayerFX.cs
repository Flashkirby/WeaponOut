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
        
        public Vector2 localTempSpawn;//spawn used by tent

        #region Knockback Threshold
        private int damageKnockbackThreshold;
        public int DamageKnockbackThreshold
        {
            get { return damageKnockbackThreshold; }
            set
            {
                if (value > damageKnockbackThreshold) damageKnockbackThreshold = value;
            }
        }
        #endregion
        #region Front Defence
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
        #endregion
        #region Reflect Projectiles
        public bool reflectingProjectiles;
        public bool reflectingProjectilesForce;
        public bool reflectingProjectilesParryStyle;
        public int reflectingProjectileDelay;
        public bool CanReflectProjectiles
        { get { return reflectingProjectilesForce || (reflectingProjectiles && reflectingProjectileDelay <= 0); } }
        #endregion
        #region Accessories
        public bool lunarRangeVisual;
        public bool lunarMagicVisual;
        public bool lunarThrowVisual;

        public bool criticalHealStar;
        #endregion
        #region Dual Weapon
        /// <summary> Multiplier for the item use animation </summary>
        public float dualItemAnimationMod;
        /// <summary> Multiplier for the item use time </summary>
        public float dualItemTimeMod;
        /// <summary> Multiplayer sync variable for figuring out if a weapon is being alt-func used. </summary>
        public bool dualItemCanUse;
        #endregion
        #region Fist Armour/Accessory Effects
        public bool taekwonCounter;
        public bool doubleDamageUp;
        public bool rapidRecovery;
        public float diveKickHeal;
        public bool millstone;
        public int barbariousDefence;
        public bool heartDropper;
        public bool heartBuff;
        public bool hidden;
        public bool angryCombo;
        public bool debuffRecover;
        public bool beeHealing;
        public bool starlightGuardian;
        public bool starlightGuardianStanceChangeInput;

        public float patienceDamage;
        public float patienceBuildUpModifier;
        private float patienceBonus;
        private const float patiencePerFrame = 0.75f / 60; // 75% per second
        private const float patienceCooldown = -patiencePerFrame * 90; // 1.5 second cooldown
        public float PatienceBonus
        { get { if (patienceBonus < 0f) { return 0f; } return patienceBonus; } }

        public float yomiEndurance;
        public bool yomiFinishedAttack;

        public int sashLifeLost;
        public int sashLastLife;
        public float sashMaxLifeRecoverMult;
        public bool recordLifeLost;

        public bool buildMomentum;
        public const float momentumDashSpeed = 17f;
        protected float momentum;
        protected int momentumMax;
        public bool momentumActive;
        public bool momentumDash;
        public int momentumDashTime;

        public bool secondWind;
        public int secondWindLifeTax;

        public const float yinyangDistance = 40f;
        public const float yinyangBalanceThreshold = 0.25f;
        public bool yinyang;
        private int yin;
        private int yang;
        public float yinMeleeBonus;
        public float GetYinYangBalance()
        {
            if (yang + yin == 0) return 0f;
            return (yang - yin) / (1f * (yang + yin));
        }
        public float GetYinYangNormal(bool isYin)
        {
            float total = yang;
            if (isYin) total = yin;
            if (total == 0) return 0f;
            return total / (yang + yin);
        }

        public bool demonBlood;
        public float demonBloodHealMod;
        public int demonBloodRally;
        public int demonBloodRallyCurrentLife; // keep track of healing
        public int demonBloodRallyDelay;
        public int demonBloodCooldown; // can't spam attacks on say, destroyer
        private const int demonBloodReallyDelayMax = 60 * 4;

        public bool summonSoap;
        public int soapCooldown;
        public const int soapCooldownMax = 90;
        private const int maxSoapBubbles = 3;

        #endregion

        public const int omHaMoShindearuTimerMax = 360;
        public const int omHaMoShindearuTimerTick = 120;
        public const int omHaMoShindearuMinDamage = 5000 / 2;
        public const int omHaMoShindearuMaxDamage = 15000 / 2;
        public NPC omHaMoShin;
        public int omHaMoShindearuTimer;

        #region Player False Position
        public bool ghostPosition;
        public Vector2 FakePositionReal;
        public Vector2 FakePositionTemp;
        private const float FakePositionLag = 120;
        private const float FakePositionDistance = 1000f;
        #endregion

        public int lastSelectedItem;
        public int itemSkillDelay;
        public int lastNPCHitStarlightGuardian = -1;

        #region Utils
        public static void drawMagicCast(Player player, SpriteBatch spriteBatch, Color colour, int frame = -1)
        {
            if (frame < 0) frame = (int)Main.time % 48 / 12;

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
            if (player.whoAmI == Main.myPlayer)
            { Main.PlaySound(25, -1, -1, 1); }
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

            localTempSpawn = new Vector2();

            // Set up position
            if (ModConf.EnableFists)
            {
                FakePositionReal = player.position;
                FakePositionTemp = player.position;
            }
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

            criticalHealStar = false;

            // Handle reflecting timer
            reflectingProjectiles = false;
            reflectingProjectilesForce = false;
            reflectingProjectilesParryStyle = false;
            if (reflectingProjectileDelay > 0) reflectingProjectileDelay = Math.Max(0, reflectingProjectileDelay - 1);

            if (ModConf.EnableDualWeapons)
            {
                if (player.itemAnimation <= 1) dualItemAnimationMod = 1f;
                if (player.itemTime <= 1) dualItemTimeMod = 1f;
            }

            if (ModConf.EnableFists)
            {
                taekwonCounter = false;
                doubleDamageUp = false;
                rapidRecovery = false;
                diveKickHeal = 0f;
                millstone = false;
                momentumDash = false;
                momentumDashTime = Math.Max(0, momentumDashTime - 1);
                heartDropper = false;
                heartBuff = false;
                hidden = false;
                angryCombo = false;
                ghostPosition = false;
                debuffRecover = false;
                beeHealing = false;
                starlightGuardian = false;

                patienceBuildUpModifier = 1f;

                // 
                //  ================ Sash Life ================
                //
                if (!recordLifeLost)
                {
                    sashLastLife = player.statLife;
                    sashLifeLost = 0;
                }
                sashMaxLifeRecoverMult = 0;
                recordLifeLost = false;

                if (demonBloodHealMod > 0f)
                {
                    if (demonBloodRallyDelay <= 0 && demonBloodRally > 0)
                    {
                        demonBloodRallyDelay = 0;
                        demonBloodRally = Math.Max(0, demonBloodRally -
                            Math.Max(1, player.statLifeMax2 / 120));
                    }
                    else
                    {
                        demonBloodRallyDelay--;
                    }
                }
                else
                {
                    demonBloodRally = 0;
                    demonBloodRallyDelay = 0;
                }
                demonBloodHealMod = 0;
                if (demonBloodCooldown > 0) demonBloodCooldown--;
                if (demonBlood && Main.expertMode) demonBloodHealMod = 4f;

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
                if (!yinyang)
                {
                    yin = 0;
                    yang = 0;
                    yinMeleeBonus = 0f;
                }
                else
                {
                    float loss = 0.02f;
                    if (yinMeleeBonus > 1f) loss += 0.01f;
                    if (yinMeleeBonus > 2f) loss += 0.02f;
                    if (yinMeleeBonus > 3f) loss += 0.05f;
                    yinMeleeBonus = Math.Max(0f, yinMeleeBonus - loss / 60); // Lose 2% damage per second

                    if (yang == 0)
                    {
                        if (yin > 0) yin = Math.Max(0, yin - 4); // Lose 20 Damage (from yin) per second
                    }
                }
                yinyang = false;

                if (summonSoap && Main.netMode != 1)
                {
                    if (soapCooldown < soapCooldownMax)
                    { soapCooldown++; }
                    else
                    {
                        int bubbleID = mod.NPCType<NPCs.ComboBubble>();
                        if (NPC.CountNPCS(bubbleID) < maxSoapBubbles)
                        {
                            Point spawnPos = player.Center.ToPoint();
                            spawnPos.X += Main.rand.Next(-100, 101);
                            spawnPos.Y += Main.rand.Next(-100, 101);
                            NPC.NewNPC(spawnPos.X, spawnPos.Y, mod.NPCType<NPCs.ComboBubble>(),
                                0, 0f, 0f, 0f, 0f, player.whoAmI);

                            soapCooldown = 0;
                        }
                    }
                }
                else
                { soapCooldown = 0; }
                summonSoap = false;
            }
        }
        public override void UpdateDead()
        {
            sashLifeLost = 0;
            sashLastLife = 0;
            recordLifeLost = false;

            yin = 0;
            yang = 0;
            yinMeleeBonus = 0f;

            player.ClearBuff(mod.BuffType<Buffs.SecondWind>());
            secondWindLifeTax = 0;

            FakePositionTemp = default(Vector2);
            FakePositionReal = default(Vector2);

            NPCIsAlreadyDead();
        }

        #region Save and Load
        public override TagCompound Save()
        {
            return new TagCompound
            {
                { "demonBlood", demonBlood }
            };
        }
        public override void Load(TagCompound tag)
        {
            demonBlood = tag.GetBool("demonBlood");
        }
        #endregion

        public override void OnRespawn(Player player)
        {
            tentScript();
            demonBloodRally = 0;
        }

        public override void PostUpdateBuffs()
        {
            if (ModConf.EnableBasicContent)
            {
                applyBannerBuff();
            }
            if (ModConf.EnableFists)
            {
                if (!starlightGuardian)
                {
                    // Delete starlight guardian debuff if it hasn't been applied
                    int buffID = mod.BuffType<Buffs.SpiritGuardian>();
                    if (player.FindBuffIndex(buffID) != -1)
                    { player.DelBuff(player.FindBuffIndex(buffID)); }

                }
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
            if (ModConf.EnableDualWeapons)
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
                        player.HeldItem.modItem.mod == WeaponOut.mod &&
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
            if (ModConf.EnableDualWeapons) return dualItemAnimationMod;
            return base.MeleeSpeedMultiplier(item);
        }
        public override float UseTimeMultiplier(Item item)
        {
            if (ModConf.EnableDualWeapons) return dualItemTimeMod;
            return base.UseTimeMultiplier(item);
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

            if (ModConf.EnableFists)
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
                        player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
                    }
                }

                // Done here because last playerhook before collision checks
                if (barbariousDefence > 0)
                {
                    player.statDefense += (player.statLifeMax2 - player.statLife) / barbariousDefence;
                }
                barbariousDefence = 0;
            }

            if (ModConf.EnableAccessories)
            {
                hookPressed = player.controlHook && player.releaseHook;
            }
        }

        public override void PostUpdate()
        {
            discordItemsCheck();
            FistPostUpdate();
            sashRestoreLogic();
            NPCIsAlreadyDead();
        }

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
                if (ModConf.EnableFists)
                {
                    MomentumDashTorwardsMouse();
                    starlightGuardianStanceChangeInput = true;
                }
            }
        }

        private void NPCIsAlreadyDead() {
            if (omHaMoShin == null) { return; }
            if (!omHaMoShin.active || omHaMoShin.life <= 0) { omHaMoShin = null; return; }
            
            if (omHaMoShindearuTimer == 36 || omHaMoShindearuTimer == 30) {
                Main.PlaySound(SoundID.Item30, omHaMoShin.position);
            }
            else if (omHaMoShindearuTimer == 0) {
                Main.PlaySound(SoundID.Item45, omHaMoShin.position);
            }

            if (omHaMoShindearuTimer > 0) {
                int seconds = omHaMoShindearuTimer / omHaMoShindearuTimerTick;
                if (omHaMoShindearuTimer % omHaMoShindearuTimerTick == 0) {
                    Main.PlaySound(SoundID.MenuTick, omHaMoShin.position);
                    CombatText.NewText(omHaMoShin.Hitbox, Color.Red,
                        seconds + ":00", true);
                }
                else {
                    int milliseconds = omHaMoShindearuTimer - seconds * omHaMoShindearuTimerTick;
                    if (milliseconds % 9 == 0) {
                        milliseconds = (milliseconds * 100) / omHaMoShindearuTimerTick;

                        CombatText.NewText(omHaMoShin.Hitbox, Color.Red,
                            (seconds) + ":" + milliseconds, false, true);
                    }
                }
                omHaMoShindearuTimer--;

            }
            else {
                
                int damage = omHaMoShindearuMinDamage + Math.Min(omHaMoShindearuMaxDamage, omHaMoShin.lifeMax / 2);
                float knockback = 10;
                int direction = omHaMoShin.direction;
                bool crit = true;
                omHaMoShin.StrikeNPC(damage, knockback, direction, crit, false, false);
                if (Main.netMode != 0) {
                    NetMessage.SendData(28, -1, -1, null, omHaMoShin.whoAmI, (float)damage, knockback, (float)direction, crit.ToInt(), 0, 0);
                }
                omHaMoShin = null;
            }
        }

        #region OnHit Methods

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            FistOnHitNPC(target, damage);
            lastNPCHitStarlightGuardian = target.whoAmI;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            FistOnHitNPCWithProj(proj, target, damage);
            if(!proj.minion) lastNPCHitStarlightGuardian = target.whoAmI;
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            FistOnHitByEntity(npc, damage);
            CriticalHeartHit(damage);
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            FistOnHitByEntity(proj, damage);
            CriticalHeartHit(damage);
        }

        #endregion

        #region ModifyHit

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        { FistModifyHit(item, player, target.life, target.lifeMax, ref damage, ref knockback, ref crit); }
        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; FistModifyHit(item, player, target.statLife, target.statLifeMax2, ref damage, ref knockBack, ref crit); }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            FistModifyHitWithProj(proj, Main.player[proj.owner], target.life, target.lifeMax, ref damage, ref crit);
        }
        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            FistModifyHitWithProj(proj, Main.player[proj.owner], target.statLife, target.statLifeMax2, ref damage, ref crit);
        }

        #endregion

        #region PreHurt & ModifyHitBy

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            ShieldPreHurt(damage, crit, hitDirection);
            if (!FistPreHurt(damage, damageSource)) return false;
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
                if (!Main.expertMode)
                { if (damage <= DamageKnockbackThreshold) player.noKnockback = true; }
                else
                { if (damage <= DamageKnockbackThreshold * Main.expertDamage) player.noKnockback = true; }

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

        #endregion

        #region Post Hurt

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            FistPostHurt(damage);
        }

        #endregion

        #region Pre-Dead

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            return FistPreKill(damage, damageSource);
        }

        #endregion

        #region Tent
        private void tentScript()
        {
            if (localTempSpawn != default(Vector2)) checkTemporarySpawn();
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

                if (Main.netMode == 1)
                {
                    NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }
        }
        #endregion
        
        #region draw

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (hidden)
            {
                foreach (PlayerLayer l in layers)
                {
                    l.visible = false;
                }
                return;
            }
        }
        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
        {
            if (ModConf.EnableBasicContent)
            {
                // Offset helmet by 2 pixels upwards to move it to the right position
                if (player.head == mod.GetEquipSlot("ColosseumHelmet", EquipType.Head))
                { player.headPosition += new Vector2(0, -2); }
            }
        }
        #endregion

        #region Fist Effects
        private int patienceDustUpdate = 0;
        private void FistPostUpdate()
        {
            if (ModConf.EnableFists)
            {
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();

                #region Yomu Buff Apply
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
                #endregion

                #region Yin Yang - Main 
                if (yinyang && Main.myPlayer == player.whoAmI && (yang > 0 || yin > 0))
                {
                    float balance = GetYinYangBalance();
                    bool isBalanced = balance <= yinyangBalanceThreshold && balance >= -yinyangBalanceThreshold;

                    #region Dust Effect
                    if (!player.dead)
                    {
                        Vector2 center = player.Center + new Vector2(0, player.gfxOffY) - player.velocity;

                        float size = 0f;
                        float angle = (float)(Main.time * 0.02f);
                        int shader = 97;
                        if (isBalanced) angle *= 4f;
                        Color colour = default(Color);

                        if (Main.time % 2 == 0)
                        {
                            size = 1f + balance;
                            colour = Color.White;
                            shader = 109;
                        }
                        else
                        {
                            size = 1f - balance;
                            angle += (float)Math.PI;
                            colour = Color.DarkGray;
                        }
                        size = Math.Max(0f, Math.Min(1f, size));
                        size *= Math.Max(1f, Math.Min(4f, 1f + (yang + yin) / 1000f));

                        Dust d = Dust.NewDustPerfect(new Vector2(
                            center.X + yinyangDistance * (float)Math.Sin(angle),
                            center.Y + yinyangDistance * (float)Math.Cos(angle)),
                            91, new Vector2(0, 0),
                            (int)(45 * size), colour, size);
                        d.noLight = true;
                        d.noGravity = true;
                        d.shader = GameShaders.Armor.GetSecondaryShader(shader, player);
                        if (isBalanced)
                        { d.rotation = -angle; } // Rotate in sync
                        else
                        { d.rotation = angle; } // Rotate contray to motion
                        d.customData = player;
                        Main.playerDrawDust.Add(d.dustIndex);
                    }
                    #endregion

                    if (mpf.ComboCounter == 0 && mpf.OldComboCounter != 0)
                    {
                        #region Actual Effect on Combo End
                        float yangPower = CalculateYangPower(balance);
                        float yinPower = CalculateYinPower(balance);

                        // Main.NewText(String.Concat("Balance is ", yang, " < ", balance, " > ", yin));
                        if (yangPower > 0f)
                        {
                            int healing = (int)((player.statLifeMax2 - player.statLife) * yangPower);
                            if (healing > 0)
                            {
                                PlayerFX.HealPlayer(player, healing);
                                if (!isBalanced)
                                {
                                    player.lifeSteal -= healing / 2; // Reduce effectiveness of lifesteal right after
                                }
                            }
                        }
                        if (yinPower > 0f)
                        {
                            if (yangPower > 0)
                            {
                                yinMeleeBonus += yinPower;
                            }
                            else
                            {
                                yinMeleeBonus = Math.Max(yinMeleeBonus, yinPower);
                            }

                            player.AddBuff(mod.BuffType<Buffs.YinDamage>(), 2);
                        }

                        yin = 0;
                        yang = 0;

                        player.ClearBuff(mod.BuffType<Buffs.YinYang>());
                        player.ClearBuff(mod.BuffType<Buffs.Yang>());
                        player.ClearBuff(mod.BuffType<Buffs.Yin>());
                        #endregion
                    }
                    else
                    {
                        #region Apply YinYang Buff
                        int bYiYa = mod.BuffType<Buffs.YinYang>();
                        int bYang = mod.BuffType<Buffs.Yang>();
                        int bYin = mod.BuffType<Buffs.Yin>();
                        int time = mpf.comboTimerMax - mpf.comboTimer;

                        if (isBalanced)
                        {
                            player.AddBuff(bYiYa, time);
                            player.ClearBuff(bYang);
                            player.ClearBuff(bYin); ;
                        }
                        else
                        {
                            if (balance > 0)
                            {
                                player.ClearBuff(bYiYa);
                                player.AddBuff(bYang, time);
                                player.ClearBuff(bYin); ;
                            }
                            else
                            {
                                player.ClearBuff(bYiYa);
                                player.ClearBuff(bYang);
                                player.AddBuff(bYin, time);
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                #region Melee Power Buildup with Bosses
                // ONLY DO THIS CLIENT SIDE
                if (Main.myPlayer == player.whoAmI)
                {
                    bool nearBoss = false;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.friendly && npc.life > 0 && npc.damage > 0 &&
                            (npc.boss || npc.GetBossHeadTextureIndex() >= 0)) 
                        {
                            Vector2 distance = npc.position - player.position;
                            if(Math.Abs(distance.X) < 10000 || Math.Abs(distance.Y) < 10000) // 625ft, after which projectiles stop spawning
                            { nearBoss = true; }
                            break;
                        }
                    }
                    if (nearBoss)
                    {
                        if (player.itemAnimation == 0)
                        {
                            if (player.HeldItem.melee)
                            {
                                patienceBonus += patiencePerFrame * Math.Max(0, patienceBuildUpModifier);
                            }
                        }
                        // using ranged/magic will reduce effect drastically
                        else if (player.HeldItem.ranged || player.HeldItem.magic)
                        {
                            patienceBonus -= patiencePerFrame * 4f;
                        }
                        patienceBonus = Math.Max(patienceCooldown, Math.Min(patienceDamage, patienceBonus));
                        patienceDustUpdate = Math.Max(0, patienceDustUpdate + (int)(patienceBonus * 100f));

                        float size = 0.5f;
                        if (patienceBonus >= patienceDamage) size = 1f;
                        while (patienceDustUpdate > 1500)// 1 dust per 100% damage every 15 frames
                        {
                            patienceDustUpdate -= 1500;
                            float yVel = player.gravDir * 0.2f * patienceBonus;
                            Dust d = Dust.NewDustPerfect(new Vector2(
                                player.position.X + player.width * Main.rand.NextFloat(0f, 1f),
                                player.Center.Y + player.gfxOffY + player.height * player.gravDir / 2 + yVel),
                                90, new Vector2(0, -yVel),
                                0, default(Color), size);
                            d.position -= player.velocity;
                            d.noGravity = true;
                            d.customData = player;
                            Main.playerDrawDust.Add(d.dustIndex);
                        }
                    }
                    else
                    {
                        patienceBonus = patienceCooldown;
                        patienceDustUpdate = 0;
                    }
                    patienceDamage = 0f;
                }
                #endregion

                #region Momentum Max Effect
                if (momentum >= momentumMax)
                {
                    player.armorEffectDrawOutlinesForbidden = true;
                }
                #endregion

                #region Demon Blood Healing, heal removal
                if (demonBloodHealMod > 0f)
                {
                    if (demonBloodRally > 0)
                    {
                        demonBloodRally -= Math.Max(0, player.statLife - demonBloodRallyCurrentLife);
                    }
                    demonBloodRallyCurrentLife = player.statLife;
                }
                #endregion

                #region False Position Faker
                FakePositionReal = player.position;
                if (ghostPosition)
                {
                    if (Vector2.Distance(FakePositionReal, FakePositionTemp) > FakePositionDistance || player.velocity.Y == 0)
                    {
                        FakePositionTemp = FakePositionReal;
                    }
                    else
                    {
                        FakePositionTemp = (FakePositionTemp * (FakePositionLag - 1)
                            + player.position) / FakePositionLag;
                    }
                }
                else
                {
                    FakePositionTemp = player.position;
                }
                #endregion
            }
        }

        public float CalculateYangPower(float balance)
        {
            float yangPower;
            if (balance > yinyangBalanceThreshold)
            {
                yangPower = yang - yin;
                yangPower = (float)Math.Log10((yangPower / 2000f) + 1);
            }
            else if (balance < -yinyangBalanceThreshold)
            {
                yangPower = 0f;
            }
            else
            {
                yangPower = (float)Math.Log10((yang / 1000f) + 1);
            }
            
            if (Main.expertMode) yangPower *= 0.875f; // Same factor as lifesteal variable

            return yangPower / 2;
        }

        public float CalculateYinPower(float balance)
        {
            float yinPower;
            if (balance > yinyangBalanceThreshold)
            {
                yinPower = 0f;
            }
            else if (balance < -yinyangBalanceThreshold)
            {
                yinPower = yin - yang;
                yinPower = (float)Math.Log10((yinPower / 1000f) + 1);
            }
            else
            {
                yinPower = (float)Math.Log10((yin / 500f) + 1);
            }

            return yinPower;
        }

        private void FistOnHitNPC(NPC target, int damage)
        {
            if (ModConf.EnableFists)
            {
                if (target.immortal) return;
                #region Divekicks heal
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if (mpf.specialMove == 2 && diveKickHeal > 0f)
                {
                    PlayerFX.LifeStealPlayer(player, damage, target.lifeMax / 4, diveKickHeal);
                }
                #endregion

                #region Second Wind combo healing
                // Punches heal
                if (secondWind)
                {
                    secondWindLifeTax -= Math.Min(6, mpf.ComboCounter / 3);
                }
                #endregion

                #region Yin Yang - Yang
                if (mpf.ComboCounter > 0)
                {
                    yang += damage;
                }
                #endregion

                #region Potion Recovery
                if (debuffRecover)
                {
                    for (int i = 0; i < player.buffTime.Length; i++)
                    {
                        if (!Main.debuff[player.buffType[i]]) continue;
                        player.buffTime[i] = Math.Max(30, player.buffTime[i] - player.itemAnimationMax);
                    }
                }
                #endregion

                #region Bee Healing
                if (beeHealing)
                {
                    int healBeeType = ProjectileID.BeeArrow;
                    if (player.strongBees && Main.rand.Next(2) == 0)
                    { healBeeType = mod.ProjectileType<Projectiles.HoneyBeeBig>(); }
                    else
                    { healBeeType = mod.ProjectileType<Projectiles.HoneyBee>(); }

                    int targetPlayer = player.whoAmI;
                    if (Main.LocalPlayer.team != 0)
                    {
                        // Prefer players low on health and defence, and are melee
                        int weight = 0;
                        foreach (Player ally in Main.player)
                        {
                            // No dead/inactive or non-team players
                            if (!ally.active || ally.dead || ally.team != Main.LocalPlayer.team) continue;

                            int myWeight = ally.statLifeMax2 - ally.statLife;
                            myWeight -= ally.statDefense;
                            if (ally.HeldItem.melee) myWeight += ally.statLifeMax2 / 2;
                            if (myWeight > weight)
                            {
                                targetPlayer = ally.whoAmI;
                                weight = myWeight;
                            }
                        }
                    }

                    float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
                    float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
                    Projectile.NewProjectile(player.Center.X, player.Center.Y, speedX, speedY, healBeeType, 0, 0f, Main.myPlayer, targetPlayer);
                }
                #endregion

                if (target.realLife >= 0) target = Main.npc[target.realLife];

                HeartDropperCheck(target, damage, false);

                DemonBloodHealing(target, false);
            }
        }

        private void FistOnHitNPCWithProj(Projectile proj, NPC target, int damage)
        {
            if (ModConf.EnableFists)
            {
                if (!proj.melee) return;
                if (player.heldProj != proj.whoAmI) return;

                if (target.realLife >= 0) target = Main.npc[target.realLife];

                HeartDropperCheck(target, damage, true);

                DemonBloodHealing(target, true);
            }
        }

        private void HeartDropperCheck(NPC target, int damage, bool projectile)
        {
            if (!heartDropper) return;
            if (target.type == 16 || // These NPCs turn into other NPCs on death (Slimes)
                target.type == 81 ||
                target.type == 121 ||
                target.lifeMax <= 1 || // Ignore "projectile" NPCs
                target.damage <= 0) return;// Ignore critters, basically

            // Only check at full health and half health
            int divider = target.defense;
            if (PastThreshold(target.life + damage, target.life, target.lifeMax, divider))
            {
                int chance = 12; // Default 8.3% heart drop chance
                if (heartBuff) chance -= 4;
                if(!target.boss) chance *= 10; // Don't drop quite so often except bosses
                if (projectile) chance *= 3;
                
                if (Main.rand.Next(chance) == 0) 
                {
                    int itemWho = Item.NewItem((int)target.position.X, (int)target.position.Y, target.width, target.height, ItemID.Heart, 1, false, 0, false, false);
                    if (Main.netMode == 1)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWho, 1f, 0f, 0f, 0, 0, 0);
                    }
                }
            }
        }
        public static bool PastThreshold(int oldLife, int currentLife, int maxLife, float divider)
        {
            if (maxLife <= divider) return true;
            if (oldLife <= currentLife) return false;
            float threshold = 0f;
            int escapeCount = 0;
            bool same = false;
            for (int i = 0; i < divider; i++)
            {
                if (threshold < oldLife)
                {
                    same = (threshold >= currentLife);
                }
                if (same) return true;
                threshold += maxLife / divider;
                escapeCount++;
            }
            return false;
        }

        private void DemonBloodHealing(NPC target, bool projectile)
        {
            if (demonBloodHealMod > 0f)
            {
                // restores about mod% every second of hitting
                int heal = 0;
                if (projectile)
                { heal = CalculateDemonHealing(demonBloodHealMod, target.life <= 0, 6); }
                else
                { heal = CalculateDemonHealing(demonBloodHealMod, target.life <= 0); }

                heal -= demonBloodCooldown;
                if(heal > 0) demonBloodCooldown += heal;

                if (heal > demonBloodRally) heal = demonBloodRally;
                PlayerFX.HealPlayer(player, heal, false);
                if (player.lifeSteal > 0) player.lifeSteal -= heal;
                sashLifeLost = Math.Max(0, sashLifeLost - heal);
            }
        }

        private int CalculateDemonHealing(float percentPerSecond, bool targetDied, int useTime = 0)
        {
            if (demonBloodRally <= 0) return 0;
            if (useTime < 1) useTime = player.itemAnimationMax;
            return Math.Max(1, 
                (int)(player.statLifeMax * (targetDied ? 0.08f : 0.02f) * 
                percentPerSecond * player.itemAnimationMax / 60f));
        }

        private void FistOnHitByEntity(Entity e, int damage)
        {
            if (ModConf.EnableFists)
            {
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();

                #region Yin Yang - Yin
                yin += (int)(damage * 30 / Main.expertDamage); // Expert mode already hits like a truck
                #endregion
            }
        }
        
        private void FistModifyHit(Item item, Player player, int life, int lifeMax, ref int damage, ref float knockBack, ref bool crit)
        {
            if (ModConf.EnableFists)
            {
                #region Combo Health% Damage
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if (millstone && mpf.IsComboActive)
                {
                    damage += (int)(life * 0.002f);
                }
                #endregion

                #region Reset Melee Buildup
                if(player.HeldItem.melee)
                {
                    ApplyPatienceBonus(player, ref damage);
                }
                #endregion
            }
        }

        private void FistModifyHitWithProj(Projectile proj, Player player, int life, int lifeMax, ref int damage, ref bool crit)
        {
            if (ModConf.EnableFists)
            {
                #region Reset Melee Buildup
                if (proj.melee)
                {
                    ApplyPatienceBonus(player, ref damage);
                }
                #endregion
            }
        }

        private int ApplyPatienceBonus(Player player, ref int damage)
        {
            damage = (int)(damage + player.HeldItem.damage * player.meleeDamage * PatienceBonus);
            patienceBonus = patienceCooldown;
            return damage;
        }

        private bool FistPreHurt(int damage, PlayerDeathReason damageSource)
        {
            if (ModConf.EnableFists)
            {
                #region Momentum Damage Prevention (return false)
                if (momentumActive)
                {
                    if (damageSource.SourceProjectileIndex >= 0)
                    {
                        momentum /= 2;
                        player.AddBuff(mod.BuffType<Buffs.Momentum>(), 0, false);
                        // Can't reflect damage that's too powerful!
                        if (Main.projectile[damageSource.SourceProjectileIndex].damage > player.statLifeMax2 / 8)
                        {
                            player.immune = true;
                            player.immuneTime = 20;
                            if (player.longInvince) player.immuneTime += 20;
                        }
                        else
                        {
                            ProjFX.ReflectProjectilePlayer(Main.projectile[damageSource.SourceProjectileIndex], player);
                            Main.PlaySound(SoundID.Item10, player.position);
                        }
                        return false;
                    }
                    else if (damageSource.SourceNPCIndex >= 0)
                    {
                        momentum = 0;
                        if (!Main.npc[damageSource.SourceNPCIndex].dontTakeDamage) {

                            player.ApplyDamageToNPC(Main.npc[damageSource.SourceNPCIndex], player.statLife / 4, 3f + Math.Abs(player.velocity.X) * 2f, player.direction, false);
                            Main.PlaySound(SoundID.Item10, player.position);
                            player.AddBuff(mod.BuffType<Buffs.Momentum>(), 0, false);
                            player.immuneTime += 60;
                            return false;
                        }
                    }
                }
                #endregion
            }
            return true;
        }

        private void FistPostHurt(double damage)
        {
            if (ModConf.EnableFists)
            {
                #region Counter Damage Buff
                if (taekwonCounter)
                {
                    player.AddBuff(mod.BuffType<Buffs.DamageUp>(), 60 + Math.Abs(player.statLife - player.statLifeMax2) / 2);
                }
                #endregion

                #region Rapid Recovery Buff
                if (rapidRecovery)
                {
                    Buffs.RapidRecovery.HealDamage(player, mod, damage);
                }
                #endregion

                #region Angry Combo
                if (angryCombo)
                {
                    if (player.itemAnimation > 0)
                    {
                        ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                        int bonus = 1;
                        if (damage > 50) bonus++;
                        if (damage > 100) bonus++;
                        if (damage > 175) bonus++;
                        if (damage > 300) bonus++;
                        mpf.ModifyComboCounter(bonus);
                    }
                }
                #endregion
                
                #region Demon Blood
                if (demonBloodHealMod > 0f)
                {
                    demonBloodRally = (int)damage;
                    demonBloodRallyDelay = demonBloodReallyDelayMax;
                }
                #endregion
            }
        }

        private bool FistPreKill(double damage, PlayerDeathReason damageSource)
        {
            if (ModConf.EnableFists)
            {
                #region Second Wind Damage
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
                #endregion
            }
            return true;
        }

        private void MomentumDashTorwardsMouse()
        {
            if (!momentumDash || momentum < momentumMax) return;
            momentum = 0;
            momentumDashTime = 6;

            player.AddBuff(mod.BuffType<Buffs.DamageUp>(), 90);
            player.immune = true;
            player.immuneTime = Math.Max(10, player.immuneTime);
            if (player.longInvince) player.immuneTime += 10;

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

        private void sashRestoreLogic()
        {
            if (!ModConf.enableFists ||
                sashMaxLifeRecoverMult <= 0f ||
                player.whoAmI != Main.myPlayer) return;

            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.ComboCounter > 0)
            {
                recordLifeLost = true;

                // Lost health? Records lost amount, but quite as much when leeched (because you're in his face)
                if (player.statLife < sashLastLife)
                {
                    if (player.moonLeech)
                    {
                        sashLifeLost += ((sashLastLife - player.statLife) / 4 * 3);
                    }
                    else
                    {
                        sashLifeLost += sashLastLife - player.statLife;
                    }
                }
                sashLastLife = player.statLife;

                sashLifeLost = lifeRestorable(player);

                if (sashLifeLost == 0)
                { player.AddBuff(mod.BuffType<Buffs.FightingSpiritEmpty>(), 1); }
                else if (sashLifeLost >= (int)(player.statLifeMax2 * sashMaxLifeRecoverMult))
                {
                    if (player.FindBuffIndex(mod.BuffType<Buffs.FightingSpiritMax>()) < 0)
                    {
                        Main.PlaySound(SoundID.Tink, player.position);
                    }
                    player.AddBuff(mod.BuffType<Buffs.FightingSpiritMax>(), 2);
                }
                else
                { player.AddBuff(mod.BuffType<Buffs.FightingSpirit>(), 1); }
            }
            else if (sashLifeLost > 0)
            {
                Main.PlaySound(2, -1, -1, 4, 0.3f, 0.2f); // mini heal effect
                PlayerFX.HealPlayer(player, sashLifeLost);
            }
        }
        private int lifeRestorable(Player player)
        { return Math.Min((int)(player.statLifeMax2 * sashMaxLifeRecoverMult), sashLifeLost); }
        #endregion

        private const int criticalHealMax = 60;
        private void CriticalHeartHit(int damage)
        {
            if (!criticalHealStar) return;
            if (player.dead) return;

            if (damage > criticalHealMax) damage = criticalHealMax;
            int amount = damage / 10;
            if (!player.HeldItem.melee) amount = damage / 20;

            for (int i = 0; i < amount; i++)
            {
                int itemAmI = Item.NewItem(player.Hitbox, mod.ItemType<Items.SparkleStar>(), 1);
                Item item = Main.item[itemAmI];
                item.velocity.X *= 1f + (0.05f * damage);
                item.velocity.Y -= Math.Abs(item.velocity.X) / 10;
                if (Main.netMode == 1)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemAmI, 1f, 0f, 0f, 0, 0, 0);
                }
            }
        }

        private bool hookPressed = false;
        private void discordItemsCheck()
        {
            if (hookPressed && player.whoAmI == Main.myPlayer)
            {
                bool canTeleHook = false;
                bool checkChaosState = false;
                // Misc equips take priority
                if (player.miscEquips[4].type == mod.ItemType<Items.Accessories.ChaosHook>())
                {
                    canTeleHook = true;
                    checkChaosState = false;
                }
                else if (player.miscEquips[4].type == mod.ItemType<Items.Accessories.DiscordHook>())
                {
                    canTeleHook = true;
                    checkChaosState = true;
                }
                else
                {
                    foreach (Item item in player.armor)
                    {
                        if (item.type == mod.ItemType<Items.Armour.DiscordantCharm>() ||
                            item.type == mod.ItemType<Items.Armour.DiscordantShades>())
                        {
                            foreach (Item i in player.inventory)
                            {
                                if (i.type == ItemID.RodofDiscord)
                                {
                                    canTeleHook = true;
                                    checkChaosState = true;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                }

                if (canTeleHook)
                {
                    DiscordTeleport(player, checkChaosState);
                }
            }
        }
        internal static void DiscordTeleport(Player player, bool checkChaosState = false)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (checkChaosState && player.FindBuffIndex(BuffID.ChaosState) >= 0) return;

                Vector2 vector32;
                vector32.X = (float)Main.mouseX + Main.screenPosition.X;
                if (player.gravDir == 1f)
                {
                    vector32.Y = (float)Main.mouseY + Main.screenPosition.Y - (float)player.height;
                }
                else
                {
                    vector32.Y = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;
                }
                vector32.X -= (float)(player.width / 2);
                if (vector32.X > 50f && vector32.X < (float)(Main.maxTilesX * 16 - 50) && vector32.Y > 50f && vector32.Y < (float)(Main.maxTilesY * 16 - 50))
                {
                    int num246 = (int)(vector32.X / 16f);
                    int num247 = (int)(vector32.Y / 16f);
                    if ((Main.tile[num246, num247].wall != 87 || (double)num247 <= Main.worldSurface || NPC.downedPlantBoss) && !Collision.SolidCollision(vector32, player.width, player.height))
                    {
                        player.Teleport(vector32, 1, 0);
                        NetMessage.SendData(65, -1, -1, null, 0, (float)player.whoAmI, vector32.X, vector32.Y, 1, 0, 0);
                        if (player.chaosState)
                        {
                            player.statLife -= player.statLifeMax2 / 7;

                            PlayerDeathReason damageSource = PlayerDeathReason.ByOther(13);
                            if (Main.rand.Next(2) == 0)
                            {
                                damageSource = PlayerDeathReason.ByOther(player.Male ? 14 : 15);
                            }
                            if (player.statLife <= 0)
                            {
                                player.KillMe(damageSource, 1.0, 0, false);
                            }

                            player.lifeRegenCount = 0;
                            player.lifeRegenTime = 0;
                        }
                        player.AddBuff(88, 360, true);
                    }
                }
            }
        }

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

        /// <summary>
        /// Handles healing and multiplayer syncing
        /// </summary>
        public static void HealPlayer(Player player, int amount, bool moonLeechable = false)
        {
            if (moonLeechable && player.moonLeech) return;
            if (amount <= 0) return;
            player.HealEffect(amount, true);
            player.statLife += amount;
            player.statLife = Math.Min(player.statLife, player.statLifeMax2);
            if (Main.netMode == 1 && Main.myPlayer == player.whoAmI) NetMessage.SendData(MessageID.PlayerHealth, -1, -1, null, player.whoAmI);
        }
        public static void LifeStealPlayer(Player player, int amount, int maxAmount, float healFactor = 1f)
        {
            if (player.moonLeech) return;
            amount = (int)(Math.Min(maxAmount, amount) * healFactor);
            if (player.lifeSteal <= 0f) return;
            player.lifeSteal -= amount; // lifeSteal caps at 30-36 per second (see Player.cs) 
            PlayerFX.HealPlayer(player, amount, true);
        }
    }
}