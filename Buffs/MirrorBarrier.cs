using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class MirrorBarrier : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Magic Barrier");
            Description.SetDefault("The next projectile may be reflected");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            PlayerFX p = player.GetModPlayer<PlayerFX>(mod);
            p.reflectingProjectiles = true;
        }
    }
}
