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
}
