using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class TipsyMead : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Monster Mead");
            Description.SetDefault("Heal from melee strikes after getting hit, lowered defense");
            Main.debuff[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense -= 4;
            player.GetModPlayer<PlayerFX>().demonBloodHealMod += 1f;
        }
    }
}
