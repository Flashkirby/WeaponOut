using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres
{
    public class EnergySabre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Beam Saber");
            
            Tooltip.SetDefault("Charge attack swings further"
                            + "\n'Eye-catching and lethal'");
        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;

            item.melee = true;
            item.damage = 50; //DPS 150
            item.knockBack = 3;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item15;

            item.useTime = 60 / 4;
            item.useAnimation = 20;

            //item.shoot = ProjectileID.DD2SquireSonicBoom;
            //item.shootSpeed = 10f;

            item.rare = 3;
            item.value = 25000;
        }

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, mod.ProjectileType<EnergySabreSlash>(),
                Color.Purple, 0.9f, player.itemTime == 0 ? 0f : 1f);
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
            int height = 64;
            int length = 84;
            if (item.noGrabDelay > 0)
            {
                length = 164;
                player.meleeDamage += 0.5f;
            }
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(1f, 0.1f, 0f);
            ModSabres.OnHitFX(player, target, crit, colour, true);
        }

        public override Color? GetAlpha(Color lightColor)
        { return Color.White; }
    }
    public class EnergySabreSlash : ModProjectile
    {
        public static Texture2D specialSlash;
        public static int specialProjFrames = 6;
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
                projectile.width = 164;
                ModSabres.AISetChargeSlashVariables(player, chargeSlashDirection);
                ModSabres.NormalSlash(projectile, player);

                // Play charged sound
                if (sndOnce)
                { Main.PlaySound(SoundID.Item60, projectile.Center); sndOnce = false; }
            }
            projectile.damage = 0;
            projectile.ai[0] += 0.75f; // Framerate
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = mod.ItemType<EnergySabre>();
            //Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, Color.White,
                SlashLogic == 0f ? specialSlash : null,
                new Color(1f, 1f, 1f, 0.1f), specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }

    }
}
