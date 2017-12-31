using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Projectiles
{
    public class HoneyBee : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Honey Bee");
            DisplayName.AddTranslation(GameCulture.Chinese, "蜜蜂");
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            //projectile.aiStyle = 36;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.alpha = 255;
            projectile.timeLeft = 600;
            projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            HoneyBee.HoneyBeeMovement(projectile);
            HoneyBee.HealTarget(projectile, 1);
            HoneyBee.ManageFrames(projectile);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.velocity.X == 0) projectile.velocity.X = -oldVelocity.X;
            if (projectile.velocity.Y == 0) projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override void Kill(int timeLeft)
        {
            int dustID = 150;
            if (projectile.timeLeft > 0) dustID = DustID.t_Honey;
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height,
                    dustID, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1f);
                d.noGravity = true;
                d.scale = 1f;
            }
        }

        #region Static Methods
        internal static void HoneyBeeMovement(Projectile projectile, bool bigBee = false)
        {
            projectile.damage = 0;

            // No water
            if (projectile.wet && !projectile.honeyWet)
            { projectile.Kill(); }

            // Appear through alpha
            if (projectile.alpha > 0) { projectile.alpha -= 50; }
            else { projectile.extraUpdates = 0; }
            projectile.alpha = Math.Max(0, projectile.alpha);

            // Find the target player's position (ai[0])
            Vector2 targetPos = projectile.Center;
            bool hasTarget = false;
            projectile.ai[1] += 1f;
            if (projectile.ai[1] > 30f)
            {
                projectile.ai[1] = 30f;
                if (Main.player[(int)projectile.ai[0]].active &&
                    !Main.player[(int)projectile.ai[0]].dead)
                {
                    targetPos = Main.player[(int)projectile.ai[0]].Center;
                    hasTarget = true;
                }
            }
            if (!hasTarget)
            {
                targetPos = projectile.Center + projectile.velocity * 100f;
            }

            // Set up movement direction
            float speed = 6f;
            float accel = 0.1f;
            if(bigBee)
            {
                speed = 6.8f;
                accel = 0.14f;
            }
            Vector2 targetVector = targetPos - projectile.Center;
            targetVector.Normalize();
            targetVector *= speed;

            // Speed up to right
            if(projectile.velocity.X < targetVector.X)
            {
                if (projectile.velocity.X < 0)
                { projectile.velocity.X += accel * 3; }
                else
                { projectile.velocity.X += accel; }
            }
            // Speed up to left
            if (projectile.velocity.X > targetVector.X)
            {
                if (projectile.velocity.X > 0)
                { projectile.velocity.X -= accel * 3; }
                else
                { projectile.velocity.X -= accel; }
            }
            // Speed up to down
            if (projectile.velocity.Y < targetVector.Y)
            {
                if (projectile.velocity.Y < 0)
                { projectile.velocity.Y += accel * 3; }
                else
                { projectile.velocity.Y += accel; }
            }
            // Speed up to up
            if (projectile.velocity.Y > targetVector.Y)
            {
                if (projectile.velocity.Y > 0)
                { projectile.velocity.Y -= accel * 3; }
                else
                { projectile.velocity.Y -= accel; }
            }
        }
        internal static void HealTarget(Projectile projectile, int healAmount)
        {
            if (projectile.ai[1] < 30) return;
            Player player = Main.player[(int)projectile.ai[0]];
            if (player.Hitbox.Intersects(projectile.Hitbox))
            {
                if (projectile.owner == Main.myPlayer && !Main.player[Main.myPlayer].moonLeech)
                {
                    Main.PlaySound(SoundID.LiquidsHoneyWater, player.position);
                    player.HealEffect(healAmount, false);
                    player.statLife += healAmount;
                    if (player.statLife > player.statLifeMax2)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                    // Tell target they got healed
                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, (int)projectile.ai[0], (float)healAmount, 0f, 0f, 0, 0, 0);

                    int bi = player.FindBuffIndex(BuffID.Honey);
                    if (bi >= 0)
                    {
                        player.AddBuff(BuffID.Honey, Math.Min(60 * 30, player.buffTime[bi] + 60 * healAmount), false);
                    }
                    else
                    {
                        player.AddBuff(BuffID.Honey, 90 * healAmount, false);
                    }
                }
                projectile.Kill();
            }
        }
        internal static void ManageFrames(Projectile projectile)
        {
            // Face direction
            if (projectile.velocity.X > 0f) { projectile.spriteDirection = 1; }
            else if (projectile.velocity.X < 0f) { projectile.spriteDirection = -1; }
            // Rotate
            projectile.rotation = projectile.velocity.X * 0.1f;
            // Manage Frame
            projectile.frameCounter++;
            if (projectile.frameCounter >= 3)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
            }
            if (projectile.frame >= 3)
            {
                projectile.frame = 0;
            }
        }
        #endregion
    }
}
