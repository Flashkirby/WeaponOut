using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Spins around a player, re-orients when player is attacking
    /// </summary>
    public class SpiritMartianFist : ModProjectile
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Booster Mini-Fist");
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;

            projectile.penetrate = 1;
            projectile.timeLeft = 600;

            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

        }
        public override bool? CanCutTiles() { return false; }
        
        public NPC target
        {
            get
            {
                try { return Main.npc[(int)projectile.ai[0]]; }
                catch { return null; }
            }
            set
            {
                try { target = value; }
                catch { target = null; }
            }
        }
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (player == null || !player.active || player.dead || projectile.hostile ||
                target == null || !target.active || target.life <= 0 || target.friendly) { projectile.timeLeft = 0; return; }
            if (projectile.localAI[0] == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    double angle = Main.time + i * 4 / 10.0;
                    Dust s = Dust.NewDustPerfect(projectile.Center, 229, new Vector2(
                        (float)(2 * Math.Sin(angle)),
                        (float)(2 * Math.Cos(angle))));
                    s.noGravity = true;
                    s.fadeIn = 1.5f;
                }
            }
            if (projectile.localAI[0] < 6)
            {
                projectile.localAI[0]++;
                projectile.friendly = false;
            }
            else
            {
                projectile.friendly = true;
                // Home in
                Vector2 vector2target = target.Center - projectile.Center;
                vector2target.SafeNormalize(default(Vector2));
                projectile.velocity *= 0.95f;
                projectile.velocity = (projectile.velocity * 119 + vector2target) / 120;
            }

            Dust d = Dust.NewDustDirect(projectile.Center, 0, 0, 229);
            d.velocity = -projectile.velocity / 4;
            d.position += d.velocity * 2;
            d.noGravity = true;
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
        }

        public override void Kill(int timeLeft)
        {
            for(int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Tin, 0f, 0f, 0, default(Color), 1.5f);
                d.velocity += projectile.velocity / 4;
                d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.t_Martian, 0f, 0f, 0, default(Color), 1f);
                d.velocity -= projectile.velocity / 8;
            }
            Main.PlaySound(SoundID.Item110, projectile.position);
        }
    }
}
