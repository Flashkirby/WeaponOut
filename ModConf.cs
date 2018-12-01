using System.Collections.Generic;
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

    public static class ModConfWeaponOutCustom
    {
        public const int configVersion = 0;

        /// <summary>
        /// Hold positions range from 1 and upwards. 0 represents default, aka nothing.
        /// </summary>
        static Dictionary<string, int> customHoldPositions = new Dictionary<string, int>();
        /// <summary>
        /// Pre-calculated array mapping itemIDs and their holdstyles.
        /// ItemIDs of vanilla items are type numbers.
        /// ItemIDs of modded items are their Namespace.Type
        /// </summary>
        static int[] customHoldStyleArray;

        static string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs/WeaponOut_Custom.json");

        static Preferences ModConfig = new Preferences(ConfigPath);
        internal static bool ModConfigLoaded = false;
        internal static bool ModConfigUpdated = false;

        internal static void Load()
        {
            customHoldStyleArray = new int[ItemLoader.ItemCount];

            customHoldPositions = new Dictionary<string, int>();
            ModConfigLoaded = ReadConfig();
        }

        public static bool ItemTypeHasCustomHoldStyle(int type)
        {
            return customHoldStyleArray[type] > 0;
        }
        public static bool TryGetCustomHoldStyle(int type, out int style)
        {
            style = customHoldStyleArray[type];
            return customHoldStyleArray[type] > 0;
        }

        /// <summary>
        /// Add a custom hold for the item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="style">Set to -1 to remove the custom item</param>
        public static void UpdateCustomHold(Item item, int style)
        {
            customHoldStyleArray[item.type] = style;
            if (item.modItem != null)
            { UpdateCustomHold(item.modItem.GetType().ToString(), style); }
            else
            { UpdateCustomHold(item.type.ToString(), style); }
        }
        public static void UpdateCustomHoldIncrement(Item item, int amount)
        {
            int currentStyle;
            if (item.modItem != null)
            { customHoldPositions.TryGetValue(item.modItem.GetType().ToString(), out currentStyle); }
            else
            { customHoldPositions.TryGetValue(item.type.ToString(), out currentStyle); }
            currentStyle += amount;
            // this is tightly coupled, but can easily be set in a constructor if necessary
            if (currentStyle > PlayerWOFX.HoldTypeCount) currentStyle = 0;
            if (currentStyle < 0) currentStyle = PlayerWOFX.HoldTypeCount;
            UpdateCustomHold(item, currentStyle);
        }
        private static void UpdateCustomHold(string itemID, int style)
        {
            int currentStyle;
            customHoldPositions.TryGetValue(itemID, out currentStyle);
            if (style > 0)
            {
                if (customHoldPositions.ContainsKey(itemID))
                { customHoldPositions[itemID] = style; }
                else
                { customHoldPositions.Add(itemID, style); }
                
            }
            else
            {
                customHoldPositions.Remove(itemID);
            }
            if (style != currentStyle) ModConfigUpdated = true;
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
                    }
                    if (!canUpdate) return false;
                }

                List<string> allKeys = ModConfig.GetAllKeys();
                foreach (string key in allKeys)
                {
                    customHoldPositions.Add(key, ModConfig.Get(key, -1));
                }
                
                for(int i = 0; i < ItemLoader.ItemCount; i++)
                {
                    ModItem item = ItemLoader.GetItem(i);
                    if (item != null)
                    {
                        string typeString = item.GetType().ToString();
                        if (customHoldPositions.ContainsKey(typeString))
                        { customHoldStyleArray[i] = customHoldPositions[typeString]; }
                    }
                    else
                    {
                        if (customHoldPositions.ContainsKey(i.ToString()))
                        { customHoldStyleArray[i] = customHoldPositions[i.ToString()]; }
                    }
                }

                return true;
            }
            return false;
        }

        internal static void SaveConfig()
        {
            if (ModConfigUpdated)
            {
                ModConfig.Clear();
                ModConfig.Put("version", configVersion);
                foreach (KeyValuePair<string, int> pair in customHoldPositions)
                {
                    ModConfig.Put(pair.Key, pair.Value);
                }
                ModConfig.Save();
                ModConfigUpdated = false;
            }
        }
    }
}
