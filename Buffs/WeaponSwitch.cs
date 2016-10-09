using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class WeaponSwitch : ModBuff
    {
        public override void SetDefaults()
        {
            Main.buffName[Type] = "Switch Item";
            Main.buffTip[Type] = "Items will use their alternate functions";
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.itemAnimation > 0 && player.altFunctionUse > 0)
            {
                player.buffTime[buffIndex] = player.itemAnimation;
            }
        }
    }
}
