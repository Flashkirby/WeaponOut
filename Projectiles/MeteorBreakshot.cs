using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Breaks on impact into multple MeteorBreakshatter
    /// </summary>
    public class MeteorBreakshot : ModProjectile
    {
        private const float bulletFadeTime = 10;

        public override void SetDefaults()
        {
            projectile.name = "Meteoric Breakshot";
            projectile.width = 4;
            projectile.height = 4;
            projectile.alpha = 255;

            projectile.timeLeft = 600;
            projectile.penetrate = 1;
            projectile.extraUpdates = 1;

            projectile.friendly = true;
            projectile.ranged = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            Lighting.AddLight(
                (int)((projectile.position.X + (float)(projectile.width / 2)) / 16f),
                (int)((projectile.position.Y + (float)(projectile.height / 2)) / 16f),
                0f,
                0.2f,
                0.9f
            );

            if (projectile.ai[0] < bulletFadeTime) projectile.ai[0]++;

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            ModifyHit(target);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            ModifyHit(target);
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            ModifyHit(target);
        }
        public void ModifyHit(Entity target)
        {
            Vector2 shatterPoint = projectile.Center;
            Vector2 normal = projectile.velocity; normal.Normalize();

            generateScatter(shatterPoint, projectile.velocity);
        }

        public override bool PreKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Fire)];
                d.scale *= 0.9f;
                d.velocity *= 3f;
                d.noGravity = true;
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 10);
            
            Vector2 shatterPoint = projectile.Center + projectile.velocity;
            float speed = projectile.oldVelocity.Length();
            Vector2 directionFactor = calculateDirectionFactor(projectile.velocity, oldVelocity);

            generateScatter(shatterPoint + directionFactor * 2, (speed * directionFactor));
            return true;
        }

        private void generateScatter(Vector2 position, Vector2 velocity)
        {
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectile(
                    position,
                    velocity * 0.25f + Inaccuracy(3f),
                    mod.ProjectileType<MeteorBreakshatter>(), projectile.damage / 3, 0f, projectile.owner);
            }
        }
        private Vector2 calculateDirectionFactor(Vector2 velocity, Vector2 oldVelocity)
        {
            Point p2t = (projectile.Center + velocity + Vector2.One / 2).ToTileCoordinates();
            Vector2 direction = new Vector2();
            try
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        Tile t = Main.tile[p2t.X + x, p2t.Y + y];
                        if (x == 0 && y == 0) continue;
                        if (t == null) continue;
                        if (t.collisionType > 0)
                        {
                            direction += new Vector2(-x, -y);
                        }
                    }
                }
            }
            catch { }
            direction.Normalize();
            if(direction.Equals(Vector2.Zero))
            {
                direction = -oldVelocity;
                direction.Normalize();
            }
            //Main.NewText("<final> direction = " + direction.X + ":" + direction.Y);
            return direction;
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

        public static Vector2 Inaccuracy(float multiplier)
        {
            return new Vector2(Main.rand.NextFloatDirection() * multiplier, Main.rand.NextFloatDirection() * multiplier);
        }
    }
}
