using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class ManaReduction : ModBuff
    {
        public override void SetDefaults()
        {
            Main.buffName[Type] = "Mana Cost Reduced";
            Main.buffTip[Type] = "Next magic attack has mana cost reduced by 80%";
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.manaCost -= 0.8f;
            if (player.itemAnimation == player.itemAnimationMax - 1
                && player.inventory[player.selectedItem].magic)
            {
                player.DelBuff(buffIndex);
            }
        }
    }
}
