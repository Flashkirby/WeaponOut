using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Localization;

namespace WeaponOut.Projectiles
{
    public class ManaRestoreBeam : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mana Restoration Beam");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔力恢复射线");
        }
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;

            projectile.extraUpdates = 5;
            projectile.penetrate = -1;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
        }

        public override void AI()
        {
            if (projectile.damage > 0)
            {
                projectile.ai[1] = projectile.damage;
                projectile.damage = 0;
                projectile.netUpdate = true;
            }
            try
            {
                Player p = Main.player[(int)projectile.ai[0]];
                Vector2 targetPos = new Vector2(p.Center.X, p.Center.Y);
                //Vector2 targetPos = new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition;

                float speed = 3f;
                float velocityXModify = targetPos.X - projectile.Center.X;
                float velocityYModify = targetPos.Y - projectile.Center.Y;
                float veloRatio = (float)Math.Sqrt((double)(velocityXModify * velocityXModify + velocityYModify * velocityYModify));
                veloRatio = speed / veloRatio;
                velocityXModify *= veloRatio;
                velocityYModify *= veloRatio;
                if (projectile.type == 297)
                {
                    projectile.velocity.X = (projectile.velocity.X * 20f + velocityXModify) / 21f;
                    projectile.velocity.Y = (projectile.velocity.Y * 20f + velocityYModify) / 21f;
                    return;
                }
                projectile.velocity.X = (projectile.velocity.X * 30f + velocityXModify) / 31f;
                projectile.velocity.Y = (projectile.velocity.Y * 30f + velocityYModify) / 31f;

                // When near the player
                if (Math.Abs(projectile.Center.X - targetPos.X) < Player.defaultWidth &&
                    Math.Abs(projectile.Center.Y - targetPos.Y) < Player.defaultWidth)
                {
                    projectile.Kill();
                }



                int d = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y),
                    projectile.width, projectile.height, 175, projectile.velocity.X * 2, projectile.velocity.Y * 2, 100, Color.Cyan, 1.6f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.5f;
            }
            catch { }
        }

        public override void Kill(int timeLeft)
        {
            try
            {
                Player p = Main.player[(int)projectile.ai[0]];
                p.statMana += (int)projectile.ai[1];
                p.AddBuff(BuffID.MagicPower, 30);
            }
            catch { }
        }
    }
}
