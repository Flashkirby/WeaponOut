using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class RallyBanner : ModBuff
    {
        public const float buffRadius = 1024;
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetDefaults()
        {
            Main.buffName[Type] = "Rallied";
            Main.buffTip[Type] = "Major improvements to all stats";
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // campfire/band of regen
            player.lifeRegen += 1;

            // regen band
            player.manaRegenDelayBonus++;
            player.manaRegenBonus += 25;

            // emblem sets
            player.magicDamage += 0.15f;
            player.meleeDamage += 0.15f;
            player.rangedDamage += 0.15f;
            player.minionDamage += 0.15f;
            player.thrownDamage += 0.15f;

            // other celestial stone things
            player.meleeSpeed += 0.1f;
            player.meleeCrit += 2;
            player.rangedCrit += 2;
            player.magicCrit += 2;
            player.pickSpeed -= 0.15f;
            player.minionKB += 0.5f;
            player.thrownCrit += 2;

            // Anklet bonus
            player.moveSpeed += 0.1f;
        }
    }
}
