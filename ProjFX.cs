using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut
{
    public class ProjFX : GlobalProjectile
    {
        public override void PostAI(Projectile projectile)
        {
            //Ignore npcs and statics
            if (projectile.npcProj) return;
            if (projectile.position == projectile.oldPosition) return;

            PlayerFX p = Main.player[projectile.owner].GetModPlayer<PlayerFX>(mod);
            if (p.lunarMagicVisual)
            {

            }
            if (p.lunarRangeVisual && projectile.ranged)
            {
                Dust d = Main.dust[Dust.NewDust(
                    projectile.Center - new Vector2(4, 4), 0, 0,
                    DustID.Vortex
                    )];
                d.scale = (float)Math.Log10(projectile.width + projectile.height) * 0.7f;
                d.velocity = d.velocity * 0.1f + projectile.velocity / 2;
                d.noGravity = true;

                if (Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
                { d.noLight = true; }
            }
            if(p.lunarThrowVisual)
            {

            }
        }
    }
}
