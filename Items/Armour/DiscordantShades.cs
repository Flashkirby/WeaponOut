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
    public class DiscordantShades : ModItem
    {
        private bool skipFrameAcc = false;
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            if (ModConf.enableAccessories)
            {
                equips.Add(EquipType.Head);
                return true;
            }
            return false;
        }

        public override void SetDefaults()
        {
            item.name = "Discordant Shades";
            item.toolTip = @"Prioritise teleporting over grappling
Requires the Rod of Discord
Functions in the Head Vanity Slot
Can be equipped as an accessory";
            item.toolTip2 = "'The future's so bright, I gotta wear shades'";
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
            useDiscordHookOverride(player, true);
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
