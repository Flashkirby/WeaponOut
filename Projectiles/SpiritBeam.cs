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
    public class SpiritBeam : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Beam");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔晶射线");
        }
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.ignoreWater = true;
            projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Vector2 start = projectile.Center;
            if (Main.player[projectile.owner].active) start = Main.player[projectile.owner].Center;
            for (int i = 0; i < 30; i++)
            {
                Dust d = Main.dust[Dust.NewDust(new Vector2(
                    MathHelper.Lerp(projectile.Center.X, start.X, i / 30f),
                    MathHelper.Lerp(projectile.Center.Y, start.Y, i / 30f)
                    ), projectile.width, projectile.height,
                    DustID.BlueCrystalShard + Main.rand.Next(3), 0, 0, 100, default(Color), 1.2f)];
                d.velocity *= 0.1f;
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Main.player[projectile.owner].active)
            {
                Main.player[projectile.owner].GetModPlayer<ModPlayerFists>().ModifyComboCounter(1);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height,
                     DustID.BlueCrystalShard + Main.rand.Next(3), 0, 0, 100, default(Color), 1.2f)];
                d.velocity *= 2 + (0.1f * i);
                d.noGravity = true;
            }
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 27, 0.6f, 0.3f);
        }
    }
}
