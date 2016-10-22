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
            return true;
        }

        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Wizard)
            {
                //add puzzling cutter if any mech boss was defeated
                if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)
                {
                    try
                    {
                        shop.item[nextSlot].SetDefaults(mod.GetItem("PuzzlingCutter").item.type);
                        nextSlot++;
                    }
                    catch (Exception e) { }
                }
            }
        }
    }
}
