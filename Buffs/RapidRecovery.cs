using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class RapidRecovery : ModBuff
    {
        public const int healthPer2Secs = 6; // Rapid Healing is 6
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Rapid Recovery");
            Description.SetDefault(
                "Life regeneration is greatly increased! Fist combos double life regeneration");
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.GetModPlayer<ModPlayerFists>().ComboCounter > 0)
            {
                player.lifeRegenCount += healthPer2Secs;
            }
            else
            {
                player.lifeRegenCount += healthPer2Secs / 2; // Effect is still there, only half strength
            }
        }

        public static void HealDamage(Player player, Mod mod, double damage)
        {
            HealDamage(player, mod, (int)damage);
        }
        public static void HealDamage(Player player, Mod mod, int damage)
        {
            int time = (int)(damage * (120f / healthPer2Secs));
            int id = mod.BuffType<RapidRecovery>();
            int index = player.FindBuffIndex(id);
            if (index >= 0)
            {
                player.buffTime[index] = time;
            }
            else
            {
                player.AddBuff(id, time, false);
            }
        }
    }
}
