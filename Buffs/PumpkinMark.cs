using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class PumpkinMark : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Pumpkin Mark");
            DisplayName.AddTranslation(GameCulture.Chinese, "南瓜标记");

            Description.SetDefault("You have been marked for detonation");
            Description.AddTranslation(GameCulture.Chinese, "你已经被标记了引爆标志");
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
