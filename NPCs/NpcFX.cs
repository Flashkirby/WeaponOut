using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.NPCs
{
    public class NpcFX : GlobalNPC
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableWhips;
        }

        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (ModConf.enableWhips)
            {
                if (type == NPCID.Wizard)
                {
                    //add puzzling cutter if hardmode
                    if (Main.hardMode)
                    {
                        shop.item[nextSlot].SetDefaults(mod.ItemType<Items.Weapons.Whips.PuzzlingCutter>());
                        nextSlot++;
                    }
                }
            }
            if (ModConf.enableBasicContent)
            {
                if (type == NPCID.ArmsDealer)
                {
                    //add scrap salvo after mech
                    if (Main.hardMode && NPC.downedPlantBoss)
                    {
                        shop.item[nextSlot].SetDefaults(mod.ItemType<Items.Weapons.Basic.ScrapSalvo>());
                        nextSlot++;
                    }
                }
            }
            if (ModConf.enableFists)
            {
                if (type == NPCID.Clothier)
                {
                    //add headbands after bossess
                    if (Main.hardMode)
                    {
                        if(NPC.downedBoss3)
                        {
                            shop.item[nextSlot].SetDefaults(mod.ItemType<Items.Armour.FistVeteranHead>());
                            nextSlot++;
                        }
                        if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                        {
                            shop.item[nextSlot].SetDefaults(mod.ItemType<Items.Armour.FistMasterHead>());
                            nextSlot++;
                        }
                    }
                }
            }
        }

        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
            if (ModConf.enableFists)
            {
                int chance = 5;
                if (NPC.downedSlimeKing) chance--;
                if (NPC.downedBoss1) chance--;
                int taekwonBody = mod.ItemType<Items.Armour.FistPowerBody>();
                int boxingBody = mod.ItemType<Items.Armour.FistSpeedBody>();
                foreach (Player p in Main.player)
                {
                    if (p.armor[1].type == taekwonBody || p.armor[1].type == boxingBody)
                    {
                        chance -= 2;
                    }
                }
                if (Main.rand.Next(Math.Max(1,chance)) == 0)
                {
                    shop[nextSlot] = mod.ItemType<Items.Armour.FistDefBody>(); nextSlot++;
                    shop[nextSlot] = mod.ItemType<Items.Armour.FistDefLegs>(); nextSlot++;
                }
            }
        }

        public override void NPCLoot(NPC npc)
        {
            if (ModConf.enableFists) {
                if (npc.boss)
                {
                    int itemType = -1;
                    if (Main.expertMode)
                    {
                        if (npc.type == NPCID.EyeofCthulhu)
                        {
                            itemType = mod.ItemType<Items.Accessories.RushCharm>();
                        }
                        if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
                        {
                            itemType = mod.ItemType<Items.Accessories.DriedEye>();
                        }
                        if (npc.type == NPCID.BrainofCthulhu)
                        {
                            itemType = mod.ItemType<Items.Accessories.StainedTooth>();
                        }
                    }
                    bool chance = Main.rand.Next(3) == 0 || Main.expertMode;
                    if (chance)
                    {
                        if (npc.type == NPCID.Plantera)
                        {
                            itemType = mod.ItemType<Items.Weapons.Fists.KnucklesPlantera>();
                        }
                        if (npc.type == NPCID.DukeFishron)
                        {
                            itemType = mod.ItemType<Items.Weapons.Fists.KnucklesDuke>();
                        }
                    }

                    // Modified from DropItemInstanced, only drop for people using fists
                    if (itemType > 0)
                    {
                        if (Main.netMode == 2)
                        {
                            int itemWho = Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                                itemType, 1, true, 0, false, false);
                            Main.itemLockoutTime[itemWho] = 54000; // This slot cannot be used again for a while
                            foreach (Player player in Main.player)
                            {
                                if (!player.active) continue;
                                if (npc.playerInteraction[player.whoAmI])
                                {// Player must've hit the NPC at least once
                                    if (player.HeldItem.useStyle == ModPlayerFists.useStyle)
                                    {// Is equipped with fists, probably punching at time of death
                                        NetMessage.SendData(90, player.whoAmI, -1, null, itemWho, 0f, 0f, 0f, 0, 0, 0);
                                    }
                                }
                            }
                            Main.item[itemWho].active = false;
                        }
                        else if (Main.netMode == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                                itemType, 1, false, 0, false, false);
                        }
                    }
                }
            }
        }

    }
}
/*
 
				if (Main.netMode == 2)
				{
					int num = Item.NewItem((int)Position.X, (int)Position.Y, (int)HitboxSize.X, (int)HitboxSize.Y, itemType, itemStack, true, 0, false, false);
					Main.itemLockoutTime[num] = 54000;
					for (int i = 0; i < 255; i++)
					{
						if ((this.playerInteraction[i] || !interactionRequired) && Main.player[i].active)
						{
							NetMessage.SendData(90, i, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
						}
					}
					Main.item[num].active = false;
				}
				else if (Main.netMode == 0)
				{
					Item.NewItem((int)Position.X, (int)Position.Y, (int)HitboxSize.X, (int)HitboxSize.Y, itemType, itemStack, false, 0, false, false);
				}

     */
