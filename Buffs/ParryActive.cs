using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class ParryActive : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableFists;
        }

        public override void SetDefaults()
        {
            Main.buffName[Type] = "Parrying Moment";
            Main.buffTip[Type] = "Ready for parry bonus";
        }
    }
}
