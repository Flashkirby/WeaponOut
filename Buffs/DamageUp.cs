﻿using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class DamageUp : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Retribution");
            Description.SetDefault("50% increased melee damage and melee critical strike chance");
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.meleeDamage += 0.5f;
            player.meleeCrit += 50;
        }
    }
}
