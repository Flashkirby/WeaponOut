using System;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace WeaponOut
{
    /// <summary>
    /// Tutorial by goldenapple: https://forums.terraria.org/index.php?threads/modders-guide-to-config-files-and-optional-features.48581/
    /// </summary>
    static class ModCfg
    {
        public const int configVersion = -1;
        public static bool showWeaponOut = true;
        public const string showWeaponOutField = "show_weaponOut";
        public static bool enableWhips = true;
        public const string enableWhipsField = "enable_whips";

        static string ConfigPath = Path.Combine(Main.SavePath, "WeaponOut.json");

        static Preferences ModConfig = new Preferences(ConfigPath);

        public static void Load()
        {
            bool success = ReadConfig();
            if (!success)
            {
                ErrorLogger.Log("WeaponOut: Couldn't load config file, creating new file. ");
                CreateConfig();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> true is loaded successfully </returns>
        static bool ReadConfig()
        {
            if (ModConfig.Load())
            {
                int readVersion = 0;
                ModConfig.Get("version", ref readVersion);
                if (readVersion != configVersion) return false;

                ModConfig.Get(showWeaponOutField, ref showWeaponOut);
                ModConfig.Get(enableWhipsField, ref enableWhips);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create a new config file for the player to edit. 
        /// </summary>
        static void CreateConfig()
        {
            ModConfig.Clear();
            ModConfig.Put("version", configVersion);
            ModConfig.Put(showWeaponOutField, showWeaponOut);
            ModConfig.Put(enableWhipsField, enableWhips);
            ModConfig.Save();
        }
    }
}
