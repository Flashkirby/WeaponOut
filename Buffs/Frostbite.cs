using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class Frostbite : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Frost Degree Burns");
            Description.SetDefault("Frostburn REALLY burns!");
            Main.debuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // snow
            Dust d = Dust.NewDustDirect(player.position, player.width, player.height, 76, 0, 0.5f);
            d.velocity.Y -= 1f;
            d.noGravity = true;

            if(player.onFrostBurn)
            {
                if (player.lifeRegen > 0) player.lifeRegen = 0;
                player.lifeRegenTime = 0;
                player.lifeRegen -= 24; // doubled normal frostburn on top
            }
            // how2troll101
            if(player.statLife <= 40 && !player.frozen)
            {
                player.AddBuff(BuffID.Frozen, 30, false);
            }
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            // snow
            Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, 76, 0, 0.5f);
            d.velocity.Y -= 1f;
            d.noGravity = true;

            if (npc.onFrostBurn)
            {
                if (npc.lifeRegen > 0) npc.lifeRegen = 0;
                npc.lifeRegen -= 32; // doubled normal frostburn on top
            }
        }
    }
}
