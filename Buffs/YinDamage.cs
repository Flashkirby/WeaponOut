using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class YinDamage : ModBuff
    {
        public int variable;
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Absolute Strength");
            Description.SetDefault("Melee damage is increased by $DAMAGE%");
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            tip = tip.Replace("$DAMAGE", "" + variable);
        }

        public override void Update(Player player, ref int buffIndex)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();
            if (pfx.yinMeleeBonus <= 0f) return;

            if (player.buffTime[buffIndex] == 1) player.buffTime[buffIndex]++;

            player.meleeDamage += pfx.yinMeleeBonus;
            variable = (int)(pfx.yinMeleeBonus * 100f);
        }
    }
}
