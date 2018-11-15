using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres
{
    public class BorealWoodSabre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boreal Saber");
            
            Tooltip.SetDefault("Charge attack delivers a double strike");
        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;

            item.melee = true;
            item.damage = 6; //DPS (1def) 10
            item.knockBack = 3f;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.useTime = 30;
            item.useAnimation = 25;

            item.rare = 0;
            item.value = 0;
        }
        public override void AddRecipes()
        {
            //if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BorealWood, 7);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (ModSabres.HoldItemManager(player, item, mod.ProjectileType<BorealWoodSabreSlash>(),
                 45, 0.9f, player.itemTime == 0 ? 0f : 1f))
            { player.AddBuff(mod.BuffType("SabreDance"), player.itemAnimationMax / 2); }
        }

        // Doesn't get called unless item.shoot is defined.
        //public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        //{ return ModSabres.IsChargedShot(player); }

        public override bool UseItemFrame(Player player)
        {
            ModSabres.UseItemFrame(player, 0.9f, item.isBeingGrabbed);
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int height = 70;
            int length = 72;
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(255, 89, 0, 119);
            ModSabres.OnHitFX(player, target, crit, colour);
        }
    }
    public class BorealWoodSabreSlash : ModProjectile
    {
        public static Texture2D specialSlash;
        public const int specialProjFrames = 5;
        bool sndOnce = true;
        int chargeSlashDirection = 1;
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = specialProjFrames;
            //if (Main.netMode == 2) return;
            //specialSlash = mod.GetTexture("Items/Weapons/Sabres/" + GetType().Name + "_Special");
        }
        public override void SetDefaults()
        {
            projectile.height = 70;
            projectile.width = 72;
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

                // Play charged sound
                if (sndOnce)
                { Main.PlaySound(SoundID.Item5.WithVolume(0.5f), projectile.Center); sndOnce = false; }
            }
            projectile.damage = 0;
            projectile.ai[0] += 1f; // Framerate
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = mod.ItemType<BorealWoodSabre>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                null,//SlashLogic == 0f ? specialSlash : null,
                lighting,
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }

    }
}
