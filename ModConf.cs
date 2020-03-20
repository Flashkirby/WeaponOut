using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace WeaponOut
{
    /// <summary>
    /// ExampleMod used as a reference
    /// </summary>
    public class ClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // For future-Proofing
        // It does not *have* to follow the version number from the old config
        // Since it is a new file
        [ReloadRequired]
        [DefaultValue(1)]
        public int Version;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool ShowWeaponOut;

        [ReloadRequired]
        [DefaultValue(false)]
        public bool ForceShowWeaponOut;

        [ReloadRequired]
        [DefaultValue(false)]
        public bool ToggleWaistRotation;

        public override void OnLoaded()
        {
            // Give ModConf a copy of this config
            // If a player chooses to change settings, they can do so safely
            // A eeload will be required to apply player changes
            ModConf.clientConfig = (ClientConfig)this.Clone();
            ModConf.createClientConfig();
        }
    }


    /// <summary>
    /// ExampleMod used as a reference
    /// </summary>
    public class ServerConfig : ModConfig
    {
        // Any settings that would require syncing between the server and client (i.e. recipes) go here
        // This config will be synced with the client on connection, (they will reload with the server config if their config differs)
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // For future-Proofing
        // It does not have to follow the version number from the old config
        // Since it is a new file
        [ReloadRequired]
        [DefaultValue(1)]
        public int Version;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableBaseContent;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableWhips;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableFists;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableDualWeapons;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableAccessories;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableEmblems;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool EnableSabres;

        public override void OnLoaded()
        {
            // Give ModConf a copy of this config
            // If a player chooses to change settings, they can do so safely
            // A eeload will be required to apply player changes
            ModConf.serverConfig = (ServerConfig)this.Clone();
            ModConf.createServerConfig();
        }

    }

    /// <summary>
    /// Tutorial by goldenapple: https://forums.terraria.org/index.php?threads/modders-guide-to-config-files-and-optional-features.48581/
    /// </summary>
    public static class ModConf
    {
        internal static ClientConfig clientConfig = null;
        internal static ServerConfig serverConfig = null;

        public const int configVersion = 3;

        public static bool ShowWeaponOut { get { return clientConfig.ShowWeaponOut; } }
        public static bool ForceShowWeaponOut { get { return clientConfig.ForceShowWeaponOut; } }
        public static bool ToggleWaistRotation { get { return clientConfig.ToggleWaistRotation; } }
        public static bool EnableBasicContent { get { return serverConfig.EnableBaseContent; } }
        public static bool EnableWhips { get { return serverConfig.EnableWhips; } }
        public static bool EnableFists { get { return serverConfig.EnableFists; } }
        public static bool EnableDualWeapons { get { return serverConfig.EnableDualWeapons; } }
        public static bool EnableAccessories { get { return serverConfig.EnableAccessories; } }
        public static bool EnableEmblems { get { return serverConfig.EnableEmblems; } }
        public static bool EnableSabres { get { return serverConfig.EnableSabres; } }

        internal static string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs");


        // tModloader names the config like so: <Mod Name>_<Config Name>.json
        // These could be simplified to WeaponOut_ClientConfig.json and WeaponOut_ServerConfig.json
        // But if the internal config names get changed, we'll be manually saving to the wrong place
        internal readonly static string clientPath = Path.Combine(ConfigManager.ModConfigPath, nameof(WeaponOut) + "_" + nameof(ClientConfig) + ".json");
        internal readonly static string serverPath = Path.Combine(ConfigManager.ModConfigPath, nameof(WeaponOut) + "_" + nameof(ServerConfig) + ".json");
        internal readonly static string legacyPath = Path.Combine(ConfigManager.ModConfigPath, "WeaponOut.json");

        /// <summary>
        /// Tmodloader only saves differing settings
        /// This will manually create a complete client config
        /// </summary>
        internal static void createClientConfig()
        {
            //Similar to above, using nameof() to ensure the same name tModloader uses
            Preferences clientPrefs = new Preferences(clientPath);
            clientPrefs.Put(nameof(ClientConfig.ShowWeaponOut), clientConfig.ShowWeaponOut);
            clientPrefs.Put(nameof(ClientConfig.ForceShowWeaponOut), clientConfig.ForceShowWeaponOut);
            clientPrefs.Put(nameof(ClientConfig.ToggleWaistRotation), clientConfig.ToggleWaistRotation);
            clientPrefs.Save();
        }

        /// <summary>
        /// Tmodloader only saves differing settings
        /// This will manually create a complete server config
        /// </summary>
        internal static void createServerConfig()
        {
            //Similar to above, using nameof() to ensure the same name tModloader uses
            Preferences serverPrefs = new Preferences(serverPath);
            serverPrefs.Put(nameof(ServerConfig.EnableBaseContent), serverConfig.EnableBaseContent);
            serverPrefs.Put(nameof(ServerConfig.EnableWhips), serverConfig.EnableWhips);
            serverPrefs.Put(nameof(ServerConfig.EnableFists), serverConfig.EnableFists);
            serverPrefs.Put(nameof(ServerConfig.EnableSabres), serverConfig.EnableSabres);
            serverPrefs.Put(nameof(ServerConfig.EnableDualWeapons), serverConfig.EnableDualWeapons);
            serverPrefs.Put(nameof(ServerConfig.EnableAccessories), serverConfig.EnableAccessories);
            serverPrefs.Put(nameof(ServerConfig.EnableEmblems), serverConfig.EnableEmblems);
            serverPrefs.Save();
        }


        /// <summary>
        /// This will manually create a complete server config
        /// </summary>
        internal static void Load()
        {
            // If an old config exists, try converting it
            if (File.Exists(legacyPath))
            {

                //ConvertLegacyConfig();

            };
            // check if ocnfig have already been loaded


            //clientConfig = GetInstance<ClientConfig>();
            //serverConfig = GetInstance<ServerConfig>();
            //bool success = ReadConfig();
            //if (!success)
            //{
            //    ErrorLogger.Log("WeaponOut: Couldn't load config file, creating new file. ");
            //    CreateConfig();
            //}
        }


        /// <returns> 
        /// true means succesful conversion, reload required 
        /// false means failed conversion
        ///     something went wrong and we're just gonnna ignore the old config,
        ///     no reload required
        /// </returns>
        /// TODO: This currently does not work
        internal static bool ConvertLegacyConfig()
        {
            Preferences legacyPrefs = new Preferences(legacyPath);
            if (legacyPrefs.Load())
            {
                int readVersion = 0;
                legacyPrefs.Get("version", ref readVersion);
                if (readVersion != configVersion)
                {
                    // Get the old prefs

                    // Version 0 Setings
                    legacyPrefs.Get("show_weaponOut_visuals", ref clientConfig.ShowWeaponOut);
                    legacyPrefs.Get("forceshow_weaponOut_visuals", ref clientConfig.ForceShowWeaponOut);
                    legacyPrefs.Get("enable_base_weapons_and_tiles", ref serverConfig.EnableBaseContent);
                    legacyPrefs.Get("enable_whips", ref serverConfig.EnableWhips);
                    legacyPrefs.Get("enable_fists", ref serverConfig.EnableFists);
                    legacyPrefs.Get("enable_dual_weapons", ref serverConfig.EnableDualWeapons);
                    legacyPrefs.Get("enable_accessories", ref serverConfig.EnableAccessories);

                    if (readVersion >= 1)
                    {
                        legacyPrefs.Get("enable_emblems", ref serverConfig.EnableEmblems);
                    }
                    if (readVersion >= 2)
                    {
                        legacyPrefs.Get("toggle_weaponOut_waist_rotation", ref clientConfig.ToggleWaistRotation);
                    }
                    if (readVersion >= 3)
                    {
                        legacyPrefs.Get("enable_sabres", ref serverConfig.EnableSabres);
                    }
                }
                return true;
            }
            return false;
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

                for (int i = 0; i < ItemLoader.ItemCount; i++)
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

