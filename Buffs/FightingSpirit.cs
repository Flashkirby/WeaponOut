using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class FightingSpirit : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }

        public const string DefaultName = "Puglist's Resolve";
        public int life2heal;
        public override void SetDefaults()
        {
            DisplayName.SetDefault(FightingSpirit.DefaultName);
            Description.SetDefault("Restores $LIFE life at the end of a combo");
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
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }

        public int life2heal;
        public override void SetDefaults()
        {
            DisplayName.SetDefault(FightingSpirit.DefaultName);
            Description.SetDefault("Restores life lost at the end of a combo ");
        }
    }
    public class FightingSpiritMax : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }

        public int life2heal;
        public override void SetDefaults()
        {
            DisplayName.SetDefault(FightingSpirit.DefaultName);
            Description.SetDefault("Restores $LIFE life at the end of a combo");
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
