﻿using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.NPCs
{
    public class NpcFX : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (ModConf.EnableWhips)
            {
                if (type == NPCID.Wizard)
                {
                    //add puzzling cutter if hardmode
                    if (Main.hardMode)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Weapons.Whips.PuzzlingCutter>());
                        nextSlot++;
                    }
                }
            }
            if (ModConf.EnableBasicContent)
            {
                if (type == NPCID.ArmsDealer)
                {
                    //add scrap salvo after mech
                    if (Main.hardMode && NPC.downedPlantBoss)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Weapons.Basic.ScrapSalvo>());
                        nextSlot++;
                    }
                }
            }
            if (ModConf.EnableFists)
            {
                if (type == NPCID.Clothier)
                {
                    //add headbands after bossess
                    if (NPC.downedBoss3)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Armour.FistVeteranHead>());
                        nextSlot++;
                    }
                    if (NPC.downedGolemBoss)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Armour.FistMasterHead>());
                        nextSlot++;
                    }
                }
            }
        }

        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
            if (ModConf.EnableFists)
            {
                int chance = 5;
                if (NPC.downedSlimeKing) chance--;
                if (NPC.downedBoss1) chance--;
                int taekwonBody = ModContent.ItemType<Items.Armour.FistPowerBody>();
                int boxingBody = ModContent.ItemType<Items.Armour.FistSpeedBody>();
                foreach (Player p in Main.player)
                {
                    if (p.armor[1].type == taekwonBody || p.armor[1].type == boxingBody)
                    {
                        chance -= 2;
                    }
                }
                if (Main.rand.Next(Math.Max(1,chance)) == 0)
                {
                    shop[nextSlot] = ModContent.ItemType<Items.Armour.FistDefBody>(); nextSlot++;
                    shop[nextSlot] = ModContent.ItemType<Items.Armour.FistDefLegs>(); nextSlot++;
                }
            }
        }
        
        public override void NPCLoot(NPC npc)
        {
            if (ModConf.EnableFists)
            {
                int itemType = -1;
                if (npc.type == NPCID.GraniteGolem && Main.rand.Next(10) == 0)
                {
                    Item.NewItem(npc.position, npc.Size, ModContent.ItemType<Items.Weapons.Fists.FistsGranite>(), 1, false, -1, false, false);
                    return;
                }
                if (npc.type == NPCID.BoneLee && Main.rand.Next(6) == 0)
                {
                    Item.NewItem(npc.position, npc.Size, ModContent.ItemType<Items.Weapons.Fists.GlovesLee>(), 1, false, -1, false, false);
                    return;
                }

                #region Expert Accessory Drops
                // Bosses drop per-player
                if (npc.boss)
                {
                    itemType = -1;
                    if (Main.expertMode)
                    {
                        if (npc.type == NPCID.KingSlime)
                        { itemType = ModContent.ItemType<Items.Accessories.RoyalHealGel>(); }

                        if (npc.type == NPCID.EyeofCthulhu)
                        { itemType = ModContent.ItemType<Items.Accessories.RushCharm>(); }

                        if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
                        { itemType = ModContent.ItemType<Items.Accessories.DriedEye>(); }

                        if (npc.type == NPCID.BrainofCthulhu)
                        { itemType = ModContent.ItemType<Items.Accessories.StainedTooth>(); }

                        if (npc.type == NPCID.QueenBee)
                        { itemType = ModContent.ItemType<Items.Accessories.QueenComb>(); }

                        if (npc.type == NPCID.SkeletronHead)
                        { itemType = ModContent.ItemType<Items.Weapons.Fists.FistsBone>(); }

                        if (npc.type == NPCID.WallofFlesh)
                        { itemType = ModContent.ItemType<Items.DemonBlood>(); }

                        if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
                        {
                            int partner = NPCID.Spazmatism;
                            if (npc.type == NPCID.Spazmatism) partner = NPCID.Retinazer;
                            // Last eye standing
                            if (!NPC.AnyNPCs(partner))
                            {
                                itemType = ModContent.ItemType<Items.Accessories.ScrapActuator>();
                            }
                        }

                        if (npc.type == NPCID.TheDestroyer)
                        { itemType = ModContent.ItemType<Items.Accessories.ScrapFrame>(); }

                        if (npc.type == NPCID.SkeletronPrime)
                        { itemType = ModContent.ItemType<Items.Accessories.ScrapTransformer>(); }

                        if (npc.type == NPCID.Plantera)
                        { itemType = ModContent.ItemType<Items.Accessories.LifeRoot>(); }

                        if (npc.type == NPCID.Golem)
                        { itemType = ModContent.ItemType<Items.Accessories.GolemHeart>(); }

                        if (npc.type == NPCID.DukeFishron)
                        { itemType = ModContent.ItemType<Items.Accessories.SoapButNotReallySoap>(); }

                    }
                    // Modified from DropItemInstanced, only drop for people using fists
                    if (itemType > 0)
                    {
                        DropInstancedFistItem(npc, itemType);
                    }
                }
                #endregion

                #region Bonus Boss Fist Drops
                itemType = -1;
                if (npc.boss || npc.type == NPCID.DD2Betsy)
                {
                    bool chance = Main.rand.Next(5) == 0;
                    if (!chance)
                    {
                        foreach (Player p in Main.player)
                        {
                            if (!p.active) continue;
                            if (p.HeldItem.useStyle == ModPlayerFists.useStyle)
                            {
                                chance = Main.rand.Next(2) == 0 || Main.expertMode;
                                break;
                            }
                        }
                    }

                    if (chance)
                    {
                        if (npc.type == NPCID.KingSlime)
                        { itemType = ModContent.ItemType<Items.Weapons.Fists.FistsSlime>(); }

                        if (npc.type == NPCID.Plantera)
                        { itemType = ModContent.ItemType<Items.Weapons.Fists.KnucklesPlantera>(); }

                        if (npc.type == NPCID.MartianSaucerCore)
                        { itemType = ModContent.ItemType<Items.Weapons.Fists.FistsMartian>(); }

                        if (npc.type == NPCID.DukeFishron)
                        { itemType = ModContent.ItemType<Items.Weapons.Fists.KnucklesDuke>(); }

                        if (npc.type == NPCID.DD2Betsy)
                        { itemType = ModContent.ItemType<Items.Weapons.Fists.FistsBetsy>(); }

                    }
                }

                if(Main.pumpkinMoon && npc.type == NPCID.Pumpking) {
                    int waveChance = Math.Max(1, (Main.expertMode ? 12 : 16) - NPC.waveNumber); // 12 is when two start spawning at once
                    int chance = Main.rand.Next(waveChance);
                    //Main.NewText("Pumpking Slain, " + chance + "/" + waveChance);
                    if (chance == 0) {
                        itemType = ModContent.ItemType<Items.Weapons.Fists.GlovesPumpkin>();
                    }
                }

                if(Main.snowMoon && npc.type == NPCID.IceQueen) {
                    int waveChance = Math.Max(1, (Main.expertMode ? 12 : 16) - NPC.waveNumber); // 12 is when two start spawning at once
                    int chance = Main.rand.Next(waveChance);
                    // Main.NewText("Pumpking Slain, " + chance + "/" + waveChance);
                    if (chance == 0) {
                        itemType = ModContent.ItemType<Items.Weapons.Fists.FistsFrozen>();
                    }
                }

                // Modified from DropItemInstanced, only drop for people using fists
                if (itemType > 0)
                {
                    DropInstancedFistItem(npc, itemType);
                }
                #endregion
            }
        }

        private static int CalculateWaveChance(float modifier) {
            int waveChance = (int)((30 -
                (Main.expertMode ? NPC.waveNumber + 6 : NPC.waveNumber)
                ) / 1.25 * modifier);
            //Main.NewText("DROPCHANCE1 = " + waveChance);
            if (Main.expertMode) waveChance -= 2;
            if (waveChance < 1) waveChance = 1;
            return waveChance;
        }

        private static void DropInstancedFistItem(NPC npc, int itemType)
        {
            if (Main.netMode == 2)
            {
                int itemWho = Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                    itemType, 1, true, -1, false, false);
                Main.itemLockoutTime[itemWho] = 54000; // This slot cannot be used again for a while
                foreach (Player player in Main.player)
                {
                    if (!player.active) continue;
                    if (npc.playerInteraction[player.whoAmI])
                    {// Player must've hit the NPC at least once
                        if (player.HeldItem.useStyle == ModPlayerFists.useStyle)
                        {// Is equipped with fists, probably punching at time of death
                            NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, itemWho, 0f, 0f, 0f, 0, 0, 0);
                            
                        }
                    }
                }
                Main.item[itemWho].active = false;
            }
            else if (Main.netMode == 0 && Main.LocalPlayer.HeldItem.useStyle == ModPlayerFists.useStyle)
            {
                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                    itemType, 1, false, -1, false, false);
            }
        }

        #region The last straw against player-snapping bosses :(
        public override bool PreAI(NPC npc)
        {
            if (!npc.active || npc.life <= 0 || npc.lifeMax < 2000 || !npc.chaseable || npc.npcSlots <= 0f ||
                !npc.HasPlayerTarget) return true; // we don't deal in small fry and clones

            if (npc.boss || npc.GetBossHeadTextureIndex() >= 0)
            {
                PlayerFX pfx;
                foreach (Player p in Main.player)
                {
                    if (!p.active || p.dead) continue;
                    pfx = p.GetModPlayer<PlayerFX>();
                    if (pfx.ghostPosition && !pfx.FakePositionReal.Equals(default(Vector2)))
                    { p.position = pfx.FakePositionTemp; }
                }
            }
            return base.PreAI(npc);
        }

        public override void PostAI(NPC npc)
        {
            if (npc.lifeMax < 2000 || npc.npcSlots <= 0f) return; // like eater of worlds
            if (npc.boss || npc.GetBossHeadTextureIndex() >= 0)
            {
                PlayerFX pfx;
                foreach (Player p in Main.player)
                {
                    if (!p.active || p.dead) continue;
                    pfx = p.GetModPlayer<PlayerFX>();
                    if (pfx.ghostPosition && !pfx.FakePositionReal.Equals(default(Vector2)))
                    { p.position = pfx.FakePositionReal; }
                }
            }
        }

        #endregion
    }
}
