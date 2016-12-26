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
    public class DiscordantCharm : ModItem
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
            item.name = "Discordant Charm";
            item.toolTip = @"Prioritise teleporting over grappling
Requires the Rod of Discord
Functions in the Head Vanity Slot
Can be equipped as an accessory";
            item.width = 28;
            item.height = 28;
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
                                rodOfDiscord(player);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void rodOfDiscord(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Vector2 vector32;
                vector32.X = (float)Main.mouseX + Main.screenPosition.X;
                if (player.gravDir == 1f)
                {
                    vector32.Y = (float)Main.mouseY + Main.screenPosition.Y - (float)player.height;
                }
                else
                {
                    vector32.Y = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;
                }
                vector32.X -= (float)(player.width / 2);
                if (vector32.X > 50f && vector32.X < (float)(Main.maxTilesX * 16 - 50) && vector32.Y > 50f && vector32.Y < (float)(Main.maxTilesY * 16 - 50))
                {
                    int num246 = (int)(vector32.X / 16f);
                    int num247 = (int)(vector32.Y / 16f);
                    if ((Main.tile[num246, num247].wall != 87 || (double)num247 <= Main.worldSurface || NPC.downedPlantBoss) && !Collision.SolidCollision(vector32, player.width, player.height))
                    {
                        player.Teleport(vector32, 1, 0);
                        NetMessage.SendData(65, -1, -1, "", 0, (float)player.whoAmI, vector32.X, vector32.Y, 1, 0, 0);
                        if (player.chaosState)
                        {
                            player.statLife -= player.statLifeMax2 / 7;
                            if (Lang.lang <= 1)
                            {
                                PlayerDeathReason damageSource = PlayerDeathReason.ByOther(13);
                                if (Main.rand.Next(2) == 0)
                                {
                                    damageSource = PlayerDeathReason.ByOther(player.Male ? 14 : 15);
                                }
                                if (player.statLife <= 0)
                                {
                                    player.KillMe(damageSource, 1.0, 0, false);
                                }
                            }
                            else if (player.statLife <= 0)
                            {
                                player.KillMe(PlayerDeathReason.LegacyEmpty(), 1.0, 0, false);
                            }
                            player.lifeRegenCount = 0;
                            player.lifeRegenTime = 0;
                        }
                        player.AddBuff(88, 360, true);
                    }
                }
            }
        }
    }
}
