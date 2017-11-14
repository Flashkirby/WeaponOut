using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    public class AllFists : GlobalItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        private Item Item2Reforge = null;
        public override void PreReforge(Item item)
        {
            if (item.useStyle == ModPlayerFists.useStyle)
            {
                if (Item2Reforge == null)
                {
                    item.useStyle = 1; // Overhead swinging like swords, to get melee prefixes
                    Item2Reforge = item;
                }
            }
        }
        public override void PostReforge(Item item)
        {
            if (Item2Reforge != null)
            {
                item.useStyle = ModPlayerFists.useStyle; // Return to being a fist usestyle
                Item2Reforge = null;
            }
        }
    }
}
