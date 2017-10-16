using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class PumpkinMark : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Pumpkin Mark");
            Description.SetDefault("You will explode if struck by a counter attack");
            Main.debuff[Type] = true;
        }
    }
}
