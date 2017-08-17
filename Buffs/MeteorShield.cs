using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class MeteorShield : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableFists;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Meteor Shield");
            Description.SetDefault("Damage taken reduced by 20%");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.2f;

            Vector2 position;
            for (int i = 0; i < 3; i++)
            {
                // meteoric shield effect
                float angle = Main.rand.Next((int)(-Math.PI * 10000), (int)(Math.PI * 10000)) / 10000f;
                position = ProjFX.CalculateCircleVector(player, angle, 22f);
                Dust d = Main.dust[Dust.NewDust(position, 0, 0, 6)];
                d.rotation += i * d.velocity.X;
                d.velocity = player.velocity / 2;
                d.noGravity = true;
                d.noLight = true;
                d.scale *= 1.5f;
            }
        }
    }
}
