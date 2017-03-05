using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Breaks off from MeteorBreakshot
    /// </summary>
    public class MeteorBreakshatter : ModProjectile
    {
        private const float bulletFadeTime = 10;

        public override void SetDefaults()
        {
            projectile.name = "Meteoric Breakshot";
            projectile.width = 4;
            projectile.height = 4;

            projectile.timeLeft = 30;
            projectile.penetrate = 2;
            projectile.extraUpdates = 2;

            projectile.friendly = true;
            projectile.ranged = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            float timeNormal = Math.Max(0f, Math.Min(1f,
                projectile.timeLeft / 30f
                ));

            Dust d = Main.dust[Dust.NewDust(projectile.position, 0, 0, DustID.Fire, projectile.velocity.X, projectile.velocity.Y)];
            d.scale *= 0.8f + 0.5f * timeNormal;
            d.velocity *= 0.25f;
            d.noGravity = true;

            Lighting.AddLight(
                (int)((projectile.position.X + (float)(projectile.width / 2)) / 16f),
                (int)((projectile.position.Y + (float)(projectile.height / 2)) / 16f),
                0.5f,
                0.1f,
                0f
            );
            
            projectile.rotation += projectile.velocity.X;
        }

        // No crits
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { crit = false; }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        { crit = false; }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        { crit = false; }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 10);
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 centre = new Vector2(projectile.width / 2f, projectile.height / 2f);
            spriteBatch.Draw(texture,
                projectile.position - Main.screenPosition + centre,
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                Color.White,
                projectile.rotation,
                centre,
                projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
