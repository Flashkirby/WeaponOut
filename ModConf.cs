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
    public static class ModConf
    {
        public const int configVersion = 2;

        internal static bool showWeaponOut = true;
        public static bool ShowWeaponOut { get { return showWeaponOut; } }
        private const string showWeaponOutField = "show_weaponOut_visuals";

        internal static bool forceShowWeaponOut = false;
        public static bool ForceShowWeaponOut { get { return forceShowWeaponOut; } }
        private const string forceShowWeaponOutField = "forceshow_weaponOut_visuals";

        internal static bool toggleWaistRotation = false;
        public static bool ToggleWaistRotation { get { return toggleWaistRotation; } }
        private const string toggleWaistRotationField = "toggle_weaponOut_waist_rotation";

        internal static bool enableBasicContent = true;
        public static bool EnableBasicContent { get { return enableBasicContent; } }
        private const string enableBasicContentField = "enable_base_weapons_and_tiles";

        internal static bool enableWhips = true;
        public static bool EnableWhips { get { return enableWhips; } }
        private const string enableWhipsField = "enable_whips";

        internal static bool enableFists = true;
        public static bool EnableFists { get { return enableFists; } }
        private const string enableFistsField = "enable_fists";

        internal static bool enableDualWeapons = true;
        public static bool EnableDualWeapons { get { return enableDualWeapons; } }
        private const string enableDualWeaponsField = "enable_dual_weapons";

        internal static bool enableAccessories = true;
        public static bool EnableAccessories { get { return enableAccessories; } }
        private const string enableAccessoriesField = "enable_accessories";

        internal static bool enableEmblems = true;
        public static bool EnableEmblems { get { return enableEmblems; } }
        private const string enableEmblemsField = "enable_emblems";

        static string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs/WeaponOut.json");

        static Preferences ModConfig = new Preferences(ConfigPath);

        internal static void Load()
        {
            bool success = ReadConfig();
            if (!success)
            {
                ErrorLogger.Log("WeaponOut: Couldn't load config file, creating new file. ");
                CreateConfig();
            }
        }
        
        /// <returns> true is loaded successfully </returns>
        internal static bool ReadConfig()
        {
            if (ModConfig.Load())
            {
                int readVersion = 0;
                ModConfig.Get("version", ref readVersion);
                if (readVersion != configVersion)
                {
                    bool canUpdate = false;
                    if (readVersion == 0)
                    {
                        canUpdate = true;
                        ModConfig.Put("version", 1);
                        ModConfig.Put(enableEmblemsField, enableEmblems);
                        ModConfig.Save();
                    }
                    if (readVersion == 1)
                    {
                        canUpdate = true;
                        ModConfig.Put("version", 2);
                        ModConfig.Put(toggleWaistRotationField, toggleWaistRotation);
                        ModConfig.Save();
                    }

                    if (!canUpdate) return false;
                }

                ModConfig.Get(showWeaponOutField, ref showWeaponOut);
                ModConfig.Get(forceShowWeaponOutField, ref forceShowWeaponOut);
                ModConfig.Get(toggleWaistRotationField, ref toggleWaistRotation);
                ModConfig.Get(enableBasicContentField, ref enableBasicContent);
                ModConfig.Get(enableWhipsField, ref enableWhips);
                ModConfig.Get(enableFistsField, ref enableFists);
                ModConfig.Get(enableDualWeaponsField, ref enableDualWeapons);
                ModConfig.Get(enableAccessoriesField, ref enableAccessories);
                ModConfig.Get(enableEmblemsField, ref enableEmblems);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Create a new config file for the player to edit. 
        /// </summary>
        internal static void CreateConfig()
        {
            ModConfig.Clear();
            ModConfig.Put("version", configVersion);

            ModConfig.Put(showWeaponOutField, showWeaponOut);
            ModConfig.Put(forceShowWeaponOutField, forceShowWeaponOut);
            ModConfig.Put(toggleWaistRotationField, toggleWaistRotation);
            ModConfig.Put(enableBasicContentField, enableBasicContent);
            ModConfig.Put(enableWhipsField, enableWhips);
            ModConfig.Put(enableFistsField, enableFists);
            ModConfig.Put(enableDualWeaponsField, enableDualWeapons);
            ModConfig.Put(enableAccessoriesField, enableAccessories);
            ModConfig.Put(enableEmblemsField, enableEmblems);

            ModConfig.Put("readme", "First off, make sure to reload before the configs will take any effect. Most of the fields do exactly as they say, they will allow the mod to load, or choose not to, sets of content from the mod. The only field that does not do this is forceshow_weaponOut_visuals, which simply forces the weapon to always show regardless of the visibility of the first accessory slot as this is an oft requested feature. WARNING: Clients will desync if their local config is different to the server - this cannot be fixed without forcing the clients to download the server's mods and forcing the mods to reload. So don't mess with this too much outside of singleplayer unless you know what you're doing. And no I'm too lazy to find out how to even fix this behaviour, though a simple server mismatch warning might be a good idea. Feel free to delete this.");

            ModConfig.Save();
        }
    }
}
