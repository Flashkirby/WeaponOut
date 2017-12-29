using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Graphics.Effects;

namespace WeaponOut.Projectiles
{
    public class Explooosion : ModProjectile
    {

        private int ChargeLevel { get { return (int)projectile.ai[0]; } set { projectile.ai[0] = value; } }
        private int ExplodeSize { get { return (int)projectile.ai[1]; } set { projectile.ai[1] = value; } }
        private int ChargeTick { get { return (int)projectile.localAI[0]; } set { projectile.localAI[0] = value; } }
        private int damage { get { return (int)projectile.localAI[1]; } set { projectile.localAI[1] = value; } }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }
        public override void SetDefaults()
        {
            projectile.width = Explosion.explosionSize;
            projectile.height = Explosion.explosionSize;

            projectile.timeLeft = 3600;
            projectile.penetrate = -1;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            fireState(player);
        }
        public override void Kill(int timeLeft)
        {
            explosionEnd();
        }

        private void fireState(Player player)
        {
            if (ChargeTick >= 0)
            {
                ChargeTick = -1;
                projectile.timeLeft = Explosion.fireTicksTime * (1 + ChargeLevel);

                nameExplosion();

                // Resize, recentre
                projectile.position += new Vector2(projectile.width, projectile.height) / 2;
                projectile.width = (int)(ExplodeSize);
                projectile.height = (int)(ExplodeSize);
                projectile.position -= new Vector2(projectile.width, projectile.height) / 2;

                explosionStart();
            }
            pushAway();

            //projectile.scale += (Explosion.explosionScale - 1) / Explosion.fireTicksTime;

            explosionFX(
                projectile.timeLeft /
                (float)(Explosion.fireTicksTime * (1 + ChargeLevel))
                );
        }

        private void nameExplosion()
        {
            if (Terraria.Localization.Language.ActiveCulture != Terraria.Localization.GameCulture.English) return;

            string prefix = "";
            if (ChargeLevel >= 2) prefix = "Large ";
            if (ChargeLevel >= 4) prefix = "Great ";
            if (ChargeLevel >= 6) prefix = "Mega ";
            if (ChargeLevel >= 8) prefix = "Ultra ";
            if (ChargeLevel >= 10) prefix = "Grand ";
            if (ChargeLevel >= 12) prefix = "Extreme ";
            if (ChargeLevel >= 14) prefix = "Ultra Extreme ";
            if (ChargeLevel >= 15) prefix = "Super Mega Ultra Extreme ";
            projectile.Name = prefix + "Explo\n" + new String('o', ChargeLevel) + "sion";
            //Main.NewText(projectile.name);
        }

        public void explosionStart()
        {
            //Always make it sound closer
            Vector2 soundPoint = (Main.player[Main.myPlayer].position + projectile.Center * 2) / 3;

            Main.PlaySound(2, soundPoint, 14); //default explosion 
            if (ChargeLevel >= 2 && ChargeLevel < 4) Main.PlaySound(2, soundPoint, 20); //fire cast noise
            if (ChargeLevel >= 4) Main.PlaySound(2, soundPoint, 45); //inferno fork noise
            if (ChargeLevel >= 5) Main.PlaySound(2, soundPoint, 62); //grenade explosion(sounds beefier)
            if (ChargeLevel >= 7 && ChargeLevel < 9) Main.PlaySound(2, soundPoint, 69); //add staff of earth why not
            if (ChargeLevel >= 9) Main.PlaySound(2, soundPoint, 74); //inferno explosion
            if (ChargeLevel >= 10) Main.PlaySound(4, soundPoint, 43); //death explosion of some kind
            if (ChargeLevel >= 12) Main.PlaySound(2, soundPoint, 122); //roar lol
            if (ChargeLevel >= 14) Main.PlaySound(2, soundPoint, 119); //roar lol
        }
        public void explosionFX(float normalTime)
        {
            //explosion ball dust indicates size
            for (int i = 0; i < (ChargeLevel + 3) * 3 * normalTime; i++)
            {
                Vector2 velocity = new Vector2(
                    Main.rand.Next(-projectile.width, projectile.width + 1),
                    Main.rand.Next(-projectile.height, projectile.height + 1));

                //make into a circle
                Vector2 normal = new Vector2(velocity.X * 0.5f, velocity.Y * 0.5f);
                normal.Normalize();
                normal.X = Math.Abs(normal.X);
                normal.Y = Math.Abs(normal.Y);
                //make dust move distance of projectile size
                float log = (float)Math.Log((double)(velocity.X * velocity.X + velocity.Y * velocity.Y));

                //make into a ring
                float ring = projectile.width / velocity.Length();

                //explosion INNER
                Dust d = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                    velocity.X * normal.X / log,
                    velocity.Y * normal.Y / log,
                    0, Color.White, 1f + ChargeLevel * 0.3f);
                d.noGravity = true;
                d.velocity *= 0.6f;
                //explosion OUTER
                d = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                    velocity.X * ring / log,
                    velocity.Y * ring / log,
                    0, Color.White, 0.6f + ChargeLevel * 0.1f);
                d.noGravity = true;
                d.velocity *= 0.5f;

                if (i % 2 == 0)
                {
                    //explosion shockwave horizontal
                    d = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                        velocity.X * ring * 1.5f / log,
                        velocity.Y * ring * 0.3f / log,
                        0, Color.White, 0.2f);
                    d.noGravity = true;
                    d.fadeIn = 0.4f + Main.rand.NextFloat() * 0.4f + ChargeLevel * 0.1f;
                    d.velocity *= 0.5f;

