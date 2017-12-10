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
    public class SpiritThornBall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thorn Ball");
        }
        public override void SetDefaults()
        {
            projectile.width = 26;
            projectile.height = 26;

            projectile.penetrate = -1;
            projectile.timeLeft = 60;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            // So it doesn't conflict with fist NPC cooldown
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 6;
        }
        public override bool? CanCutTiles() { return false; }

        public float OrbitDistance = 96f;
        public float TravelDir { get { return projectile.ai[0]; } set { projectile.ai[0] = value; } }
        public float RotationToPlayer { get { return projectile.ai[1]; } set { projectile.ai[1] = value; } }
        public override void AI()
        {
            projectile.oldPosition = projectile.position; // Otherwise in this case, it's the player
            projectile.timeLeft++;
            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active || owner.dead) { projectile.timeLeft = 0; return; }
            if (owner.whoAmI == Main.myPlayer)
            {
                // Kill if no combo
                if (!owner.GetModPlayer<ModPlayerFists>().IsComboActive)
                {
                    if(projectile.penetrate < 0) projectile.penetrate = 1;
                    if (Main.netMode != 0)
                    {
                        projectile.netUpdate = true;
                    }
                }
            }

            Lighting.AddLight(projectile.Center, new Vector3(0.15f, 0.25f, 0f));

            // Change travel direction whilst swinging
            if (owner.itemAnimation > 0) TravelDir = owner.direction * owner.gravDir;

            // Point actual hit direction based on position
            if (projectile.Center.X > owner.Center.X)
            { projectile.direction = 1; }
            else
            { projectile.direction = -1; }

            RotationToPlayer += TravelDir * MathHelper.TwoPi / 60; // 1 Full rotation per second
            projectile.rotation += TravelDir * 0.2f;

            projectile.Center = owner.Center;

            projectile.velocity = OrbitDistance * new Vector2((float)Math.Sin(RotationToPlayer), (float)Math.Cos(RotationToPlayer));
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 600);

            // Send a healing projectile back to owner
            if (projectile.owner == Main.myPlayer)
            {
                Player owner = Main.player[projectile.owner];
                if (owner.lifeSteal <= 0f) return;
                float heal = 1f;
                if (projectile.penetrate >= 0) heal = damage / 10;
                owner.lifeSteal -= heal;
                Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, 0f, 0f, 298, 0, 0f, projectile.owner, (float)projectile.owner, heal);
            }
        }

        public override void Kill(int timeLeft)
        {
            for(int i = 0; i < 10; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 44, 0f, 0f, 150, default(Color), 0.5f);
            }
            Main.PlaySound(SoundID.Grass, projectile.position);
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
