using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class RallyBanner : ModBuff
    {
        public const float buffRadius = 800; // 100ft, same as shared accessory info
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Rallied");
            Description.SetDefault("Major improvements to all stats");
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

            // Sunflower buff
            player.moveSpeed += 0.1f;
            player.moveSpeed *= 1.1f;
        }
    }
}
