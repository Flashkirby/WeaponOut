using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    public class Explooosion : ModProjectile
    {
        private int ChargeLevel { get { return (int)projectile.ai[0]; } set { projectile.ai[0] = value; } }
        private int ExplosionState { get { return (int)projectile.ai[1]; } set { projectile.ai[1] = value; } }
        private int ChargeTick { get { return (int)projectile.localAI[0]; } set { projectile.localAI[0] = value; } }
        private int Damage { get { return (int)projectile.localAI[1]; } set { projectile.localAI[1] = value; } }

        public override void SetDefaults()
        {
            projectile.name = "Explosion"; //great explosion, mega explosion, extreme explosion, ultra explosion, grand explosion
            projectile.width = 100;
            projectile.height = 100;

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
            player.heldProj = projectile.whoAmI; //set to this
            fireState(player);
        }

        private void fireState(Player player)
        {
            if (ChargeTick >= 0)
            {
                ChargeTick = -1;
                projectile.timeLeft = Explosion.fireTicksTime * (1 + ChargeLevel);

                nameExplosion();

               //-// Main.NewText("newSize = " + projectile.width + " / charge = " + ChargeLevel);
                for (int i = 0; i < ChargeLevel; i++)
                {
                    projectile.scale *= Explosion.explosionScale;
                    //-//Main.NewText("testw/h = " + (projectile.width * projectile.scale) + " | scale: " + projectile.scale);
                }
                projectile.position += new Vector2(projectile.width, projectile.height) / 2;
                projectile.width = (int)(projectile.width * projectile.scale);
                projectile.height = (int)(projectile.height * projectile.scale);
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
            string prefix = "";
            if (ChargeLevel >= 2) prefix = "Large ";
            if (ChargeLevel >= 4) prefix = "Great ";
            if (ChargeLevel >= 6) prefix = "Mega ";
            if (ChargeLevel >= 8) prefix = "Ultra ";
            if (ChargeLevel >= 10) prefix = "Grand ";
            if (ChargeLevel >= 12) prefix = "Extreme ";
            if (ChargeLevel >= 14) prefix = "Ultra Extreme ";
            if (ChargeLevel >= 15) prefix = "Super Mega Ultra Extreme ";
            projectile.name = prefix + "Explo" + new String('o', ChargeLevel) + "sion";
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
            for (int i = 0; i < (ChargeLevel + 3) * 3; i++)
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
                int d = Dust.NewDust(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                    velocity.X * normal.X / log,
                    velocity.Y * normal.Y / log,
                    0, Color.White, 1f + ChargeLevel * 0.3f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.6f;
                //explosion OUTER
                d = Dust.NewDust(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                    velocity.X * ring / log,
                    velocity.Y * ring / log,
                    0, Color.White, 0.6f + ChargeLevel * 0.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.5f;

                if (i % 2 == 0)
                {
                    //explosion shockwave horizontal
                    d = Dust.NewDust(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                        velocity.X * ring * 1.5f / log,
                        velocity.Y * ring * 0.3f / log,
                        0, Color.White, 0.2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].fadeIn = 0.8f + ChargeLevel * 0.1f;
                    Main.dust[d].velocity *= 0.5f;

                    //explosion shockwave vertical
                    d = Dust.NewDust(projectile.Center - new Vector2(16, 16), 32, 32, 262,
                        velocity.X * ring * 0.2f / log,
                        velocity.Y * ring * 1.5f / log,
                        0, Color.White, 0.2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].fadeIn = 0.8f + ChargeLevel * 0.1f;
                    Main.dust[d].velocity *= 0.5f;
                }
            }

            try
            {
                float denominator = 1 + Vector2.DistanceSquared(Main.player[Main.myPlayer].Center, projectile.Center) / 500000;
                WeaponOut.shakeIntensity = Math.Max(WeaponOut.shakeIntensity, (int)(2 * ChargeLevel / denominator));
                //Main.NewText("shake = " + (int)(2 * ChargeLevel / denominator) + ": " + (2 * ChargeLevel) + "/" + denominator);
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
                int d = Dust.NewDust(projectile.position + new Vector2(projectile.width / 4, projectile.height / 4),
                    projectile.width / 2, projectile.height / 2,
                    31, 0f, 0f, 150, default(Color), 0.8f);
                Main.dust[d].fadeIn = 1f + ChargeLevel * 0.2f;
                Main.dust[d].velocity *= 0.8f;
            }
            int totalSmoke = 1 + ChargeLevel;
            for (int i = 0; i < totalSmoke; i++)
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

                int g = Gore.NewGore(projectile.Center - new Vector2(16, 16), velocity, Main.rand.Next(61, 64), 1f);
                Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
                Main.gore[g].velocity *= 0.1f;
                g = Gore.NewGore(projectile.Center - new Vector2(16, 16), velocity, Main.rand.Next(61, 64), 1f);
                Main.gore[g].scale *= 1 + 0.08f * ChargeLevel;
                Main.gore[g].velocity *= 0.05f;
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
                knockBack *= (projectile.width) / (projectile.width / 2 + dist * 2) * (6f + ChargeLevel * 0.3f);
                ////-//Main.NewText("knockback: " + knockBack);
                if (p.noKnockback) knockBack /= 2;
                p.velocity = (p.velocity + knockBack * 9) / 10;
            }
            foreach (NPC n in Main.npc)
            {
                if (!n.active || n.life == 0 || n.knockBackResist == 0) continue;
                float dist = n.Distance(projectile.Center);
                if (dist > projectile.width) continue;

                Vector2 knockBack = (n.Center - projectile.Center);
                knockBack.Normalize();
                knockBack *= (projectile.width) / (projectile.width / 2 + dist * 2) * (6f + ChargeLevel * 0.3f);
                //Main.NewText("knockback: " + projectile.knockBack);
                knockBack *= n.knockBackResist * (1 + projectile.knockBack / 3);
                n.velocity = (n.velocity + knockBack * 9) / 10;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit) { faceExplosion(target); }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) { faceExplosion(target); }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) { faceExplosion(target); }
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
