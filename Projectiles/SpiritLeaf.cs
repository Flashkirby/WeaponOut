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
    public class SpiritLeaf : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Leaf");
			DisplayName.AddTranslation(GameCulture.Russian, "Лист");
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;

            projectile.penetrate = 1;
            projectile.timeLeft = 600;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

        }
        public override bool? CanCutTiles() { return false; }

        public float OrbitDistance = 48;
        public float TravelDir { get { return projectile.ai[0]; } set { projectile.ai[0] = value; } }
        public float RotationToPlayer { get { return projectile.ai[1]; } set { projectile.ai[1] = value; } }
        public override void AI()
        {
            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active || owner.dead) { projectile.timeLeft = 0; return; }

            // Point actual hit direction based on position
            if (projectile.Center.X > owner.Center.X)
            { projectile.direction = 1; }
            else
            { projectile.direction = -1; }

            RotationToPlayer += -1f * TravelDir * MathHelper.TwoPi / 60; // 1 Full rotation per second

            projectile.Center = owner.Center;

            OrbitDistance = owner.height - 10;
            projectile.velocity = OrbitDistance * new Vector2((float)Math.Sin(RotationToPlayer), (float)Math.Cos(RotationToPlayer));

            Rectangle hitbox;
            foreach (Projectile p in Main.projectile)
            {
                if (projectile.timeLeft <= 0) return;
                if(p.active && p.hostile)
                {
                    for (int i = 1; i < p.MaxUpdates + 1; i++)
                    {
                        hitbox = p.Hitbox;
                        hitbox.X += (int)p.velocity.X * i;
                        hitbox.Y += (int)p.velocity.Y * i;
                        if (hitbox.Intersects(projectile.Hitbox))
                        {
                            if (ProjFX.ReflectProjectilePlayer(p, owner))
                            {
                                projectile.timeLeft = 0;
                                projectile.Kill();
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 600);
        }

        public override void Kill(int timeLeft)
        {
            for(int i = 0; i < 4; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 44, 0f, 0f, 150, default(Color), 1f);
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.GrassBlades, 0f, 0f, 0, default(Color), 1f);
            }
            Main.PlaySound(SoundID.Grass, projectile.position);

            Player owner = Main.player[projectile.owner];
            if (owner != null && owner.active && !owner.dead)
            {
                owner.immune = true;
                owner.immuneTime = 40;
                if (owner.longInvince)
                {
                    owner.immuneTime += 40;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor.R = (byte)Math.Max((byte)90, lightColor.R);
            lightColor.G = (byte)Math.Max((byte)120, lightColor.G);

            spriteBatch.Draw(Main.projectileTexture[projectile.type],
                projectile.position - Main.screenPosition + new Vector2(projectile.width / 2f, projectile.height / 2f),
                new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)),
                lightColor,
                projectile.rotation,
                new Vector2(projectile.width / 2f, projectile.height / 2f),
                projectile.scale,
                projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
