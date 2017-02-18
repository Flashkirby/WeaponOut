using System;
using System.IO;
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
        public static Texture2D textureSPSH;
        public static Texture2D textureDMNB;
        public static Texture2D textureMANB;
        public static Texture2D textureMANBO;
        public static Texture2D textureSCSH;

        public static int BuffIDManaReduction;
        public static int BuffIDMirrorBarrier;
        public static int BuffIDWeaponSwitch;

        public static int DustIDManaDust;

        public static int shakeIntensity = 0;
        private static int shakeTick = 0;

        public WeaponOut()
        {
            Properties = new ModProperties()
            {
                Autoload = true
                //AutoloadGores = true,
                //AutoloadSounds = true
            };
            Items.Weapons.HelperDual.ResetListOnLoad();
        }

        public override void Load()
        {
            ModConf.Load();
        }

        public override void PostSetupContent()
        {

            if (ModConf.enableAccessories) BuffIDMirrorBarrier = GetBuff("MirrorBarrier").Type;
            if (ModConf.enableDualWeapons) BuffIDManaReduction = GetBuff("ManaReduction").Type;
            if (ModConf.enableDualWeapons) BuffIDWeaponSwitch = GetBuff("WeaponSwitch").Type;

            DustIDManaDust = GetDust("ManaDust").Type;

            if (ModConf.enableEmblems) Items.HeliosphereEmblem.SetUpGlobalDPS();

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
                Console.WriteLine("WeaponOut loaded with no errors:   supercharge#01");
            }
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

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int code = reader.ReadInt32();
            int sender = reader.ReadInt32();
            if (code == 1) // Set dash used
            {
                int dash = reader.ReadInt32();
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
                            me.Write(dash);
                            me.Send();
                        }
                    }
                    // Console.WriteLine("Set player " + Main.player[sender].name + " weapon dash to " + dash);
                }
                else
                {
                    Main.player[sender].GetModPlayer<PlayerFX>(this).weaponDash = dash;
                    // Main.NewText("Set player " + Main.player[sender].name + " weapon dash to " + dash);
                }
            }

            if (code == 2) // Set parry move
            {
                int parryTime = reader.ReadInt32();
                int parryActive = reader.ReadInt32();
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
                            me.Write(parryTime);
                            me.Write(parryActive);
                            me.Send();
                        }
                    }
                    // Console.WriteLine("Set player " + Main.player[sender].name + " weapon dash to " + dash);
                }
                else
                {
                    PlayerFX pfx = Main.player[sender].GetModPlayer<PlayerFX>(this);
                    pfx.parryTime = parryTime;
                    pfx.parryTimeMax = parryTime;
                    pfx.parryActive = parryActive;
                    // Main.NewText("Set player " + Main.player[sender].name + " weapon dash to " + dash);
                }
            }
        }

        public static void NetUpdateParry(Mod mod, PlayerFX pfx)
        {
            if (Main.netMode == 1 && pfx.player.whoAmI == Main.myPlayer)
            {
                ModPacket message = mod.GetPacket();
                message.Write(2);
                message.Write(Main.myPlayer);
                message.Write(pfx.parryTimeMax);
                message.Write(pfx.parryActive);
                message.Send();
            }
        }
    }
}
