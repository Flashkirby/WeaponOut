using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    public class SpiritPumpkinsplosion : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pumpkin Explosion");
        }
        public override void SetDefaults()
        {
            projectile.width = 112;
            projectile.height = 112;
            projectile.alpha = 255;
            
            projectile.penetrate = -1;

            projectile.friendly = false;
            projectile.melee = true;
            projectile.tileCollide = false;

            projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 6;
            projectile.melee = true;
        }

        public override void AI()
        {
            if (projectile.ai[0] == 0)
            {
                projectile.friendly = true;
                projectile.timeLeft = 3;
                Main.PlaySound(SoundID.Item14, projectile.position);

                #region Explosion Dust FX
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
                #endregion
            }
            projectile.ai[0]++;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { crit = false; }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        { crit = false; }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            int buffIndex = target.FindBuffIndex(Items.Weapons.Fists.GlovesPumpkin.buffID);
            if (buffIndex >= 0)
            {
                target.DelBuff(buffIndex);
                SpawnExplosions(target);
            }
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            int buffIndex = target.FindBuffIndex(Items.Weapons.Fists.GlovesPumpkin.buffID);
            if (buffIndex >= 0)
            {
                target.DelBuff(buffIndex);
                SpawnExplosions(target);
            }
        }

        private void SpawnExplosions(Entity target)
        {
            int dmg = 535;
            Projectile.NewProjectile(target.Center, new Vector2(), projectile.type, dmg, 12f, projectile.owner, -5f);

            Vector2 pos = new Vector2(
                64 * Main.rand.NextFloatDirection(),
                 64 * Main.rand.NextFloatDirection()
                );
            Projectile.NewProjectile(target.Center + target.velocity * 15 + pos, new Vector2(), projectile.type, dmg, 12f, projectile.owner, -20f);
        }
    }
}
