using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class Knockup : ModBuff
    {
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (!player.noKnockback)
            {
                player.velocity = new Vector2(
                    Math.Abs(player.velocity.Y) * Math.Sign(player.velocity.X) * 0.5f,
                    Math.Abs(player.velocity.X) * -2f);
            }
            player.DelBuff(buffIndex);
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if(npc.knockBackResist > 0f)
            {
                npc.velocity = new Vector2(
                    Math.Abs(npc.velocity.Y) * Math.Sign(npc.velocity.X) * 0.5f, 
                    Math.Abs(npc.velocity.X) * -(2f - npc.knockBackResist));
            }
            npc.DelBuff(buffIndex);
        }
    }
}
