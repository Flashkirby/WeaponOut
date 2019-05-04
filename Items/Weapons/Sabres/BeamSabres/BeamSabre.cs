using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres.BeamSabres
{
    public abstract class BeamSabre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Beam Saber");

            Tooltip.SetDefault(
                "Charge Attack to power slash\n" +
                "'Made of pure light energy!'");
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

            item.rare = 5;
            item.value = 25000;
        }

        public abstract Color SabreColour();
        public abstract int SabreSlashType();

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, SabreSlashType(),
                SabreColour(), 0.75f, player.itemTime == 0 ? 0f : 1f);
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
            ModSabres.OnHitFX(player, target, crit, SabreColour(), true);
        }

        public override Color? GetAlpha(Color lightColor)
        { return Color.White; }
    }

    public abstract class BeamSabreSlash : ModProjectile
    {
        public static int specialProjFrames = 6;
        bool sndOnce = true;
        int chargeSlashDirection = 1;
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            projectile.width = 84;
            projectile.height = 64;
            projectile.aiStyle = -1;
            projectile.timeLeft = 60;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }
        public override bool? CanCutTiles() { return false; }
        public float FrameCheck
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }
        public int SlashLogic
        {
            get { return (int)projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public abstract Vector3 SabreColour();

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (ModSabres.AINormalSlash(projectile, SlashLogic))
            {
                Lighting.AddLight(projectile.Center, SabreColour() / 2f);
            }
            else
            {
                // Charged attack
                projectile.width = 164;
                ModSabres.AISetChargeSlashVariables(player, chargeSlashDirection);
                ModSabres.NormalSlash(projectile, player);

                // Play charged sound
                if (sndOnce)
                { Main.PlaySound(SoundID.Item60, projectile.Center); sndOnce = false; }

                Lighting.AddLight(projectile.Center, SabreColour());
            }
            projectile.damage = 0;
            FrameCheck += 0.75f; // Framerate
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            Texture2D specialSlash = mod.GetTexture("Items/Weapons/Sabres/BeamSabres/" + GetType().Name + "_Special");
            int weaponItemID = player.HeldItem.type;
            //Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, Color.White,
                SlashLogic == 0f ? specialSlash : null,
                new Color(1f, 1f, 1f, 0.1f), specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }

    }
}
