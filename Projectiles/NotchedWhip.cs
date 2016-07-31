using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;

namespace WeaponOut.Projectiles
{
    public class NotchedWhip : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.name = "Notched Whip";
            projectile.width = 8;
            projectile.height = 8;
            projectile.alpha = 255;
            projectile.aiStyle = 75;
            projectile.penetrate = -1;

            projectile.alpha = 0;
            projectile.hide = true;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.updatedNPCImmunity = true;
        }

        public override void AI()
        {
            //use localAI[1] to track hitting something
            if (projectile.ai[0] == 0f)
            {
                projectile.localAI[1] = Main.player[projectile.owner].itemAnimationMax;
            }
            AI_075(24f, (int)projectile.localAI[1]);
        }

        /// <summary>
        /// ai0:time out
        /// lai0:rotation
        /// </summary>
        private void AI_075(float swingLength, int swingTime)
        {
            Player player = Main.player[projectile.owner];
            float num = 1.57079637f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, true);

            //make visible quickly
            projectile.alpha -= 42;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            //set local AI to direction
            if (projectile.localAI[0] == 0f)
            {
                projectile.localAI[0] = projectile.velocity.ToRotation();
            }
            Vector2 vector25 = (projectile.ai[0] / swingTime * 6.28318548f - 1.57079637f).ToRotationVector2();
            vector25.Y *= (float)Math.Sin((double)projectile.ai[1]);
            if (projectile.ai[1] <= 0f)
            {
                vector25.Y *= -1f;
            }
            vector25 = vector25.RotatedBy((double)projectile.localAI[0], default(Vector2));
            projectile.ai[0] += 1f;
            if (projectile.ai[0] < swingTime)
            {
                projectile.velocity += swingLength * vector25;
            }
            else
            {
                projectile.Kill();
            }

            projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.itemTime = 5;
            player.itemAnimation = 5;
            player.itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));

            Vector2 vector34 = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
            if (player.direction != 1)
            {
                vector34.X = (float)player.bodyFrame.Width - vector34.X;
            }
            if (player.gravDir != 1f)
            {
                vector34.Y = (float)player.bodyFrame.Height - vector34.Y;
            }
            vector34 -= new Vector2((float)(player.bodyFrame.Width - player.width), (float)(player.bodyFrame.Height - 42)) / 2f;
            projectile.Center = player.RotatedRelativePoint(player.position + vector34, true) - projectile.velocity;

            
            //Dust effect at the end
            for (int num47 = 0; num47 < 2; num47++)
            {
                Dust dust = Main.dust[Dust.NewDust(projectile.position + projectile.velocity * 2f, projectile.width, projectile.height, 14, 0f, 0f, 200, Color.Transparent, 1.8f)];
                dust.noGravity = true;
                dust.velocity += projectile.localAI[0].ToRotationVector2();
                dust.fadeIn = 2f;
            }
            //Dust along projectile
            float num48 = 18f;
            int num49 = 0;
            while ((float)num49 < num48)
            {
                if (Main.rand.Next(4) == 0)
                {
                    Vector2 position = projectile.position + projectile.velocity + projectile.velocity * ((float)num49 / num48);
                    Dust dust2 = Main.dust[Dust.NewDust(position, projectile.width, projectile.height, 14, 0f, 0f, 100, Color.Transparent, 1f)];
                    dust2.noGravity = true;
                    dust2.fadeIn = 0.5f;
                    dust2.velocity += projectile.localAI[0].ToRotationVector2();
                }
                num49++;
            }

            Vector2 endPoint = projectile.position + projectile.velocity * 2f;
            Vector2 fakeVelo = endPoint - (projectile.oldPosition + projectile.oldVelocity * 2f);
            if (Collision.TileCollision(endPoint, fakeVelo, projectile.width, projectile.height, true, true) != fakeVelo)
            {
                if (projectile.ai[0] * 2 < projectile.localAI[1])
                {
                    projectile.localAI[1] = projectile.ai[0] * 2;
                    Main.PlaySound(2, endPoint, 39);
                    Collision.HitTiles(endPoint, fakeVelo, 8, 8);
                }
            }

            // Main.NewText("========================");
            // Main.NewText("ai0: " + projectile.ai[0]);// ==== 0 - 30
            // Main.NewText("ai1: " + projectile.ai[1]);// ==== 0
            // Main.NewText("lai0: " + projectile.localAI[0]);// ==== rotation 0 - 3.14
            // Main.NewText("lai1: " + projectile.localAI[1]);// ==== swingtime
            // Main.NewText("anim: " + player.itemAnimation);// ==== 2
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            //Main.NewText("tip hit : " + projectile.ai[0] + " | " + projectile.localAI[1]);
            if (projectile.ai[0] <= projectile.localAI[1] / 2 + projectile.localAI[1] / 16 &&
                projectile.ai[0] >= projectile.localAI[1] / 2 - projectile.localAI[1] / 16)
            {
                Player p = Main.player[projectile.owner];
                //Main.NewText("crit: " + p.inventory[p.selectedItem].crit + p.meleeCrit);
                damage = (int)(damage * (1 + (p.inventory[p.selectedItem].crit + p.meleeCrit) * 0.01f));
                knockback *= 3;
                crit = true;
            }
            else
            {
                crit = false;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (crit) { Main.PlaySound(2, target.Center, 40); }
            projectile.npcImmune[target.whoAmI] = 15;
            target.immune[projectile.owner] = 15;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            if (Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                projectile.Center, projectile.Center + projectile.velocity,
                projectile.width * projectile.scale, ref collisionPoint))
            {
                return true;
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 vector38 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
            Texture2D texture2D17 = Main.projectileTexture[projectile.type];
            Microsoft.Xna.Framework.Color alpha3 = projectile.GetAlpha(Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16));
            
            //define texture parts here
            Rectangle handle = new Rectangle(0, 0, texture2D17.Width, 24);
            Rectangle chain = new Rectangle(0, 24, texture2D17.Width, 12);
            Rectangle part = new Rectangle(0, 36, texture2D17.Width, 12);
            Rectangle tip = new Rectangle(0, 48, texture2D17.Width, 24);
            
            
            if (projectile.velocity == Vector2.Zero)
            {
                return false;
            }
            float num197 = projectile.velocity.Length() + 16f;
            bool flag21 = num197 < 100f;
            Vector2 value17 = Vector2.Normalize(projectile.velocity);
            Microsoft.Xna.Framework.Rectangle rectangle6 = handle;
            Vector2 value18 = new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            float rotation24 = projectile.rotation + 3.14159274f;
            Main.spriteBatch.Draw(texture2D17, projectile.Center.Floor() - Main.screenPosition + value18, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, rectangle6.Size() / 2f - Vector2.UnitY * 4f, projectile.scale, SpriteEffects.None, 0f);
            num197 -= 40f * projectile.scale;
            Vector2 vector39 = projectile.Center.Floor();
            vector39 += value17 * projectile.scale * handle.Height/2;
            Vector2 vector40;
            rectangle6 = part;
            if (num197 > 0f)
            {
                float num198 = 0f;
                while (num198 + 1f < num197)
                {
                    if (num197 - num198 < (float)rectangle6.Height)
                    {
                        rectangle6.Height = (int)(num197 - num198);
                    }
                    vector40 = vector39 + value18;
                    alpha3 = projectile.GetAlpha(Lighting.GetColor((int)vector40.X / 16, (int)vector40.Y / 16));
                    Main.spriteBatch.Draw(texture2D17, vector40 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, new Vector2((float)(rectangle6.Width / 2), 0f), projectile.scale, SpriteEffects.None, 0f);
                    num198 += (float)rectangle6.Height * projectile.scale;
                    vector39 += value17 * (float)rectangle6.Height * projectile.scale;
                }
            }
            Vector2 value19 = vector39;
            vector39 = projectile.Center.Floor();
            vector39 += value17 * projectile.scale * chain.Height / 2;
            rectangle6 = chain;
            int num199 = 18;
            if (flag21)
            {
                num199 = 9;
            }
            float num200 = num197;
            if (num197 > 0f)
            {
                float num201 = 0f;
                float num202 = num200 / (float)num199;
                num201 += num202 * 0.25f;
                vector39 += value17 * num202 * 0.25f;
                int num44;
                for (int num203 = 0; num203 < num199; num203 = num44 + 1)
                {
                    float num204 = num202;
                    if (num203 == 0)
                    {
                        num204 *= 0.75f;
                    }
                    vector40 = vector39 + value18;
                    alpha3 = projectile.GetAlpha(Lighting.GetColor((int)vector40.X / 16, (int)vector40.Y / 16));
                    Main.spriteBatch.Draw(texture2D17, vector40 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, new Vector2((float)(rectangle6.Width / 2), 0f), projectile.scale, SpriteEffects.None, 0f);
                    num201 += num204;
                    vector39 += value17 * num204;
                    num44 = num203;
                }
            }
            rectangle6 = tip;
            Vector2 vector41 = value19 + value18;
            alpha3 = projectile.GetAlpha(Lighting.GetColor((int)vector41.X / 16, (int)vector41.Y / 16));
            Main.spriteBatch.Draw(texture2D17, vector41 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle6), alpha3, rotation24, texture2D17.Frame(1, 1, 0, 0).Top(), projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
