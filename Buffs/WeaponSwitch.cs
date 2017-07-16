using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class WeaponSwitch : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableDualWeapons;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Switch Item");
            Description.SetDefault("Items will use their alternate functions");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.itemAnimation > 1 && player.altFunctionUse > 0)
            {
                player.buffTime[buffIndex] = player.itemAnimation - 1;
            }
        }
    }
}
