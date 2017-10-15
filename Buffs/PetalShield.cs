using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class PetalShield : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture) { return ModConf.enableFists; }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Petal Shield");
            Description.SetDefault(Language.GetTextValue("BuffDescription.Ironskin").Replace("8", "12"));
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 12;

            Vector2 position;
            for (int i = 0; i < 3; i++)
            {
                // meteoric shield effect
                float angle = Main.rand.Next((int)(-Math.PI * 10000), (int)(Math.PI * 10000)) / 10000f;
                position = ProjFX.CalculateCircleVector(player, angle, 26f);
                Dust d = Main.dust[Dust.NewDust(position, 0, 0, 58)]; // try 72 as well
                d.rotation += i * d.velocity.X;
                d.velocity = player.velocity / 2;
                d.noGravity = true;
                d.noLight = true;
                d.scale *= 1.5f;
            }
        }
    }
}
