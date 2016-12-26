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
        public static bool showWeaponOut = false;
        public const string showWeaponOutField = "show_weaponOut";

        // Terraria/ModLoader/Mod Configs/
        static string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs", "WeaponOut.json");

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
                ModConfig.Get(showWeaponOutField, ref showWeaponOut);
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
            ModConfig.Put(showWeaponOutField, showWeaponOut);
            ModConfig.Save();
        }
    }
}
