using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class MirrorBadge : ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Mirror Badge";
            item.toolTip = "Reflects enemy projectiles";
            item.width = 18;
            item.height = 20;
            item.mana = 8;
            item.rare = 3;
            item.value = 50000;
            item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if(player.CheckMana(item.mana,false,false))
            {
                player.AddBuff(mod.BuffType("MirrorBarrier"), 1);
            }
        }

        public override bool PreDrawInWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale)
        {
            if (Main.rand.Next(15) == 0)
            {
                int dustIndex = Dust.NewDust(item.position, item.width, item.height, 43, 0, 0, 100, Color.White, 0.3f);
                Main.dust[dustIndex].velocity *= 0.1f;
                Main.dust[dustIndex].fadeIn = 1f;
            }
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale);
        }
    }
}