                    //explosion shockwave vertical
                    d = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                        velocity.X * ring * 0.2f / log,
                        velocity.Y * ring * 1.5f / log,
                        0, Color.White, 0.2f);
                    d.noGravity = true;
                    d.fadeIn = 0.4f + Main.rand.NextFloat() * 0.4f + ChargeLevel * 0.1f;
                    d.velocity *= 0.5f;
                }
            }

            try
            {
                float denominator = 1 + Vector2.DistanceSquared(Main.player[Main.myPlayer].Center, projectile.Center) / 500000;
                WeaponOut.shakeIntensity = Math.Max(WeaponOut.shakeIntensity, (int)(2 * ChargeLevel / denominator));
                //Main.NewText("shake = \n" + (int)(2 * ChargeLevel / denominator) + ": \n" + (2 * ChargeLevel) + "/\n" + denominator);
            }
            catch
            { //-//Main.NewText("ERROR IN SHAKING"); 
            }
        }
        public void explosionEnd()
        {
            //smoke dust
            for (int i = 0; i < 30 + ChargeLevel * 10; i++)
            {
                Dust d = Dust.NewDustDirect(projectile.position + new Vector2(projectile.width / 4, projectile.height / 4),
                    projectile.width / 2, projectile.height / 2,
                    31, 0f, 0f, 150, default(Color), 0.8f);
                d.fadeIn = 1f + ChargeLevel * 0.2f;
                d.velocity *= 5f;
            }
            
            int g = Gore.NewGore(projectile.Center - new Vector2(16, 16), 
                new Vector2(projectile.width, projectile.height), Main.rand.Next(61, 64), 1f);
            Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
            Main.gore[g].velocity *= 0.01f;

            g = Gore.NewGore(projectile.Center - new Vector2(16, 16), 
                new Vector2(projectile.width, -projectile.height), Main.rand.Next(61, 64), 1f);
            Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
            Main.gore[g].velocity *= 0.01f;

            g = Gore.NewGore(projectile.Center - new Vector2(16, 16), 
                new Vector2(-projectile.width, -projectile.height), Main.rand.Next(61, 64), 1f);
            Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
            Main.gore[g].velocity *= 0.01f;

            g = Gore.NewGore(projectile.Center - new Vector2(16, 16), 
                new Vector2(-projectile.width, projectile.height), Main.rand.Next(61, 64), 1f);
            Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
            Main.gore[g].velocity *= 0.01f;

            for (int i = 0; i < ChargeLevel - 3; i++)
            {
                Vector2 velocity = new Vector2(
                    Main.rand.Next(-projectile.width, projectile.width + 1),
                    Main.rand.Next(-projectile.height, projectile.height + 1));
                //make into a circle
                Vector2 normal = new Vector2(velocity.X * 0.5f, velocity.Y * 0.5f);
                normal.Normalize();
                normal.X = Math.Abs(normal.X);
                normal.Y = Math.Abs(normal.Y);
                velocity *= normal;
                //make dust move distance of projectile size
                float log = (float)Math.Log((double)(velocity.X * velocity.X + velocity.Y * velocity.Y));
                velocity /= log;

                g = Gore.NewGore(projectile.Center - new Vector2(16, 16), velocity, Main.rand.Next(61, 64), 1f);
                Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
                Main.gore[g].velocity *= 0.4f;
                g = Gore.NewGore(projectile.Center - new Vector2(16, 16), velocity, Main.rand.Next(61, 64), 1f);
                Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
                Main.gore[g].velocity *= 0.2f;
            }
        }

        private void pushAway()
        {
            foreach (Player p in Main.player)
            {
                if (!p.active || p.dead || p.webbed || p.stoned) continue;
                float dist = p.Distance(projectile.Center);
                if (dist > projectile.width) continue;

                Vector2 knockBack = (p.Center - projectile.Center);
                knockBack.Normalize();
                knockBack *= (projectile.width) / (projectile.width / 2 + dist * 2) * (6f + ChargeLevel * 0.5f);
                ////-//Main.NewText("knockback: \n" + knockBack);
                if (p.noKnockback)
                { p.velocity = (p.velocity + knockBack * 2) / 3; }
                else
                { p.velocity = (p.velocity + knockBack * 9) / 10; }
            }
            foreach (NPC n in Main.npc)
            {
                if (!n.active || n.life <= 0 || (n.realLife >= 0 && n.realLife != n.whoAmI) || (n.knockBackResist == 0 && n.velocity == default(Vector2))) continue;
                float dist = n.Distance(projectile.Center);
                if (dist > projectile.width) continue;

                Vector2 knockBack = (n.Center - projectile.Center);
                knockBack.Normalize();
                knockBack *= (projectile.width) / (projectile.width / 2 + dist * 2) * (6f + ChargeLevel * 0.5f);
                //Main.NewText("knockback: \n" + projectile.knockBack);
                knockBack *= (n.knockBackResist * 0.8f + 0.1f) * (1 + projectile.knockBack / 3);

                if((n.realLife >= 0 && n.realLife != n.whoAmI))
                {
                    // Only push worms if velocity is greater
                    if (knockBack.Length() > n.velocity.Length()) n.velocity = knockBack;
                }
                else
                {
                    // Push away
                    n.velocity = (n.velocity + knockBack * 9) / 10;
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            faceExplosion(target);
            target.AddBuff(BuffID.OnFire, 600, false);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            faceExplosion(target);
            target.AddBuff(BuffID.OnFire, 600, false);
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            faceExplosion(target);
            target.AddBuff(BuffID.OnFire, 600, false);
        }
        private void faceExplosion(Entity entity)
        {
            if (entity.Center.X < projectile.Center.X)
            {
                projectile.direction = -1;
            }
            else
            {
                projectile.direction = 1;
            }
        }
    }
}
