using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class PumpkinMark : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Pumpkin Mark");
            Description.SetDefault("You have been marked for detonation");
            Main.debuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if(npc.buffTime[buffIndex] == 0 && npc.lastInteraction != 255)
            {
                Projectile.NewProjectile(npc.Center, new Vector2(), 
                    Items.Weapons.Fists.GlovesPumpkin.projID, 400, 12f, npc.lastInteraction);
            }
        }
    }
}
