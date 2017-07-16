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
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public const float whipLength = 24f;
        public const bool whipSoftSound = true;
        public const int handleHeight = 24;
        public const int chainHeight = 12;
        public const int partHeight = 12;
        public const int tipHeight = 12;
        public const bool doubleCritWindow = false;
        public const bool ignoreLighting = false;
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.scale = 0.9f;
            projectile.alpha = 255;
            projectile.aiStyle = 75;
            projectile.penetrate = -1;

            projectile.alpha = 0;
            projectile.hide = true;
            projectile.extraUpdates = 3;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Vector2 endPos = BaseWhip.WhipAI(projectile, whipLength);

            //Dust effect at the end
            if (projectile.ai[0] % 4 == 0)
            {
                Dust dust = Main.dust[Dust.NewDust(endPos, projectile.width, projectile.height, 18, 0f, 0f, 200, Color.Transparent, 1.2f)];
                dust.noGravity = true;
                dust.velocity += projectile.localAI[0].ToRotationVector2();
                dust.fadeIn = 2f;
            }
            //Dust along projectile
            for (int i = 0; i < 18; i++)
            {
                if (Main.rand.Next(16) == 0)
                {
                    Vector2 position = projectile.position + projectile.velocity + projectile.velocity * ((float)i / 18);
                    Dust dust2 = Main.dust[Dust.NewDust(position, projectile.width, projectile.height, 14, 0f, 0f, 100, Color.Transparent, 1f)];
                    dust2.noGravity = true;
                    dust2.fadeIn = 0.5f;
                    dust2.velocity += projectile.localAI[0].ToRotationVector2();
                }
            }
        }

        #region BaseWhip Stuff
        
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            BaseWhip.ModifyHitAny(projectile, ref damage, ref knockback, ref crit, doubleCritWindow);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            BaseWhip.ModifyHitAny(projectile, ref damage, ref crit);
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            BaseWhip.ModifyHitAny(projectile, ref damage, ref crit);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            BaseWhip.OnHitAny(projectile, target, crit, whipSoftSound);
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            BaseWhip.OnHitAny(projectile, crit, whipSoftSound);
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            BaseWhip.OnHitAny(projectile, crit, whipSoftSound);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return BaseWhip.Colliding(projectile, targetHitbox);
        }

        public override bool? CanCutTiles()
        {
            return BaseWhip.CanCutTiles(projectile);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            return BaseWhip.PreDraw(projectile, handleHeight, chainHeight, partHeight, tipHeight, 10, ignoreLighting, doubleCritWindow);
        }
        
        #endregion
    
    }
}
