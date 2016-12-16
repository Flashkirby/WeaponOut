using System;
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
        }

        public override void PostSetupContent()
        {
            BuffIDManaReduction = GetBuff("ManaReduction").Type;
            BuffIDMirrorBarrier = GetBuff("MirrorBarrier").Type;
            BuffIDWeaponSwitch = GetBuff("WeaponSwitch").Type;

            DustIDManaDust = GetDust("ManaDust").Type;

            Items.HeliosphereEmblem.SetUpGlobalDPS();

            if (Main.netMode != 2)
            {
                textureDMNB = GetTexture("Projectiles/DemonBlast");
                textureMANB = GetTexture("Projectiles/ManaBlast");
                textureMANBO = GetTexture("Projectiles/ManaBolt");
                textureSPSH = GetTexture("Projectiles/SplinterShot");

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
    }
}
