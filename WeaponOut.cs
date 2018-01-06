﻿using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics;

/*
 RARITY:
 * 1 - Special/Unique early armour + weapons
 * 2 - Dungeon loot and other decent mid-early things
 * 3 - Hell/Jungle
 * 4 - Hardmode ore stuff
 * 5 - Mechanical Bosses
 * 6 - Unique/Powerful (intermediate)
 * 7 - Chloropyhte and Plantera Loot
 * 8 - Golem Loot, the Terrarian
 * 9 - Lunar materials, dev loot
 * 10 - Ancient Manipulator + Moonlord
 */

namespace WeaponOut
{
    public class WeaponOut : Mod
    {
        internal static Mod mod;

        public static Texture2D dHeart;
        public static Texture2D pumpkinMark;

        public static int BuffIDManaReduction;
        public static int BuffIDMirrorBarrier;

        public static int DustIDManaDust;

        public static int shakeIntensity = 0;
        private static int shakeTick = 0;
        
        // WeaponOut Hotkey
        ModHotKey ControlToggleVisual = null;

        public WeaponOut()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
            ModConf.Load();
        }

        public override void Load()
        {
            mod = this;

            ControlToggleVisual = RegisterHotKey("Toggle WeaponOut", "#");
        }
        public static string GetTranslationTextValue(string key) {
            return Terraria.Localization.Language.GetText("Mods.WeaponOut." + key).Value;
        }

        public override void PostSetupContent()
        {

            if (ModConf.EnableAccessories) BuffIDMirrorBarrier = GetBuff("MirrorBarrier").Type;
            if (ModConf.EnableDualWeapons) BuffIDManaReduction = GetBuff("ManaReduction").Type;

            DustIDManaDust = GetDust<Dusts.ManaDust>().Type;

            if (ModConf.EnableEmblems) Items.Accessories.HeliosphereEmblem.SetUpGlobalDPS();

            if (Main.netMode != 2)
            {
				dHeart = mod.GetTexture("Gores/DemonHearts");
                pumpkinMark = mod.GetTexture("Gores/PumpkinMark");
            }
            else
            {
                Console.WriteLine("WeaponOut loaded:    fistsupdate2#01");
            }
        }

        /// <summary>
        /// Registers a glowmask texture to the game's array, and returns that value.
        /// The file should be located under Glow/ItemName_Glow. Make sure to register
        /// the returned value under item.glowMask in SetDefaults.
        /// </summary>
        /// <param name="modItem">The mod item to register. </param>
        /// <returns></returns>
        public static short SetStaticDefaultsGlowMask(ModItem modItem)
        {
            if (!Main.dedServ)
            {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++)
                {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = mod.GetTexture("Glow/" + modItem.GetType().Name + "_Glow");
                Main.glowMaskTexture = glowMasks;
                return (short)(glowMasks.Length - 1);
            }
            else return 0;
        }

        /// <summary>
        /// Handy dandy game method for implementating screen shake
        /// </summary>
        /// <param name="Transform"></param>
        /// <returns></returns>
        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
            if (!Main.gameMenu) {
                shakeTick++;
                if (shakeIntensity >= 0 && shakeTick >= 12) shakeIntensity--;
                if (shakeIntensity > 10) shakeIntensity = 10;//cap it
                if (shakeIntensity < 0) shakeIntensity = 0;
                if (!Main.gamePaused && Main.hasFocus) {
                    Main.screenPosition += new Vector2(
                        shakeIntensity * Main.rand.NextFloatDirection() / 2f,
                        shakeIntensity * Main.rand.NextFloatDirection() / 2f);
                }
            }
            else {
                shakeIntensity = 0;
                shakeTick = 0;
            }
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            try { DrawPumpkinMark(spriteBatch); } catch { }
            DrawInterfaceDemonBloodHeart(spriteBatch);
            if(ModConf.EnableFists)
            {
                Main.LocalPlayer.meleeDamage += Main.LocalPlayer.GetModPlayer<PlayerFX>().PatienceBonus;
            }

