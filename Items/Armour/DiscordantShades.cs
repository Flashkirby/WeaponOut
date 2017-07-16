using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    /// <summary>
    /// Intercepts hook controls for discord teleporting when free
    /// </summary>
    [AutoloadEquip(EquipType.Head)]
    public class DiscordantShades : ModItem
    {
        private bool skipFrameAcc = false;
        public override bool Autoload(ref string name)
        {
            return ModConf.enableAccessories;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Discordant Shades");
            Tooltip.SetDefault(
                "Prioritise teleporting over grappling\n" +
                "Requires the Rod of Discord\n" +
                "Functions in the Head Vanity Slot\n" +
                "Can be equipped as an accessory\n" +
                "'The future's so bright, I gotta wear shades'");
        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 12;
            item.rare = 7;
            item.accessory = true;
            item.vanity = false;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofLight, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
        }

        public override void UpdateVanity(Player player, EquipType type)
        {
            useDiscordHookOverride(player, false);
        }
        public override void UpdateEquip(Player player)
        {
            if (skipFrameAcc)
            {
                useDiscordHookOverride(player, false);
            }
            useDiscordHookOverride(player, true);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (skipFrameAcc)
            {
                useDiscordHookOverride(player, false);
            }
            else
            {
                useDiscordHookOverride(player, true);
                player.releaseHook = false;
            }
        }

        private void useDiscordHookOverride(Player player, bool isAcc)
        {
            if (player.controlHook)
            {
                if (player.FindBuffIndex(BuffID.ChaosState) == -1)
                {
                    for (int i = 0; i < player.inventory.Length; i++)
                    {
                        if (player.inventory[i].type == ItemID.RodofDiscord)
                        {
                            //player has a rod
                            if (isAcc)
                            {
                                skipFrameAcc = true;
                                player.releaseHook = false;
                            }
                            else
                            {
                                skipFrameAcc = false;
                                DiscordantCharm.rodOfDiscord(player);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
