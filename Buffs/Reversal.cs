using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class Reversal : ModBuff
    {
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (!player.noKnockback)
            {
                player.velocity.X = player.velocity.X * -1f;
            }
            player.DelBuff(buffIndex);
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if(npc.knockBackResist > 0f)
            {
                npc.velocity.X = npc.velocity.X * -1f;
            }
            npc.DelBuff(buffIndex);
        }
    }
}