            DrawInterfaceWeaponOutToggleEye(spriteBatch);
        }

        private void DrawPumpkinMark(SpriteBatch spriteBatch)
        {
            if (!ModConf.enableFists) return;
            if (Main.gameMenu) return;

            int buffID = BuffType<Buffs.PumpkinMark>();
            Dictionary<Vector2, bool> drawPositions = new Dictionary<Vector2, bool>();
            foreach (NPC i in Main.npc)
            {
                if (!i.active || i.life <= 0) continue;
                if (i.buffImmune.Length < buffID) continue;

                int buffIndex = i.FindBuffIndex(buffID);

                // In case two NPCs occupy the same position
                Vector2 centre = i.Center + new Vector2(0, i.gfxOffY);
                if (buffIndex >= 0 && !drawPositions.ContainsKey(centre)) {
                    drawPositions.Add(centre, i.buffTime[buffIndex] < 120);
                }
            }
            foreach (Player i in Main.player)
            {
                Vector2 centre = i.Center + new Vector2(0, i.gfxOffY);
                if (i.active && !i.dead && i.FindBuffIndex(buffID) >= 0 && !drawPositions.ContainsKey(centre))
                {
                    drawPositions.Add(i.Center + new Vector2(0, i.gfxOffY), false);
                }
            }

            if (drawPositions.Count > 0)
            {
                int frameHeight = pumpkinMark.Height / 3;
                int frameY = 0;
                int explodeFrameY = frameY;
                if ((int)(Main.time / 6) % 2 == 0)
                { explodeFrameY = frameHeight; }
                else
                { explodeFrameY = frameHeight * 2; }
                if (ModPlayerFists.Get(Main.LocalPlayer).GetParryBuff() >= 0)
                {
                    frameY = explodeFrameY;
                }
                spriteBatch.End();

                //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
                foreach (KeyValuePair<Vector2, bool> kvp in drawPositions)
                {
                    spriteBatch.Draw(pumpkinMark, (kvp.Key - Main.screenPosition),
                        new Rectangle(0, kvp.Value ? explodeFrameY : frameY, pumpkinMark.Width, frameHeight),
                        new Color(0.8f, 0.8f, 0.8f, 0.5f), 0f, new Vector2(pumpkinMark.Width / 2, frameHeight / 2),
                        1f, SpriteEffects.None, 0f);

                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(Main.UIScale, Main.UIScale, 1f));
            }
        }

        private void DrawInterfaceDemonBloodHeart(SpriteBatch spriteBatch)
        {
            if (!ModConf.enableFists) return;
            Player p = Main.LocalPlayer;
            PlayerFX pfx = p.GetModPlayer<PlayerFX>();
            if (pfx.demonBloodRally <= 0) return;

            float lifePerHeart = Math.Max(20f, p.statLifeMax2 / 20f);
            // 2 rows of 10
            int numOfHearts = (int)Math.Min(20f, p.statLifeMax2 / lifePerHeart);
            float rally = p.statLife + pfx.demonBloodRally;

            bool firstHeart = !p.dead;
            float currentHeartLife = 0;
            int frame = 1;
            int frameHeight = dHeart.Height / 2;
            int heartOffsetHeight = Main.heartTexture.Height + (frameHeight - Main.heartTexture.Height) / 2;
            int ScreenAnchorX = Main.screenWidth - 800;
            Vector2 basePos = new Vector2(ScreenAnchorX + 500, 32);
            // Two rows of 10 columns
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (numOfHearts <= 0) { break; } // Don't go over heart limit
                    numOfHearts--;
                    currentHeartLife = 1 + lifePerHeart * (x + 1) + lifePerHeart * 10 * y;

                    float hpNormal = 1f;

                    if (currentHeartLife <= p.statLife) continue; // Not at rally amonut yet
                    else if (currentHeartLife > rally + lifePerHeart) continue; // higher than rally amount
                    else if (currentHeartLife >= rally)
                    {
                        hpNormal = (rally + lifePerHeart - currentHeartLife) / lifePerHeart;
                    }
                    else if (currentHeartLife <= rally + lifePerHeart)
                    {
                        hpNormal = (rally - p.statLife) / lifePerHeart;
                    }

                    if (hpNormal <= 0f) continue;
                    if (hpNormal > 1f) hpNormal = 1f;

                    float alpha = hpNormal;
                    float size = 0.75f;

                    frame = 1;
                    if (firstHeart)
                    {
                        alpha = 0.25f + alpha * 0.75f;
                        float heartNormInverse = ((p.statLife - currentHeartLife) / -lifePerHeart);
                        size = Main.cursorScale - 0.25f * heartNormInverse;
                        frame = 0;
                    }

                    spriteBatch.Draw(dHeart,
                        basePos + new Vector2(
                            26 * x + Main.heartTexture.Width / 2,
                            26 * y + Main.heartTexture.Height
                            ),
                        new Rectangle(0, frame * frameHeight, dHeart.Width, frameHeight - 1),
                        new Color(hpNormal, hpNormal, hpNormal, hpNormal),
                        0f,
                        new Vector2(dHeart.Width / 2, heartOffsetHeight),
                        size, SpriteEffects.None, 0);
                    firstHeart = false;
                }
            }
        }

        private void DrawInterfaceWeaponOutToggleEye(SpriteBatch spriteBatch)
        {
            //if (Disabled) return;

            // Janky quick inventory visibilty
            if (!Main.playerInventory || !ModConf.showWeaponOut || ModConf.forceShowWeaponOut) return;
            //Get vars
            PlayerWOFX pfx = Main.LocalPlayer.GetModPlayer<PlayerWOFX>(this);
            Texture2D eye = Main.inventoryTickOnTexture;
            string hoverText = "Weapon " + Lang.inter[59]; // Visible
            Vector2 position = new Vector2(20, 10);

            // Show hidden instead
            if (!pfx.weaponVisual)
            {
                eye = Main.inventoryTickOffTexture;
                hoverText = "Weapon " + Lang.inter[60]; // Hidden
            }

            // Get rectangle for eye
            Rectangle eyeRect = new Rectangle(
                (int)position.X, (int)position.Y - (eye.Height / 2),
                eye.Width, eye.Height);
            if (eyeRect.Contains(Main.mouseX, Main.mouseY))
            {
                // Prevent item use and show text
                Main.hoverItemName = hoverText;
                Main.blockMouse = true;

                // On click
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    ToggleWeaponVisual(pfx, !pfx.weaponVisual);
                }
            }

            // Draw this!
            spriteBatch.Draw(
                eye,
                new Vector2(20, 4),
                null,
                Color.White
                );
        }
        private void ToggleWeaponVisual(PlayerWOFX pfx, bool state) {
            Main.PlaySound(SoundID.MenuTick);
            pfx.weaponVisual = state;
            NetUpdateWeaponVisual(this, pfx);
        }

        #region Netcode
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int code = reader.ReadInt32();
            int sender = reader.ReadInt32();
            #region Dash
            if (code == 1) // Set dash used
            {
                HandlePacketDash(reader, code, sender);
            }
            #endregion

            #region Parrying
            if (code == 2) // Set parry move
            {
                HandlePacketParry(reader, code, sender);
            }
            #endregion

            #region Weapon Visual
            if (code == 3) // Set weapon
            {
                HandlePacketWeaponVisual(reader, code, sender);
            }
            #endregion

            #region Combo Special
            if (code == 4) // Set combo altfunction
            {
                HandlePacketCombo(reader, code, sender);
            }
            #endregion
        }

        public static void NetUpdateDash(ModPlayerFists mpf)
        {
            if (Main.netMode == 1 && mpf.player.whoAmI == Main.myPlayer)
            {
                //-/ Main.NewText("sent " + mpf.dashSpeed + "dash " + mpf.dashEffect + " from " + Main.myPlayer);
                ModPacket message = mpf.mod.GetPacket();
                message.Write(1);
                message.Write(Main.myPlayer);
                message.Write(mpf.dashSpeed);
                message.Write(mpf.dashMaxSpeedThreshold);
                message.Write(mpf.dashMaxFriction);
                message.Write(mpf.dashMinFriction);
                message.Write(mpf.dashEffect);
                message.Send();
            }
        }
        private void HandlePacketDash(BinaryReader reader, int code, int sender)
        {
            float dSpeed = reader.ReadSingle();
            float dThreshold = reader.ReadSingle();
            float dMax = reader.ReadSingle();
            float dMin = reader.ReadSingle();
            int dEffect = reader.ReadInt32();
            if (Main.netMode == 2)
            {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(dSpeed);
                me.Write(dThreshold);
                me.Write(dMax);
                me.Write(dMin);
                me.Write(dEffect);
                me.Send(-1, sender);
                //-/ Console.WriteLine("echo " + dSpeed + " dash " + dEffect + " from " + sender);
            }
            else
            {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>();
                pfx.SetDash(dSpeed, dThreshold, dMax, dMin, true, dEffect);
            }
        }

        public static void NetUpdateParry(ModPlayerFists pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
                //-/ Main.NewText("sent parry from " + Main.myPlayer);
                ModPacket message = pfx.mod.GetPacket();
                message.Write(2);
                message.Write(Main.myPlayer);
                message.Write(pfx.parryTimeMax);
                message.Write(pfx.parryWindow);
                message.Send();
            }
        }
        private void HandlePacketParry(BinaryReader reader, int code, int sender)
        {
            int parryTimeMax = reader.ReadInt32();
            int parryWindow = reader.ReadInt32();
            if (Main.netMode == 2)
            {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(parryTimeMax);
                me.Write(parryWindow);
                me.Send(-1, sender);
                //-/ Console.WriteLine("received parry from " + sender);
            }
            else
            {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>(this);
                pfx.AltFunctionParryMax(Main.player[sender], parryWindow, parryTimeMax);
                //-/ Main.NewText("received parry from " + sender);
            }
        }

        public static void NetUpdateCombo(ModPlayerFists pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
                //-/ Main.NewText("sent combo " + pfx.ComboEffectAbs + " from " + Main.myPlayer);
                ModPacket message = pfx.mod.GetPacket();
                message.Write(4);
                message.Write(Main.myPlayer);
                message.Write(pfx.ComboEffectAbs);
                message.Send();
            }
        }
        private void HandlePacketCombo(BinaryReader reader, int code, int sender)
        {
            int comboEffect = reader.ReadInt32();
            if (Main.netMode == 2)
            {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(comboEffect);
                me.Send(-1, sender);
                //-/ Console.WriteLine("echo combo " + comboEffect + " from " + sender);
            }
            else
            {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>(this);
                pfx.player.itemAnimation = 0;
                pfx.AltFunctionCombo(Main.player[sender], comboEffect, true);
                //-/ Main.NewText("received combo " + comboEffect + " from " + sender);
            }
        }

        public static void NetUpdateWeaponVisual(Mod mod, PlayerWOFX pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
                ModPacket message = mod.GetPacket();
                message.Write(3);
                message.Write(Main.myPlayer);
                message.Write(pfx.WeaponVisual);
                message.Send();
            }
        }
        private void HandlePacketWeaponVisual(BinaryReader reader, int code, int sender)
        {
            bool weaponVis = reader.ReadBoolean();
            if (Main.netMode == 2)
            {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(weaponVis);
                me.Send(-1, sender);
            }
            else
            {
                PlayerWOFX pfx = Main.player[sender].GetModPlayer<PlayerWOFX>(this);
                pfx.weaponVisual = weaponVis;
            }
        }
        #endregion
        
        #region Mod Calls
        public override object Call(params object[] args) {
            if (args.Length < 1) return null;
            if (args[1].GetType() != typeof(string)) return null;
            string method = (string)args[0];
            try {
                switch (method) {
                    case "SetPlayerWeaponVisual":
                        CallShowWeapon((Player)args[1], (bool)args[2]);
                        break;
                }
            }
            catch { }
            return null;
        }
        private void CallShowWeapon(Player p, bool show) {
            ToggleWeaponVisual(p.GetModPlayer<PlayerWOFX>(this), show);
        }
        #endregion

        public override void PostUpdateInput() {
            if (Main.gameMenu || ControlToggleVisual == null || ModConf.ForceShowWeaponOut) return;
            if (ControlToggleVisual.JustPressed) {
                PlayerWOFX pfx = Main.LocalPlayer.GetModPlayer<PlayerWOFX>(this);
                ToggleWeaponVisual(pfx, !pfx.weaponVisual);
            }
        }

        public static Vector2 CalculateNormalAngle(Vector2 start, Vector2 end)
        {
            Vector2 diff = end - start;
            diff.Normalize();
            return diff;
        }
    }
}
