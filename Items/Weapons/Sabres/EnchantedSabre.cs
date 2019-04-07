using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres
{
    public class EnchantedSabre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Saber");
            
            Tooltip.SetDefault("Charge attack increases range");
        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;

            item.melee = true;
            item.damage = 25; //DPS 75
            item.knockBack = 3;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.useTime = 30 / 4;
            item.useAnimation = 18;

            item.shoot = mod.ProjectileType<EnchantedSabreBeam>();
            item.shootSpeed = 16f;

            item.rare = 2;
            item.value = 25000;
        }

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, mod.ProjectileType<EnchantedSabreSlash>(),
                Color.Yellow, 0.9f, player.itemTime == 0 ? 0f : 1f);
        }

        // Doesn't get called unless item.shoot is defined.
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        { return ModSabres.IsChargedShot(player); }

        public override bool UseItemFrame(Player player)
        {
            ModSabres.UseItemFrame(player, 0.9f, item.isBeingGrabbed);
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int height = 84;
            int length = 88;
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(0.8f, 0.4f, 1f);
            ModSabres.OnHitFX(player, target, crit, colour, true);
        }
    }
    public class EnchantedSabreSlash : ModProjectile
    {
        public static Texture2D specialSlash;
        public static int specialProjFrames = 5;
        bool sndOnce = true;
        int chargeSlashDirection = 1;
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 5;
            if (Main.netMode == 2) return;
            specialSlash = mod.GetTexture("Items/Weapons/Sabres/" + GetType().Name + "_Special");
        }
        public override void SetDefaults()
        {
            projectile.width = 88;
            projectile.height = 84;
            projectile.aiStyle = -1;
            projectile.timeLeft = 60;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }
        public override bool? CanCutTiles() { return false; }
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
            if (ModSabres.AINormalSlash(projectile, SlashLogic)) { }
            else
            {
                // Charged attack
                ModSabres.AISetChargeSlashVariables(player, chargeSlashDirection);
                ModSabres.NormalSlash(projectile, player);
            }
            projectile.damage = 0;
            projectile.ai[0] += 1f; // Framerate
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = mod.ItemType<EnchantedSabre>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                SlashLogic == 0f ? specialSlash : null,
                SlashLogic == 0f ? new Color(1f, 1f, 1f, 0.1f) : lighting, 
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }

    }
    public class EnchantedSabreBeam : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;

            projectile.penetrate = 1;

            projectile.friendly = true;
            projectile.melee = true;
        }
        public override void AI()
        {
            if(projectile.ai[0] == 0)
            {
                Main.PlaySound(SoundID.Item8, projectile.Center);
                projectile.rotation = -MathHelper.PiOver2 +
                    (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X);
            }
            
            projectile.rotation += 0.8f * projectile.direction;

            if (projectile.ai[0] > 3)
            {
                // Enchanted Dust
                int dustID = 15;
                switch (Main.rand.Next(3)) { case 1: dustID = 57; break; case 2: dustID = 58; break; }

                Vector2 pos = projectile.Center + 16f * new Vector2(
                    (float)Math.Cos(projectile.rotation + projectile.direction * 2.9f),
                    (float)Math.Sin(projectile.rotation + projectile.direction * 2.9f));
                Dust.NewDustPerfect(pos, dustID, projectile.velocity / 16, 150, default(Color), 1.2f);

                pos = projectile.Center + 16f * new Vector2(
                    (float)Math.Cos(projectile.rotation + projectile.direction * 3.3f),
                    (float)Math.Sin(projectile.rotation + projectile.direction * 3.3f));
                Dust d = Dust.NewDustPerfect(pos, dustID, projectile.velocity / 8, 200, default(Color), 1f);
                d.noGravity = true;
            }

            projectile.frame++;
            if(projectile.frame >= Main.projFrames[projectile.type])
            { projectile.frame = 0; }

            projectile.ai[0]++;
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item10, projectile.Center);
            Vector2 vel = projectile.position - projectile.oldPosition;
            for (int i = 0; i < 20; i++)
            {
                int dustID = 15;
                switch (Main.rand.Next(3)) { case 1: dustID = 57; break; case 2: dustID = 58; break; }
                int d = Dust.NewDust(projectile.Center, 2, 2, dustID,
                    vel.X / 40 * i, vel.Y / 40 * i, 150, default(Color), 1.5f);
                Main.dust[d].noGravity = true;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 6; height = 6;
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(1f, 1f, 1f, 0.6f);
        }
    }
}
