using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class DemonFrenzy : ModBuff
    {
        public static readonly int POWERFACTOR = 1;

        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableFists;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Demon Frenzy");
            Description.SetDefault("Damage dealt increased by $POWERFACTOR%, damage received increased by $PAINFACTOR%");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            float power = POWERFACTOR * 0.01f * mpf.ComboCounter;
            player.endurance -= power / 2;
            player.thrownDamage += power;
            player.meleeDamage += power;
            player.rangedDamage += power;
            player.magicDamage += power;
            player.minionDamage += power;
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            ModPlayerFists mpf = Main.LocalPlayer.GetModPlayer<ModPlayerFists>();
            int power = POWERFACTOR * mpf.ComboCounter;
            tip = tip.Replace("$POWERFACTOR", "" + power);
            tip = tip.Replace("$PAINFACTOR", "" + (power / 2));
            if (power >= 100) rare = 4;
            else if (power >= 50) rare = 3;
            else if (power >= 25) rare = 2;
            else if (power >= 10) rare = 1;
        }
    }
}
