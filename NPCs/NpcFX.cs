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
    }
}
