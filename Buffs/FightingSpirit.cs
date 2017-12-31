using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class FightingSpirit : ModBuff
    {
        public const string DefaultName = "Puglist's Resolve";
        public int life2heal;
        public override void SetDefaults()
        {
            DisplayName.SetDefault(FightingSpirit.DefaultName);
            DisplayName.AddTranslation(GameCulture.Chinese, "拳师的决意");

            Description.SetDefault("Restores $LIFE life at the end of a combo");
            Description.AddTranslation(GameCulture.Chinese, "连击结束后回复$LIFE点生命值");
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            tip = tip.Replace("$LIFE", "" + life2heal);
        }
        public override void Update(Player player, ref int buffIndex)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>(); life2heal = pfx.sashLifeLost;
        }
    }

    public class FightingSpiritEmpty : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.EnableFists; }

        public int life2heal;
        public override void SetDefaults()
        {
            DisplayName.SetDefault(FightingSpirit.DefaultName);
            DisplayName.AddTranslation(GameCulture.Chinese, "拳师的决意");

            Description.SetDefault("Restores life lost at the end of a combo ");
            Description.AddTranslation(GameCulture.Chinese, "连击结束后恢复在连击时损失的生命值");
        }
    }
    public class FightingSpiritMax : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.EnableFists; }

        public int life2heal;
        public override void SetDefaults()
        {
            DisplayName.SetDefault(FightingSpirit.DefaultName);
            DisplayName.AddTranslation(GameCulture.Chinese, "拳师的决意");

            Description.SetDefault("Restores $LIFE life at the end of a combo");
            Description.AddTranslation(GameCulture.Chinese, "连击结束后回复$LIFE点生命值");
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            tip = tip.Replace("$LIFE", "" + life2heal);
        }
        public override void Update(Player player, ref int buffIndex)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>(); life2heal = pfx.sashLifeLost;
        }
    }
}
