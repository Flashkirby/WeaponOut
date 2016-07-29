using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
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
            equips.Add(EquipType.Head);
            return true;
        }

        public override void SetDefaults()
        {
            item.name = "Discordant Charm";
            item.toolTip = @"Teleports you instead of grappling
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
                if (player.HasBuff(BuffID.ChaosState) == -1)
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
                Vector2 vector22;
                vector22.X = (float)Main.mouseX + Main.screenPosition.X;
                if (player.gravDir == 1f)
                {
                    vector22.Y = (float)Main.mouseY + Main.screenPosition.Y - (float)player.height;
                }
                else
                {
                    vector22.Y = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;
                }
                vector22.X -= (float)(player.width / 2);
                if (vector22.X > 50f && vector22.X < (float)(Main.maxTilesX * 16 - 50) && vector22.Y > 50f && vector22.Y < (float)(Main.maxTilesY * 16 - 50))
                {
                    int num246 = (int)(vector22.X / 16f);
                    int num247 = (int)(vector22.Y / 16f);
                    if ((Main.tile[num246, num247].wall != 87 || (double)num247 <= Main.worldSurface || NPC.downedPlantBoss) && !Collision.SolidCollision(vector22, player.width, player.height))
                    {
                        player.Teleport(vector22, 1, 0);
                        NetMessage.SendData(65, -1, -1, "", 0, (float)player.whoAmI, vector22.X, vector22.Y, 1, 0, 0);
                        if (player.chaosState)
                        {
                            player.statLife -= player.statLifeMax2 / 7;
                            if (Lang.lang <= 1)
                            {
                                string deathText = " didn't materialize";
                                if (Main.rand.Next(2) == 0)
                                {
                                    if (player.Male)
                                    {
                                        deathText = "'s legs appeared where his head should be";
                                    }
                                    else
                                    {
                                        deathText = "'s legs appeared where her head should be";
                                    }
                                }
                                if (player.statLife <= 0)
                                {
                                    player.KillMe(1.0, 0, false, deathText);
                                }
                            }
                            else if (player.statLife <= 0)
                            {
                                player.KillMe(1.0, 0, false, "");
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
