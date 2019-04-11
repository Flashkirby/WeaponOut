using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WeaponOut.Items.Weapons.Sabres
{
    /// <summary>
    /// Stand (mostly) still to charge a slash, messes with hitboxes etc.
    /// drawstrike does quad damage, with added crit for a total of x8
    /// 35 * 8 == 280
    /// Draw Strike speed = 80 + 20 + 15 == 115
    /// Draw Strike DPS = 146
    /// hey, its me, jetstream sammy
    /// </summary>
    public class Hayauchi : ModItem
    {
        public const int waitTime = 80; //charge for special attack, due to coding must be >60 since that's the charge time
        public const int sheenTime = 60; // duration of actual charge animation

        private bool drawStrike;
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hayauchi");
            DisplayName.AddTranslation(GameCulture.Chinese, "快打");
			DisplayName.AddTranslation(GameCulture.Russian, "Хаяуси");
            
            Tooltip.SetDefault(
                "'Focus, steel thyself'\n" +
                "'Wait for the perfect moment'\n" +
                "'A decisive blow'");
            Tooltip.AddTranslation(GameCulture.Chinese, "“聚集力量”\n“等待时机”\n“一击定音”");
			Tooltip.AddTranslation(GameCulture.Russian,
				"'Сосредоточься'\n" +
                "'Выбери момент'\n" +
                "'Точный удар'");
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

            item.useTime = waitTime;
            item.useAnimation = 20;

            item.rare = 5;
            item.value = Item.buyPrice(0, 02, 50, 00);
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.Katana, 1);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.CobaltBar, 6);
                }
                else
                {
                    recipe.AddIngredient(ItemID.PalladiumBar, 6);
                }
                recipe.AddTile(TileID.MythrilAnvil);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        // Define if the player is still enough to use the special
        public bool hasHayauchiSpecialCharge(Player player)
        {
            return player.itemTime == 0 
                && (player.position - player.oldPosition).Length() < 0.5f;
        }

        public override void HoldItem(Player player)
        {
            bool specialCharge = hasHayauchiSpecialCharge(player);

            ModSabres.HoldItemManager(player, item, mod.ProjectileType<HayauchiSlash>(),
                Color.Red, 0.9f, specialCharge ? 0f : 1f, customCharge);

            if (specialCharge)
            {
                Vector2 dustPos = player.Center;
                int random = Main.rand.Next(60);
                if (player.stealth > 0)
                {
                    dustPos.X += (
                        ((sheenTime / 2) - random) * 0.55f - 12)
                        * player.direction - 4;
                    dustPos.Y -= (
                        ((sheenTime / 2) - random) * 0.29f - 12)
                        * player.gravDir;

                    if (player.gravDir < 0) dustPos.Y -= 6;
                    int d = Dust.NewDust(dustPos, 1, 1, 90, 0f, 0f, 0, Color.White, 0.2f);
                    Main.dust[d].velocity = Vector2.Zero;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].fadeIn = 0.8f;
                }
            }
        }

        public Action<Player, bool> customCharge = CustomCharge;
        public static void CustomCharge(Player player, bool flashFrame)
        {
            int patience = 80 - player.itemTime;

            Vector2 dustPos = player.Center;
            if (flashFrame)//burst
            {
                dustPos = player.Center;
                dustPos.X += (
                    (sheenTime / 2) * 0.55f - 12)
                    * player.direction - 4;
                dustPos.Y -= (
                    (sheenTime / 2) * 0.29f - 12)
                    * player.gravDir;

                if (player.gravDir < 0) dustPos.Y -= 6;
                Main.PlaySound(25, player.position);
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(dustPos, player.direction, 1, 130, player.direction, 0f, 0, Color.White, 0.6f);
                }
            }
            if (patience > waitTime - sheenTime) //blade sheen
            {
                dustPos.X += (
                    (patience + (sheenTime / 2) - waitTime) * 0.55f - 12)
                    * player.direction - 4;
                dustPos.Y -= (
                    (patience + (sheenTime / 2) - waitTime) * 0.29f - 12)
                    * player.gravDir;

                if (player.gravDir < 0) dustPos.Y -= 6;

                int d = Dust.NewDust(dustPos, 1, 1, 71, 0f, 0f, 0, Color.White, 0.5f);
                Main.dust[d].velocity *= 5f / (patience + 6);
            }
        }
        
        public override bool HoldItemFrame(Player player) //called on player holding but not swinging
        {
            if (hasHayauchiSpecialCharge(player)) //ready to slash
            {
                player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                return true;
            }
            return false;
        }

        public override bool UseItemFrame(Player player)
        {
            ModSabres.UseItemFrame(player, 0.9f, item.isBeingGrabbed);
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int height = 76;
            int length = 70;
            if (item.noGrabDelay > 0)
            {
                length = 240; // 228 + 12 offset
                height = 116; // 140 - 24 offset
            }
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(1f, 0f, 0f);
            ModSabres.OnHitFX(player, target, crit, colour, true);
        }

        //x6 damage + crit to make up for terrible (but cool) usage
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (ModSabres.SabreIsChargedStriking(player, item))
            {
                damage *= 6;
                knockBack *= 2;
                if ((player.Center - target.Center).Length() > 70)
                { crit = true; }
            }
         
        }
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        {
            if (ModSabres.SabreIsChargedStriking(player, item))
            {
                damage *= 6;
                if ((player.Center - target.Center).Length() > 70)
                { crit = true; }
            }
        }
    }


    public class HayauchiSlash : ModProjectile
    {
        public static Texture2D specialSlash;
        public static int specialProjFrames = 6;
        bool sndOnce = true;
        int chargeSlashDirection = -1;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hayauchi");
            DisplayName.AddTranslation(GameCulture.Chinese, "快打");
            DisplayName.AddTranslation(GameCulture.Russian, "Хаяуси");
            Main.projFrames[projectile.type] = 5;
            if (Main.netMode == 2) return;
            specialSlash = mod.GetTexture("Items/Weapons/Sabres/" + GetType().Name + "_Special");
        }
        public override void SetDefaults()
        {
            projectile.width = 70;
            projectile.height = 76;
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
        public float SlashLogic
        {
            get { return (int)projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (ModSabres.AINormalSlash(projectile, SlashLogic))
            {
                FrameCheck += 1f;
            }
            else
            {
                // Charged attack
                projectile.height = 140;
                projectile.width = 228;
                ModSabres.AISetChargeSlashVariables(player, chargeSlashDirection);
                ModSabres.NormalSlash(projectile, player);

                // Play charged sound
                if (sndOnce)
                {
                    Main.PlaySound(SoundID.Item71, projectile.Center); sndOnce = false;
                }

                float pow = (specialProjFrames - SlashLogic) / 16f;
                Lighting.AddLight(new Vector2(projectile.Center.X + 70, projectile.Center.Y),
                    new Vector3(pow, pow * 0.2f, pow * 0.8f));
                Lighting.AddLight(new Vector2(projectile.Center.X - 70, projectile.Center.Y),
                    new Vector3(pow, pow * 0.2f, pow * 0.8f));
                Lighting.AddLight(new Vector2(projectile.Center.X, projectile.Center.Y + 70),
                    new Vector3(pow, pow * 0.2f, pow * 0.8f));
                Lighting.AddLight(new Vector2(projectile.Center.X, projectile.Center.Y - 70),
                    new Vector3(pow, pow * 0.2f, pow * 0.8f));

                FrameCheck += 0.5f;
            }
            projectile.damage = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = mod.ItemType<Hayauchi>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                SlashLogic == 0f ? specialSlash : null,
                SlashLogic == 0f ? new Color(1f, 1f, 1f, 0.1f) : lighting,
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic);
        }
    }
}
