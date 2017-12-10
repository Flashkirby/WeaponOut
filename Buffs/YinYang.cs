using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class YinYang : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Balance");
            Description.SetDefault("Restores $LIFE life and further increases melee damage by $DAMAGE% at the end of a combo");
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            PlayerFX pfx = Main.LocalPlayer.GetModPlayer<PlayerFX>();
            tip = tip.Replace("$LIFE", "" + 
                (int)((Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife) * pfx.CalculateYangPower(pfx.GetYinYangBalance())));
            tip = tip.Replace("$DAMAGE", "" + 
                (int)(pfx.CalculateYinPower(pfx.GetYinYangBalance())* 100));
        }
    }

    public class Yin : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.EnableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Imbalance: Yin");
            Description.SetDefault("Increases melee damage by $DAMAGE% at the end of a combo");
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            PlayerFX pfx = Main.LocalPlayer.GetModPlayer<PlayerFX>();
            tip = tip.Replace("$DAMAGE", "" + 
                (int)(pfx.CalculateYinPower(pfx.GetYinYangBalance()) * 100));
        }
    }
    public class Yang : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.EnableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Imbalance: Yang");
            Description.SetDefault("Restores $LIFE life at the end of a combo");
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            PlayerFX pfx = Main.LocalPlayer.GetModPlayer<PlayerFX>();
            tip = tip.Replace("$LIFE", "" + 
                (int)((Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife) * pfx.CalculateYangPower(pfx.GetYinYangBalance())));
            tip = tip.Replace("$DAMAGE", "" + 
                (int)(pfx.CalculateYinPower(pfx.GetYinYangBalance()) * 100));
        }
    }
}
