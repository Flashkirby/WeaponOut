using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Projectiles
{
    public class SpiritDragon : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Betsy's Rage");
            DisplayName.AddTranslation(GameCulture.Chinese, "贝蒂之怒");
			DisplayName.AddTranslation(GameCulture.Russian, "Ярость Бетси");
            Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            projectile.width = 104;
            projectile.height = 88;
            projectile.timeLeft = 30;

            projectile.alpha = 0;
            
            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (player == null || player.dead || !player.active) { projectile.timeLeft = 0; return; }

            if (projectile.timeLeft > 10)
            {
                if (player.dashDelay <= 0)
                {

                    projectile.timeLeft = player.itemAnimation - 3;
                }
                else
                {
                    projectile.timeLeft = 10;
                }
            }
            if (projectile.timeLeft <= 0) return;

            player.heldProj = projectile.whoAmI; // draw over part of player

            projectile.Center = player.Center;
            projectile.velocity = default(Vector2);
            projectile.oldVelocity = player.velocity;
            projectile.spriteDirection = player.direction;
            projectile.localAI[0] = player.gravDir;

            if(projectile.ai[0] == 0)
            {
                projectile.frame = 0;
                if (projectile.localAI[1] == 0)
                { Main.PlaySound(SoundID.DD2_BetsyFireballShot, projectile.position); projectile.localAI[1]++; }
            }
            if (projectile.ai[0] == 1)
            {
                projectile.frame = 0;
                if (projectile.localAI[1] == 0)
                { Main.PlaySound(SoundID.DD2_BetsyFlameBreath, projectile.position); projectile.localAI[1]++; }
            }
            else if (projectile.ai[0] == 2)
            {
                projectile.frame = 1;
                if (projectile.localAI[1] == 0)
                { Main.PlaySound(SoundID.DD2_WyvernDiveDown, projectile.position); projectile.localAI[1]++; }
            }
            if (projectile.timeLeft < 10)
            {
                projectile.alpha += 24;
            }

            Vector2 orient = new Vector2(projectile.spriteDirection, projectile.localAI[0]);
            Dust d;
            if (projectile.frame == 0)
            {
                Vector2 topWing = player.Center + new Vector2(-20, -32) * orient;
                Vector2 botWing = player.Center + new Vector2(32, -12) * orient;

                d = Dust.NewDustPerfect(topWing, 133);
                d.velocity = player.velocity / 2;
                d.noGravity = true;
                d.position -= d.velocity;
                d.alpha = projectile.alpha;
                d.scale *= projectile.Opacity;

                d = Dust.NewDustPerfect(botWing, 133);
                d.velocity = player.velocity / 2;
                d.noGravity = true;
                d.position -= d.velocity;
                d.alpha = projectile.alpha;
                d.scale *= projectile.Opacity;
            }
            else if (projectile.frame == 1)
            {
                Vector2 tiger = player.Center + new Vector2(30, 42) * orient;

                for (int i = 0; i < 2; i++)
                {
                    d = Dust.NewDustPerfect(tiger, 133);
                    if (i == 0)
                    {
                        d.velocity.X += player.velocity.X;
                    }
                    else
                    {
                        d.velocity.Y += player.velocity.Y;
                    }
                    d.noGravity = true;
                    d.scale *= projectile.Opacity;
                }
            }

            Lighting.AddLight(player.position + player.velocity, new Vector3(0.6f, 0.6f, 0.2f));
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D t = Main.projectileTexture[projectile.type];
            int frameHeight = t.Height / Main.projFrames[projectile.type];
            SpriteEffects effects = SpriteEffects.None;
            if (projectile.spriteDirection < 0) effects = SpriteEffects.FlipHorizontally;
            if (projectile.localAI[0] < 0) effects = effects | SpriteEffects.FlipVertically;
            Vector2 origin = new Vector2(t.Width / 2, frameHeight / 2);
            for (int i = 10; i >= 0; i--)
            {
                Vector2 drawPos = projectile.Center - Main.screenPosition - projectile.oldVelocity * i * 0.5f;
                float trailOpacity = projectile.Opacity - 0.2f * i;
                if (i != 0) trailOpacity /= 2f;
                if (trailOpacity > 0f)
                {
                    float colMod = 0.4f + 0.6f * trailOpacity;
                    spriteBatch.Draw(t,
                        drawPos.ToPoint().ToVector2(),
                        new Rectangle(0, frameHeight * projectile.frame, t.Width, frameHeight),
                        new Color(1f, 1f * colMod, 1f * colMod, 0.5f) * trailOpacity,
                        projectile.rotation,
                        origin,
                        projectile.scale * (1f + 0.05f * i),
                        effects,
                        0);
                }
            }
            return false;
        }
    }
}
