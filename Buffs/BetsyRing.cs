using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class BetsyRing : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Betsy's Protection");
            DisplayName.AddTranslation(GameCulture.Chinese, "贝蒂之护");
            Description.SetDefault("Nearby enemies suffer reduced defense and share damage");
            Description.AddTranslation(GameCulture.Chinese, "附近的敌人将降低防御力和承受伤害");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public const int debuffDist = 16 * 12;
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
                    npc.AddBuff(BuffID.BetsysCurse, 300, Main.time % 60 != 0);
                }
            }
        }
    }
}
