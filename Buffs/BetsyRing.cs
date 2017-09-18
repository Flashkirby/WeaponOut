using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class BetsyRing : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Betsy's Protection");
            Description.SetDefault("Nearby enemies are ignited and suffer reduced defense");
            Main.buffNoTimeDisplay[Type] = true;
        }

        private const int debuffDist = 16 * 12;
        public override void Update(Player player, ref int buffIndex)
        {
            player.inferno = true;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.immortal ||
                    npc.Center.X > player.Center.X + debuffDist ||
                    npc.Center.X < player.Center.X - debuffDist ||
                    npc.Center.Y > player.Center.Y + debuffDist ||
                    npc.Center.Y < player.Center.Y - debuffDist) continue;
                if (npc.Distance(player.Center) < debuffDist)
                {
                    // Only update buff net-side once in a while
                    npc.AddBuff(BuffID.OnFire, 120, Main.time % 60 != 0);
                    npc.AddBuff(BuffID.ShadowFlame, 60, Main.time % 30 != 0);
                    npc.AddBuff(BuffID.BetsysCurse, 300, Main.time % 60 != 0);
                }
            }
        }
    }
}
