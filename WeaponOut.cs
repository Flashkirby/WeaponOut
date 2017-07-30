using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

        public static Texture2D textureSPSH;
        public static Texture2D textureDMNB;
        public static Texture2D textureMANB;
        public static Texture2D textureMANBO;
        public static Texture2D textureSCSH;

        public static int BuffIDManaReduction;
        public static int BuffIDMirrorBarrier;

        public static int DustIDManaDust;

        public static int shakeIntensity = 0;
        private static int shakeTick = 0;

        public WeaponOut()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                //AutoloadSounds = true
            };
            ModConf.Load();
        }

        public override void Load()
        {
            mod = this;
        }

        public override void PostSetupContent()
        {
            if (ModConf.enableAccessories) BuffIDMirrorBarrier = GetBuff("MirrorBarrier").Type;
            if (ModConf.enableDualWeapons) BuffIDManaReduction = GetBuff("ManaReduction").Type;

            DustIDManaDust = GetDust<Dusts.ManaDust>().Type;

            if (ModConf.enableEmblems) Items.Accessories.HeliosphereEmblem.SetUpGlobalDPS();

            if (Main.netMode != 2)
            {
                textureDMNB = GetTexture("Projectiles/DemonBlast");
                textureMANB = GetTexture("Projectiles/ManaBlast");
                textureMANBO = GetTexture("Projectiles/ManaBolt");
                textureSPSH = GetTexture("Projectiles/SplinterShot");
                textureSCSH = GetTexture("Projectiles/ScatterShot");

                Projectiles.Explosion.textureTargetS = GetTexture("Projectiles/Explosion_Targetsm");
                Projectiles.Explosion.textureTargetM = GetTexture("Projectiles/Explosion_Targetmd");
                Projectiles.Explosion.textureTargetL = GetTexture("Projectiles/Explosion_Targetlg");
                Projectiles.Explosion.textureTargetXL = GetTexture("Projectiles/Explosion_Targetxl");
                Projectiles.Explosion.textureLaser = GetTexture("Projectiles/Explosion_Laser");
            }
            else
            {
                Console.WriteLine("WeaponOut loaded with no errors:   visuals&fists#01");
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
        public override Microsoft.Xna.Framework.Matrix ModifyTransformMatrix(Microsoft.Xna.Framework.Matrix Transform)
        {
            if (!Main.gameMenu)
            {
				if(!Main.gamePaused)
				{
					shakeTick++;
					if (shakeIntensity >= 0 && shakeTick >= 4) shakeIntensity--;
					if (shakeIntensity > 10) shakeIntensity = 10;//cap it
					if (shakeIntensity < 0) shakeIntensity = 0;
					return Transform
						* Microsoft.Xna.Framework.Matrix.CreateTranslation(
						Main.rand.Next((int)(shakeIntensity * -0.5f), (int)(shakeIntensity * 0.5f + 1)),
						Main.rand.Next((int)(shakeIntensity * -0.5f), (int)(shakeIntensity * 0.5f + 1)),
						0f);
				}
            }
            else
            {
                shakeIntensity = 0;
                shakeTick = 0;
            }
            return Transform;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            // Janky quick inventory visibilty
            if (Main.playerInventory && ModConf.showWeaponOut && !ModConf.forceShowWeaponOut)
            {
                //Get vars
                PlayerFX pfx = Main.LocalPlayer.GetModPlayer<PlayerFX>(this);
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
                        Main.PlaySound(SoundID.MenuTick);
                        pfx.weaponVisual = !pfx.weaponVisual;

                        NetUpdateWeaponVisual(this, pfx);
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
        }

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
                bool weaponVis = reader.ReadBoolean();

                if (Main.netMode == 2)
                {
                    for (int i = 0; i < 255; i++)
                    {
                        if (!Main.player[i].active) continue;
                        if (i != sender)
                        {
                            ModPacket me = GetPacket();
                            me.Write(code);
                            me.Write(sender);
                            me.Write(weaponVis);
                            me.Send();
                        }
                    }
                    // Console.WriteLine("Set player " + Main.player[sender].name + " weapon dash to " + dash);
                }
                else
                {
                    PlayerFX pfx = Main.player[sender].GetModPlayer<PlayerFX>(this);
                    pfx.weaponVisual = weaponVis;
                    // Main.NewText("Set player " + Main.player[sender].name + " weapon dash to " + dash);
                }
            }
            #endregion
        }

        public static void NetUpdateDash(ModPlayerFists pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
                ModPacket message = pfx.mod.GetPacket();
                message.Write(1);
                message.Write(Main.myPlayer);
                message.Write(pfx.dashSpeed);
                message.Write(pfx.dashMaxSpeedThreshold);
                message.Write(pfx.dashMaxFriction);
                message.Write(pfx.dashMinFriction);
                message.Send();
            }
        }
        private void HandlePacketDash(BinaryReader reader, int code, int sender)
        {
            int dSpeed = reader.ReadInt32();
            int dThreshold = reader.ReadInt32();
            int dMax = reader.ReadInt32();
            int dMin = reader.ReadInt32();
            if (Main.netMode == 2)
            {
                for (int i = 0; i < 255; i++)
                {
                    if (!Main.player[i].active) continue;
                    if (i != sender)
                    {
                        ModPacket me = GetPacket();
                        me.Write(code);
                        me.Write(sender);
                        me.Write(dSpeed);
                        me.Write(dThreshold);
                        me.Write(dMax);
                        me.Write(dMin);
                        me.Send();
                    }
                }
                // Console.WriteLine("Set player " + Main.player[sender].name + " weapon dash to " + dash);
            }
            else
            {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>();
                pfx.player.dashDelay = 0;
                pfx.SetDash(dSpeed, dThreshold, dMax, dMin);
                // Main.NewText("Set player " + Main.player[sender].name + " weapon dash to " + dash);
            }
        }

        public static void NetUpdateParry(ModPlayerFists pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
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
                for (int i = 0; i < 255; i++)
                {
                    if (!Main.player[i].active) continue;
                    if (i != sender)
                    {
                        ModPacket me = GetPacket();
                        me.Write(code);
                        me.Write(sender);
                        me.Write(parryTimeMax);
                        me.Write(parryWindow);
                        me.Send();
                    }
                }
                // Console.WriteLine("Set player " + Main.player[sender].name + " weapon dash to " + dash);
            }
            else
            {
                ModPlayerFists pfx = Main.player[sender].GetModPlayer<ModPlayerFists>(this);
                pfx.AltFunctionParryMax(Main.player[sender], parryWindow, parryTimeMax);
                // Main.NewText("Set player " + Main.player[sender].name + " weapon dash to " + dash);
            }
        }

        public static void NetUpdateWeaponVisual(Mod mod, PlayerFX pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
                ModPacket message = mod.GetPacket();
                message.Write(3);
                message.Write(Main.myPlayer);
                message.Write(pfx.weaponVisual);
                message.Send();
            }
        }
    }
}
