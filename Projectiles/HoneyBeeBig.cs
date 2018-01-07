using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class HoneyBeeBig : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Honey Bee");
			DisplayName.AddTranslation(GameCulture.Russian, "Медовая Пчела");
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            //projectile.aiStyle = 36;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.alpha = 255;
            projectile.timeLeft = 660;
            projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            HoneyBee.HoneyBeeMovement(projectile, true);
            HoneyBee.HealTarget(projectile, 3);
            HoneyBee.ManageFrames(projectile);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.velocity.X == 0) projectile.velocity.X = -oldVelocity.X;
            if (projectile.velocity.Y == 0) projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override void Kill(int timeLeft)
        {
            int dustID = 150;
            if (projectile.timeLeft > 0) dustID = DustID.t_Honey;
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height,
                    dustID, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1f);
                d.noGravity = true;
                d.scale = 1f;
            }
        }
    }
}
