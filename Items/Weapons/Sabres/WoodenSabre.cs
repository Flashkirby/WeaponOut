using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres
{
    public class WoodenSabre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wooden Sabrr");
            
            Tooltip.SetDefault("It do things");
        }
        public override void SetDefaults()
        {
            item.width = 46;
            item.height = 46;

            item.melee = true;
            item.damage = 35; //DPS 105
            item.knockBack = 3;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.useTime = 60;
            item.useAnimation = 12;

            item.shoot = ProjectileID.DD2SquireSonicBoom;
            item.shootSpeed = 10f;

            item.rare = 5;
            item.value = 25000;
        }

        public override void HoldItem(Player player)
        {
            ModPlayerSabres.HoldItemManager(player, item, mod.ProjectileType<WoodenSabreSlash>());
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return ModPlayerSabres.IsChargedShot(player);
        }
        
        public override bool UseItemFrame(Player player)
        {
            ModPlayerSabres.UseItemFrame(player, 0.9f, item.isBeingGrabbed);
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            float delayStart = 0.9f;
            int height = 64;
            int length = 164;
            float magicHitNumber = 4;

            height = (int)(height * item.scale);
            length = (int)(length * item.scale);
            int backOffset = height / 2 - Player.defaultWidth / 2; // dist from centre to edge
            int dist = (length + Player.defaultWidth) - height; // total distance covered by the moving hitbox
            int startFrame = (int)(player.itemAnimationMax * delayStart);
            if (startFrame < magicHitNumber) startFrame = (int)magicHitNumber;
            int activeFrame = startFrame - player.itemAnimation;
            if (activeFrame >= 0 && activeFrame < magicHitNumber)
            {
                hitbox.Width = (int)(height * 1.416f);
                hitbox.Height = (int)(height * 1.416f);

                float invert = 0f;
                if (player.direction < 0) invert = MathHelper.Pi;
                hitbox.Location = new Point(
                    // centre, cos by 3rd dist x frame, with backoffset to pull forward
                   (int)(player.Center.X + System.Math.Cos(player.itemRotation + invert)
                   * (dist / magicHitNumber * activeFrame + backOffset)) - hitbox.Width / 2,
                   (int)(player.Center.Y + System.Math.Sin(player.itemRotation + invert)
                   * (dist / magicHitNumber * activeFrame + backOffset)) - hitbox.Height / 2);
            
                player.attackCD = 0;

                //for (int i = 0; i < 24; i++)
                //{ Dust.NewDust(hitbox.Location.ToVector2(), hitbox.Width, hitbox.Height, 6); }
            }
            else
            {
                hitbox = player.Hitbox;
                noHitbox = true;
            }
        }
    }
    public class WoodenSabreSlash : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wooden Saber Slash");
            Main.projFrames[projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            projectile.width = 164;
            projectile.height = 64;
            projectile.aiStyle = -1;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }

        public int FrameCheck
        {
            get { return (int)projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }
        public int SlashLogic
        {
            get { return (int)projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (SlashLogic == 1 || SlashLogic == -1) { NormalSlash(player); }

            projectile.damage = 0;
            projectile.ai[0] += 0.5f;
        }
        private void NormalSlash(Player player)
        {
            if(projectile.ai[0] == 0f)
            {
                projectile.spriteDirection = player.direction;
                projectile.rotation = (float)System.Math.Atan2(projectile.velocity.Y, projectile.velocity.X);

                // Centre the projectile on player
                projectile.Center = player.Center;
                if (player.direction < 0) projectile.position.X += projectile.width;

                // move to intended side, then pull back to player width
                Vector2 offset = new Vector2(
                   (float)System.Math.Cos(projectile.rotation) * ((projectile.width / 2) - Player.defaultWidth / projectile.scale),
                   (float)System.Math.Sin(projectile.rotation) * ((projectile.width / 2) - Player.defaultWidth / projectile.scale)
                    );
                projectile.position += offset;

                if (player.direction < 0) projectile.position.X -= projectile.width;
            }
            else
            {
                projectile.position -= projectile.velocity * 2;
            }

            projectile.frame = (int)projectile.ai[0];
            if (projectile.frame >= Main.projFrames[projectile.type])
            {
                projectile.timeLeft = 0;
            }

            projectile.scale = projectile.knockBack;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            Texture2D weapon = Main.itemTexture[mod.ItemType<WoodenSabre>()];
            Texture2D slash = Main.projectileTexture[projectile.type];

            // Flip Horziontally
            SpriteEffects spriteEffect = SpriteEffects.None;
            spriteEffect = SpriteEffects.None;
            if (projectile.spriteDirection < 0)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
            }

            // Flip Vertically
            float gravDir = player.gravDir * SlashLogic;
            if (gravDir <= 0)
            {
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                { spriteEffect = spriteEffect | SpriteEffects.FlipVertically; }
                else
                { spriteEffect = SpriteEffects.FlipVertically; }
            }

            spriteBatch.Draw(slash,
                projectile.Center - Main.screenPosition,
                slash.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame),
                lightColor,
                player.itemRotation,
                new Vector2(slash.Width / 2, slash.Height / (2 * Main.projFrames[projectile.type])),
                projectile.scale,
                spriteEffect,
                1f);
            return false;
        }
    }
}
