using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class BloodLust : ModBuff
    {
        public static readonly int POWERFACTOR = 200;

        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableFists;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Blood Lust");
            Description.SetDefault("Damage dealt increased by $POWERFACTOR%, increases Blood Baghnakh effectiveness");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            float power = POWERFACTOR * 0.01f;
            player.thrownDamage += power;
            player.meleeDamage += power;
            player.rangedDamage += power;
            player.magicDamage += power;
            player.minionDamage += power;
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            ModPlayerFists mpf = Main.LocalPlayer.GetModPlayer<ModPlayerFists>();
            tip = tip.Replace("$POWERFACTOR", "" + POWERFACTOR);
        }
    }
}
