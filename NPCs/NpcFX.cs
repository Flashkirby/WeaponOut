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
                        if (NPC.downedGolemBoss)
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

        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            if (Main.expertMode)
            {
                if (npc.life <= 0 && item.useStyle == ModPlayerFists.useStyle) // Killed by a fist weapon?
                {
                    if (npc.type == NPCID.EyeofCthulhu)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                            mod.ItemType<Items.Accessories.RushCharm>(), 1, false, 0, false, false);
                    }
                    if (npc.type >= 13 && npc.type <= 15)
                    {
                        int wormCount = 0;
                        foreach (NPC worm in Main.npc)
                        {
                            if (!worm.active) continue;
                            if (worm.type >= 13 && worm.type <= 15)
                            {
                                wormCount++;
                            }
                        }
                        if (wormCount < 2 || (wormCount == 2 && npc.type == NPCID.EaterofWorldsBody))
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                                mod.ItemType<Items.Accessories.DriedEye>(), 1, false, 0, false, false);
                        }
                    }
                    if (npc.type == NPCID.BrainofCthulhu)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height,
                            mod.ItemType<Items.Accessories.StainedTooth>(), 1, false, 0, false, false);
                    }
                }
            }
        }
    }
}
