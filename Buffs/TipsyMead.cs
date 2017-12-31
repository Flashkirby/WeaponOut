using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class TipsyMead : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Monster Mead");
            DisplayName.AddTranslation(GameCulture.Chinese, "恶魔蜂蜜酒");

            Description.SetDefault("Heal from melee strikes after getting hit, lowered defense");
            Description.AddTranslation(GameCulture.Chinese, "所受伤害可以通过近战攻击恢复，但是降低防御力");
            Main.debuff[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense -= 4;
            player.GetModPlayer<PlayerFX>().demonBloodHealMod += 1f;
        }
    }
}
