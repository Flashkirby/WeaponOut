using System;
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
            DisplayName.SetDefault("Wooden Saber");
            
            Tooltip.SetDefault("Charge Attack grants 50% increased knockback");
        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;

            item.melee = true;
            item.damage = 7; //DPS 27.5
            item.knockBack = 3;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.useTime = 30 / 4;
            item.useAnimation = 24;

            item.rare = 0;
            item.value = 25000;
        }
        public override void AddRecipes()
        {
            //if (!ModConf.EnableSabres) return;
            //ModRecipe recipe = new ModRecipe(mod);
            //recipe.AddIngredient(ItemID.Wood, 7);
            //recipe.AddTile(TileID.WorkBenches);
            //recipe.SetResult(this, 1);
            //recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, mod.ProjectileType<WoodenSabreSlash>(),
                default(Color), 0.9f, player.itemTime == 0 ? 0f : 1f);
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
            int height = 62;
            int length = 68;
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(255, 89, 0, 119);
            ModSabres.OnHitFX(player, target, crit, colour);
        }
        
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (ModSabres.SabreIsChargedStriking(player, item))
            { knockBack *= 2f; }
        }
    }
    public class WoodenSabreSlash : ModProjectile
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
            projectile.width = 62;
            projectile.height = 68;
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
            int weaponItemID = mod.ItemType<WoodenSabre>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                null,//SlashLogic == 0f ? specialSlash : null,
                lighting,
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }

    }
}
