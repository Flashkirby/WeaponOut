using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres
{
    public class JungleWoodSabre : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mahogany Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Красный Клинок");

            Tooltip.SetDefault("Charge Attack to lunge forwards");
            Tooltip.AddTranslation(GameCulture.Russian, "Заряд для рывка вперёд");
        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;

            item.melee = true;
            item.damage = 8; //DPS 27.5
            item.knockBack = 3.5f;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.useTime = 30 / 4;
            item.useAnimation = 22;

            item.rare = 0;
            item.value = 0;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.RichMahogany, 7);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, ModContent.ProjectileType<JungleWoodSabreSlash>(),
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
            {
                target.AddBuff(ModContent.BuffType<Buffs.Reversal>(), 60);
            }
        }
    }
    public class JungleWoodSabreSlash : ModProjectile
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

        Vector2? preDashVelocity;
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
                {
                    Main.PlaySound(SoundID.Item5.WithVolume(0.5f), projectile.Center); sndOnce = false;
                    preDashVelocity = player.velocity; // Save velocity before dash
                }
            }
            
            if (SlashLogic == 0)
            {
                float dashFrameDuration = 3;
                float dashSpeed = player.maxRunSpeed * 4f;
                int freezeFrame = 2;
                ModSabres.AIDashSlash(player, projectile, dashFrameDuration, dashSpeed, freezeFrame, ref preDashVelocity);
            }

            projectile.damage = 0;
            projectile.ai[0] += 1f; // Framerate
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = ModContent.ItemType<JungleWoodSabre>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                null,//SlashLogic == 0f ? specialSlash : null,
                lighting,
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }

    }
}
