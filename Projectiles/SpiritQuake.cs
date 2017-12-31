using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System;

namespace WeaponOut.Projectiles
{
    public class SpiritQuake : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        private const float bulletFadeTime = 10;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Quake");
            DisplayName.AddTranslation(GameCulture.Chinese, "震颤");
        }
        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;
            projectile.alpha = 255;

            projectile.timeLeft = 12;
            projectile.penetrate = 4;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (projectile.ai[0] == 0)
            {
                projectile.ai[0]++;
                Main.PlaySound(SoundID.DD2_MonkStaffGroundMiss, projectile.position);
            }

            if (projectile.timeLeft % 2 == 0)
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke,
                    projectile.velocity.X / 5, projectile.velocity.Y / 5, 50, default(Color), 0.5f);
            }
            if (projectile.timeLeft % 4 == 0)
            {
                Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            }
            projectile.damage = projectile.damage * 5 / 6;
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
