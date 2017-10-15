using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    public class SpiritExplosion : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Adamantite Explosion");
        }
        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;
            projectile.alpha = 255;

            projectile.timeLeft = 3;
            projectile.penetrate = -1;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (projectile.ai[0] == 0)
            {
                projectile.ai[0]++;
                Main.PlaySound(SoundID.Item14.WithVolume(0.5f), projectile.position);

                Dust d;
                for (int i = 0; i < 8; i++)
                {
                    d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
                    d.velocity *= 3f;
                    if (Main.rand.Next(2) == 0)
                    {
                        d.scale = 0.5f;
                        d.fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int i = 0; i < 14; i++)
                {
                    d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
                    d.velocity *= 5f;
                    d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
                    d.velocity *= 2f;
                }
                Gore g;
                for (int i = 0; i < 2; i++)
                {
                    for (int y = -1; y < 2; y += 2)
                    {
                        for (int x = -1; x < 2; x += 2)
                        {
                            g = Gore.NewGoreDirect(projectile.Center, default(Vector2), Main.rand.Next(61, 64), 1f);
                            g.velocity *= 0.33f * i;
                            g.velocity += new Vector2(x, y);
                        }
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Main.player[projectile.owner].active && projectile.localAI[0] == 0)
            {
                Main.player[projectile.owner].GetModPlayer<ModPlayerFists>().ModifyComboCounter(1);
                projectile.localAI[0]++;
            }
        }
    }
}
