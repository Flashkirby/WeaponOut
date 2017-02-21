using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;

namespace WeaponOut.Projectiles
{
    public class CrystalVileLash : ModProjectile
    {
        public const float whipLength = 40f; //Long enough for spazmatism
        public const bool whipSoftSound = false;
        public const int handleHeight = 24;
        public const int chainHeight = 14;
        public const int partHeight = 14;
        public const int tipHeight = 16;
        public override void SetDefaults()
        {
            projectile.name = "Crystal Vilelash";
            projectile.width = 18;
            projectile.height = 18;
            projectile.scale = 1.2f;
            projectile.alpha = 255;
            projectile.aiStyle = 75;
            projectile.penetrate = -1;

            projectile.alpha = 0;
            projectile.hide = true;
            projectile.extraUpdates = 3;
            projectile.light = 0.3f;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Vector2 endPos = BaseWhip.WhipAI(projectile, whipLength);
            //Dust along projectile
            Dust dust2 = Main.dust[Dust.NewDust(endPos, projectile.width, projectile.height, Main.rand.Next(68, 71), 0f, 0f, 200, default(Color), 1.3f)];
            dust2.noGravity = true;
            dust2.velocity += projectile.localAI[0].ToRotationVector2();

            Lighting.AddLight(endPos, projectile.light, projectile.light * 0.3f, projectile.light);
        }

        #region BaseWhip Stuff
        
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            BaseWhip.ModifyHitAny(projectile, ref damage, ref knockback, ref crit, true);
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
            return BaseWhip.PreDraw(projectile, handleHeight, chainHeight, partHeight, tipHeight, 12, true);
        }
        
        #endregion
    
    }
}
