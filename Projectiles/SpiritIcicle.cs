using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Goes in either direction based on ai[1]
    /// </summary>
    public class SpiritIcicle : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Icicle");
            DisplayName.AddTranslation(GameCulture.Chinese, "冰锥");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            
            projectile.penetrate = 4;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.ignoreWater = false;
        }

        public override void AI()
        {
            if (projectile.ai[0] < 10)
            {
                if (projectile.ai[0] == 0)
                {
                    projectile.velocity /= 2; // Half speed
                }
                else
                {
                    projectile.velocity *= 1.1f; // Speed up to original (roughly)
                }
                projectile.alpha = Math.Max(0, 200 - (int)projectile.ai[0] * 20);
            }

            if (projectile.ai[0] > 20)
            {
                projectile.velocity.X *= 0.98f;
                projectile.velocity.Y = Math.Min(20f, projectile.velocity.Y + 0.4f);
            }

            Dust d = Main.dust[Dust.NewDust(projectile.Center - new Vector2(4, 4), 
                0, 0,
                 88, 0, 0, 100, default(Color), 0.5f)];
            d.fadeIn = 0.9f;
            d.noGravity = true;
            d.velocity = projectile.velocity / 2;

            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
            projectile.ai[0]++;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 8;
            height = 8;
            return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.penetrate == projectile.maxPenetrate)
            {
                if (Main.player[projectile.owner].active)
                {
                    Main.player[projectile.owner].GetModPlayer<ModPlayerFists>().ModifyComboCounter(1);
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height,
                     DustID.Ice, -projectile.velocity.X, -projectile.velocity.Y, 100, default(Color), 1.2f)];
                d.velocity *= 0.1f * i;
            }
            Main.PlaySound(SoundID.Item27, projectile.Center);
        }
    }
}
