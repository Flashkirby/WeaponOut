using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class YinYang : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Balance");
            DisplayName.AddTranslation(GameCulture.Chinese, "平衡");
            Description.SetDefault("Restores $LIFE life and further increases melee damage by $DAMAGE% at the end of a combo");
            Description.AddTranslation(GameCulture.Chinese, "连击结束时回复$LIFE生命值与增加$DAMAGE%近战伤害");
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
            DisplayName.AddTranslation(GameCulture.Chinese, "阴");

            Description.SetDefault("Increases melee damage by $DAMAGE% at the end of a combo");
            Description.AddTranslation(GameCulture.Chinese, "连击结束时增加$DAMAGE%近战伤害");
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
            DisplayName.AddTranslation(GameCulture.Chinese, "阳");

            Description.SetDefault("Restores $LIFE life at the end of a combo");
            Description.AddTranslation(GameCulture.Chinese, "连击结束时回复$LIFE生命值");
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
