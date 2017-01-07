using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class Hayauchi : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.name = "Hayauchi";
            projectile.width = 228;
            projectile.height = 142;
            projectile.timeLeft = 16;

            projectile.alpha = 255;
            Main.projFrames[projectile.type] = 9;
            
            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            projectile.position.X = (int)player.Center.X - 50 * player.direction;
            projectile.spriteDirection = player.direction;
            if (player.gravDir > 0) { projectile.position.Y = (int)player.Bottom.Y + 16 - projectile.height; }
            else { projectile.position.Y = (int)player.Top.Y - 16; }
            if (player.direction < 0) projectile.position.X -= projectile.width;
            
            projectile.damage = 0;
            float pow = projectile.timeLeft / 16f;
            Lighting.AddLight(new Vector2(projectile.Hitbox.Left, projectile.Center.Y),
                new Vector3(pow, pow * 0.2f, pow * 0.8f));
            Lighting.AddLight(new Vector2(projectile.Hitbox.Right, projectile.Center.Y),
                new Vector3(pow, pow * 0.2f, pow * 0.8f));
            Lighting.AddLight(new Vector2(projectile.Center.X, projectile.Hitbox.Top),
                new Vector3(pow, pow * 0.2f, pow * 0.8f));
            Lighting.AddLight(new Vector2(projectile.Center.X, projectile.Hitbox.Bottom),
                new Vector3(pow, pow * 0.2f, pow * 0.8f));
        }

        //Allows you to draw things behind this projectile. Returns false to stop the game from drawing extras textures related to the projectile (for example, the chains for grappling hooks), useful if you're manually drawing the extras. Returns true by default.
        public override bool PreDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft <= 16)
            {
                Player player = Main.player[projectile.owner];
                Point playerCentre = player.Center.ToTileCoordinates();

                lightColor = Lighting.GetColor(playerCentre.X, playerCentre.Y);

                projectile.alpha = 75;
                projectile.frame = 16 - (projectile.timeLeft * 2);
                return true;
            }
            return false;
        }
    }
}
