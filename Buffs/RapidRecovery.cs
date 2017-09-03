using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class RapidRecovery : ModBuff
    {
        public const int framesPerHeal = 30;
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Rapid Recovery");
            Description.SetDefault(
                "Recovering damage from last hit taken");
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.buffTime[buffIndex] % RapidRecovery.framesPerHeal == 0 &&
                player.GetModPlayer<ModPlayerFists>().IsComboActive)
            {
                CombatText.NewText(player.Hitbox, CombatText.HealLife, 1, false, true);
                player.statLife++;
                if (player.statLife > player.statLifeMax2) player.statLife = player.statLifeMax2;
            }
        }

        public static void HealDamage(Player player, Mod mod, double damage)
        {
            HealDamage(player, mod, (int)damage);
        }
        public static void HealDamage(Player player, Mod mod, int damage)
        {
            int time = (damage * framesPerHeal) + framesPerHeal - 1;
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
