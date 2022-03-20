using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class MirrorBarrier : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Magic Barrier");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔法屏障");

            Description.SetDefault("The next projectile may be reflected");
            Description.AddTranslation(GameCulture.Chinese, "下一次的抛射物可以被反射");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            PlayerFX p = player.GetModPlayer<PlayerFX>();
            p.reflectingProjectiles = true;
        }
    }
}
