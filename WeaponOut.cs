using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.DataStructures;

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
        internal static WeaponOut mod;
        internal static Mod modOverhaul;

        /// <summary>
        /// Holds a list of custom draw functions that can be added to by other mods.
        /// </summary>
        internal List<Func<Player, Item, DrawData, bool>> weaponOutCustomPreDrawMethods;
        internal List<Func<Player, Item, int, int>> weaponOutCustomHoldMethods;

        public static Texture2D dHeart;
        public static Texture2D pumpkinMark;

        public static int BuffIDManaReduction;
        public static int BuffIDMirrorBarrier;

        public static int DustIDManaDust;
        public static int DustIDSlashFX;

        public static int shakeIntensity = 0;
        private static int shakeTick = 0;

        // WeaponOut Hotkey
        ModHotKey ControlToggleVisual = null;

        public WeaponOut() {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
            weaponOutCustomPreDrawMethods = new List<Func<Player, Item, DrawData, bool>>();
            weaponOutCustomHoldMethods = new List<Func<Player, Item, int, int>>();
        }

        public override void PreSaveAndQuit()
        {
            ModConfWeaponOutCustom.SaveConfig();
        }

        public override void Load() {
            mod = this;
            modOverhaul = ModLoader.GetMod("TerrariaOverhaul");

            ModTranslation text;

            text = mod.CreateTranslation("WOVisualControl");
            text.SetDefault("Toggle WeaponOut");
            text.AddTranslation(GameCulture.Chinese, "切换WeaponOut");
            text.AddTranslation(GameCulture.Russian, "Вкл/Выкл WeaponOut");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualShow");
            text.SetDefault("Weapon Visible");
            text.AddTranslation(GameCulture.Chinese, "显示武器");
            text.AddTranslation(GameCulture.Russian, "Показать оружие");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualHide");
            text.SetDefault("Weapon Hidden");
            text.AddTranslation(GameCulture.Chinese, "隐藏武器");
            text.AddTranslation(GameCulture.Russian, "Спрятать оружие");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeHand");
            text.SetDefault("Hand");
            text.AddTranslation(GameCulture.Russian, "Рука");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeWaist");
            text.SetDefault("Belt");
            text.AddTranslation(GameCulture.Russian, "Пояс");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeBack");
            text.SetDefault("Back");
            text.AddTranslation(GameCulture.Russian, "Спина");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeSpear");
            text.SetDefault("Pole");
            text.AddTranslation(GameCulture.Russian, "Копьё");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypePowerTool");
            text.SetDefault("Power Tool");
            text.AddTranslation(GameCulture.Russian, "Механический инструмент");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeBow");
            text.SetDefault("Bow");
            text.AddTranslation(GameCulture.Russian, "Лук");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeSmallGun");
            text.SetDefault("Handgun");
            text.AddTranslation(GameCulture.Russian, "Пистолет");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeLargeGun");
            text.SetDefault("Firearm");
            text.AddTranslation(GameCulture.Russian, "Ружьё");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("WOVisualTypeStaff");
            text.SetDefault("Staff");
            text.AddTranslation(GameCulture.Russian, "Посох");
            mod.AddTranslation(text);

            ControlToggleVisual = RegisterHotKey(GetTranslationTextValue("WOVisualControl"), "#");
        }

        public override void Unload()
        {
            mod = null;
            modOverhaul = null;
            weaponOutCustomPreDrawMethods.Clear();
            weaponOutCustomHoldMethods.Clear();
        }


        internal static string GetTranslationTextValue(string key) {
            return Language.GetText("Mods.WeaponOut." + key).Value;
        }

        public override void PostSetupContent() {
            ModConf.Load();
            if (ModConf.ShowWeaponOut) ModConfWeaponOutCustom.Load();
            if (ModConf.EnableAccessories) BuffIDMirrorBarrier = GetBuff("MirrorBarrier").Type;
            if (ModConf.EnableDualWeapons) BuffIDManaReduction = GetBuff("ManaReduction").Type;

            DustIDManaDust = GetDust<Dusts.ManaDust>().Type;
            DustIDSlashFX = GetDust<Dusts.SlashDust>().Type;

            if (ModConf.EnableEmblems) Items.Accessories.HeliosphereEmblem.SetUpGlobalDPS();

            Call("AddCustomPreDrawMethod", WOPreDrawData);

            if (Main.netMode != 2) {
                dHeart = mod.GetTexture("Gores/DemonHearts");
                pumpkinMark = mod.GetTexture("Gores/PumpkinMark");
            }
            else {
                Console.WriteLine("WeaponOut loaded:    qol#01");
            }
        }

        /// <summary>
        /// Registers a glowmask texture to the game's array, and returns that value.
        /// The file should be located under Glow/ItemName_Glow. Make sure to register
        /// the returned value under item.glowMask in SetDefaults.
        /// </summary>
        /// <param name="modItem">The mod item to register. </param>
        /// <returns></returns>
        public static short SetStaticDefaultsGlowMask(ModItem modItem) {
            if (!Main.dedServ) {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++) {
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

        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            try { DrawPumpkinMark(spriteBatch); } catch { }
            DrawInterfaceDemonBloodHeart(spriteBatch);
            if (ModConf.EnableFists) {
                Main.LocalPlayer.meleeDamage += Main.LocalPlayer.GetModPlayer<PlayerFX>().PatienceBonus;
            }

            DrawInterfaceWeaponOutToggleEye(spriteBatch);
        }

        private void DrawPumpkinMark(SpriteBatch spriteBatch) {
            if (!ModConf.enableFists) return;
            if (Main.gameMenu) return;

            int buffID = BuffType<Buffs.PumpkinMark>();
            Dictionary<Vector2, bool> drawPositions = new Dictionary<Vector2, bool>();
            foreach (NPC i in Main.npc) {
                if (!i.active || i.life <= 0) continue;
                if (i.buffImmune.Length < buffID) continue;

                int buffIndex = i.FindBuffIndex(buffID);

                // In case two NPCs occupy the same position
                Vector2 centre = i.Center + new Vector2(0, i.gfxOffY);
                if (buffIndex >= 0 && !drawPositions.ContainsKey(centre)) {
                    drawPositions.Add(centre, i.buffTime[buffIndex] < 120);
                }
            }
            foreach (Player i in Main.player) {
                Vector2 centre = i.Center + new Vector2(0, i.gfxOffY);
                if (i.active && !i.dead && i.FindBuffIndex(buffID) >= 0 && !drawPositions.ContainsKey(centre)) {
                    drawPositions.Add(i.Center + new Vector2(0, i.gfxOffY), false);
                }
            }

            if (drawPositions.Count > 0) {
                int frameHeight = pumpkinMark.Height / 3;
                int frameY = 0;
                int explodeFrameY = frameY;
                if ((int)(Main.time / 6) % 2 == 0) { explodeFrameY = frameHeight; }
                else { explodeFrameY = frameHeight * 2; }
                if (ModPlayerFists.Get(Main.LocalPlayer).GetParryBuff() >= 0) {
                    frameY = explodeFrameY;
                }
                spriteBatch.End();

                //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
                foreach (KeyValuePair<Vector2, bool> kvp in drawPositions) {
                    spriteBatch.Draw(pumpkinMark, (kvp.Key - Main.screenPosition),
                        new Rectangle(0, kvp.Value ? explodeFrameY : frameY, pumpkinMark.Width, frameHeight),
                        new Color(0.8f, 0.8f, 0.8f, 0.5f), 0f, new Vector2(pumpkinMark.Width / 2, frameHeight / 2),
                        1f, SpriteEffects.None, 0f);

                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(Main.UIScale, Main.UIScale, 1f));
            }
        }

        private void DrawInterfaceDemonBloodHeart(SpriteBatch spriteBatch) {
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
            for (int y = 0; y < 2; y++) {
                for (int x = 0; x < 10; x++) {
                    if (numOfHearts <= 0) { break; } // Don't go over heart limit
                    numOfHearts--;
                    currentHeartLife = 1 + lifePerHeart * (x + 1) + lifePerHeart * 10 * y;

                    float hpNormal = 1f;

                    if (currentHeartLife <= p.statLife) continue; // Not at rally amonut yet
                    else if (currentHeartLife > rally + lifePerHeart) continue; // higher than rally amount
                    else if (currentHeartLife >= rally) {
                        hpNormal = (rally + lifePerHeart - currentHeartLife) / lifePerHeart;
                    }
                    else if (currentHeartLife <= rally + lifePerHeart) {
                        hpNormal = (rally - p.statLife) / lifePerHeart;
                    }

                    if (hpNormal <= 0f) continue;
                    if (hpNormal > 1f) hpNormal = 1f;

                    float alpha = hpNormal;
                    float size = 0.75f;

                    frame = 1;
                    if (firstHeart) {
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

        private void DrawInterfaceWeaponOutToggleEye(SpriteBatch spriteBatch) {
            //if (Disabled) return;

            // Janky quick inventory visibilty
            if (!Main.playerInventory || !ModConf.showWeaponOut || ModConf.forceShowWeaponOut) return;
            //Get vars
            PlayerWOFX pfx = Main.LocalPlayer.GetModPlayer<PlayerWOFX>(this);
            Texture2D eye = Main.inventoryTickOnTexture;
            string hoverText = GetTranslationTextValue("WOVisualShow"); // Visible
            Vector2 position = new Vector2(20, 10);

            // Display custom styling
            int style;
            if (ModConfWeaponOutCustom.TryGetCustomHoldStyle(Main.LocalPlayer.HeldItem.type, out style))
            {
                switch (style)
                {
                    case 1: hoverText += ": " + GetTranslationTextValue("WOVisualTypeHand"); break;
                    case 2: hoverText += ": " + GetTranslationTextValue("WOVisualTypeWaist"); break;
                    case 3: hoverText += ": " + GetTranslationTextValue("WOVisualTypeBack"); break;
                    case 4: hoverText += ": " + GetTranslationTextValue("WOVisualTypeSpear"); break;
                    case 5: hoverText += ": " + GetTranslationTextValue("WOVisualTypePowerTool"); break;
                    case 6: hoverText += ": " + GetTranslationTextValue("WOVisualTypeBow"); break;
                    case 7: hoverText += ": " + GetTranslationTextValue("WOVisualTypeSmallGun"); break;
                    case 8: hoverText += ": " + GetTranslationTextValue("WOVisualTypeLargeGun"); break;
                    case 9: hoverText += ": " + GetTranslationTextValue("WOVisualTypeStaff"); break;
                }
            }

            // Show hidden instead
            if (!pfx.weaponVisual) {
                eye = Main.inventoryTickOffTexture;
                hoverText = GetTranslationTextValue("WOVisualHide"); // Hidden
            }

            // Get rectangle for eye
            Rectangle eyeRect = new Rectangle(
                (int)position.X, (int)position.Y - (eye.Height / 2),
                eye.Width, eye.Height);
            if (eyeRect.Contains(Main.mouseX, Main.mouseY)) {
                // Prevent item use and show text
                Main.hoverItemName = hoverText;
                Main.blockMouse = true;

                // On plain click
                if (!Main.mouseRight && Main.mouseLeft && Main.mouseLeftRelease) {
                    ToggleWeaponVisual(pfx, !pfx.weaponVisual);
                }
                if (pfx.weaponVisual)
                {
                    // On alt click
                    if (Main.mouseRight && Main.mouseRightRelease)
                    {
                        ModConfWeaponOutCustom.UpdateCustomHoldIncrement(Main.LocalPlayer.HeldItem, 1);
                    }

                    // On click during alt
                    if (Main.mouseRight && Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        ModConfWeaponOutCustom.UpdateCustomHoldIncrement(Main.LocalPlayer.HeldItem, -1);
                    }
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
        public override void HandlePacket(BinaryReader reader, int whoAmI) {
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

        public static void NetUpdateDash(ModPlayerFists mpf) {
            if (Main.netMode == 1 && mpf.player.whoAmI == Main.myPlayer) {
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
        private void HandlePacketDash(BinaryReader reader, int code, int sender) {
            float dSpeed = reader.ReadSingle();
            float dThreshold = reader.ReadSingle();
            float dMax = reader.ReadSingle();
            float dMin = reader.ReadSingle();
            int dEffect = reader.ReadInt32();
            if (Main.netMode == 2) {
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
            else {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>();
                pfx.SetDash(dSpeed, dThreshold, dMax, dMin, true, dEffect);
            }
        }

        public static void NetUpdateParry(ModPlayerFists pfx) {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer) {
                //-/ Main.NewText("sent parry from " + Main.myPlayer);
                ModPacket message = pfx.mod.GetPacket();
                message.Write(2);
                message.Write(Main.myPlayer);
                message.Write(pfx.parryTimeMax);
                message.Write(pfx.parryWindow);
                message.Send();
            }
        }
        private void HandlePacketParry(BinaryReader reader, int code, int sender) {
            int parryTimeMax = reader.ReadInt32();
            int parryWindow = reader.ReadInt32();
            if (Main.netMode == 2) {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(parryTimeMax);
                me.Write(parryWindow);
                me.Send(-1, sender);
                //-/ Console.WriteLine("received parry from " + sender);
            }
            else {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>(this);
                pfx.AltFunctionParryMax(Main.player[sender], parryWindow, parryTimeMax);
                //-/ Main.NewText("received parry from " + sender);
            }
        }

        public static void NetUpdateCombo(ModPlayerFists pfx) {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer) {
                //-/ Main.NewText("sent combo " + pfx.ComboEffectAbs + " from " + Main.myPlayer);
                ModPacket message = pfx.mod.GetPacket();
                message.Write(4);
                message.Write(Main.myPlayer);
                message.Write(pfx.ComboEffectAbs);
                message.Send();
            }
        }
        private void HandlePacketCombo(BinaryReader reader, int code, int sender) {
            int comboEffect = reader.ReadInt32();
            if (Main.netMode == 2) {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(comboEffect);
                me.Send(-1, sender);
                //-/ Console.WriteLine("echo combo " + comboEffect + " from " + sender);
            }
            else {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>(this);
                pfx.player.itemAnimation = 0;
                pfx.AltFunctionCombo(Main.player[sender], comboEffect, true);
                //-/ Main.NewText("received combo " + comboEffect + " from " + sender);
            }
        }

        public static void NetUpdateWeaponVisual(Mod mod, PlayerWOFX pfx) {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer) {
                ModPacket message = mod.GetPacket();
                message.Write(3);
                message.Write(Main.myPlayer);
                message.Write(pfx.WeaponVisual);
                message.Send();
            }
        }
        private void HandlePacketWeaponVisual(BinaryReader reader, int code, int sender) {
            bool weaponVis = reader.ReadBoolean();
            if (Main.netMode == 2) {
                ModPacket me = GetPacket();
                me.Write(code);
                me.Write(sender);
                me.Write(weaponVis);
                me.Send(-1, sender);
            }
            else {
                PlayerWOFX pfx = Main.player[sender].GetModPlayer<PlayerWOFX>(this);
                pfx.weaponVisual = weaponVis;
            }
        }
        #endregion

        public override void PostUpdateInput()
        {
            if (Main.gameMenu || ControlToggleVisual == null || ModConf.ForceShowWeaponOut) return;
            if (ControlToggleVisual.JustPressed)
            {
                PlayerWOFX pfx = Main.LocalPlayer.GetModPlayer<PlayerWOFX>(this);
                ToggleWeaponVisual(pfx, !pfx.weaponVisual);
            }
        }

        internal static Vector2 CalculateNormalAngle(Vector2 start, Vector2 end)
        {
            Vector2 diff = end - start;
            diff.Normalize();
            return diff;
        }

        #region Mod Calls
        /// <summary>
        /// <para>"SetPlayerWeaponVisual", Player player, boolean show</para>
        /// <para>"SetFrenzyHeart", Player player, boolean state</para>
        /// <para>"AddCustomPreDrawMethod", Func(Player, Item, DrawData, returns bool show) customMethod</para>
        /// <para>"AddCustomHoldMethod", Func(Player, Item, int holdType, returns int holdType) customMethod</para>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object Call(params object[] args) {
            if (args.Length < 1) return null;
            if (args[0].GetType() != typeof(string)) return null;
            string method = (string)args[0];
            try
            {
                switch (method)
                {
                    case "SetPlayerWeaponVisual":
                        CallShowWeapon((Player)args[1], (bool)args[2]);
                        break;
                    case "SetFrenzyHeart":
                        SetFrenzyHeart((Player)args[1], (bool)args[2]);
                        break;

                    case "AddCustomPreDrawMethod":
                        AddCustomPreDrawMethod(args[1] as Func<Player, Item, DrawData, bool>);
                        break;
                    case "AddCustomHoldMethod":
                        AddCustomHoldMethod(args[1] as Func<Player, Item, int, int>);
                        break;
                }
            }
            catch { }
            return null;
        }

        private void CallShowWeapon(Player p, bool show) {
            ToggleWeaponVisual(p.GetModPlayer<PlayerWOFX>(this), show);
        }

        private void SetFrenzyHeart(Player p, bool state) {
            p.GetModPlayer<PlayerFX>().demonBlood = true;
        }

        /// <summary>
        /// Add a custom draw method to WeaponOut held item rendering. WeaponOut provides the following:
        /// <para>Player = The player the item is being drawn for. </para>
        /// <para>Item = The item to be drawn. </para>
        /// <para>DrawData = The calculated DrawData just before being added to Main.playerDrawData. </para>
        /// <para>Returns bool = True to allow drawing of the item for the player. </para>
        /// </summary>
        /// <param name="customDrawMethod"></param>
        public void AddCustomPreDrawMethod(Func<Player, Item, DrawData, bool> customDrawMethod)
        {
            weaponOutCustomPreDrawMethods.Add(customDrawMethod);
        }
        /// <summary>
        /// Use this to override the 'Auto' hold position for the item.
        /// [1: Hand]
        /// [2: Waist, drawn behind]
        /// [3: Back, drawn behind]
        /// [4: Spear]
        /// [5: PowerTool]
        /// [6: Bow]
        /// [7: SmallGun]
        /// [8: LargeGun]
        /// [9: Staff]
        /// </summary>
        /// <param name="customDrawMethod"></param>
        public void AddCustomHoldMethod(Func<Player, Item, int, int> customDrawMethod)
        {
            weaponOutCustomHoldMethods.Add(customDrawMethod);
        }

        private Func<Player, Item, DrawData, bool> WOPreDrawData = _HideCustomPreDrawMethod;
        private static bool _HideCustomPreDrawMethod(Player p, Item i, DrawData dd)
        {
            // Hide the Nebula Blaze
            if (i.type == ItemID.NebulaBlaze) return false;
            return true;
        }
        #endregion



    }
}
