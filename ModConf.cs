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
    static class ModConf
    {
        public const int configVersion = 1;
        public static bool showWeaponOut = true;
        public const string showWeaponOutField = "show_weaponOut_visuals";

        public static bool enableBasicContent = true;
        public const string enableBasicContentField = "enable_base_weapons_and_tiles";

        public static bool enableWhips = true;
        public const string enableWhipsField = "enable_whips";

        public static bool enableFists = true;
        public const string enableFistsField = "enable_fists";

        public static bool enableDualWeapons = true;
        public const string enableDualWeaponsField = "enable_dual_weapons";

        public static bool enableAccessories = true;
        public const string enableAccessoriesField = "enable_accessories";

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
                ModConfig.Get(enableBasicContentField, ref enableBasicContent);
                ModConfig.Get(enableWhipsField, ref enableWhips);
                ModConfig.Get(enableFistsField, ref enableFists);
                ModConfig.Get(enableDualWeaponsField, ref enableDualWeapons);
                ModConfig.Get(enableAccessoriesField, ref enableAccessories);

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
            ModConfig.Put(enableBasicContentField, enableBasicContent);
            ModConfig.Put(enableWhipsField, enableWhips);
            ModConfig.Put(enableFistsField, enableFists);
            ModConfig.Put(enableDualWeaponsField, enableDualWeapons);
            ModConfig.Put(enableAccessoriesField, enableAccessories);

            ModConfig.Put("readme", "WARNING: Clients will desync if their local config is different to the server - this cannot be fixed without forcing the clients to download the server's mods and forcing the mods to reload. So don't mess with this too much outside of singleplayer unless you know what you're doing. And no I'm too lazy to find out how to even fix this behaviour. Feel free to delete this.");

            ModConfig.Save();
        }
    }
}
