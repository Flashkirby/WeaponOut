using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Sabres
{
    /// <summary>
    /// Dash around like some kind of... cyborg ninja
    /// </summary>
    public class Onsoku : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Onsoku");
           DisplayName.AddTranslation(GameCulture.Chinese, "音速");
			DisplayName.AddTranslation(GameCulture.Russian, "Онсоку");

           Tooltip.SetDefault(
                "Dashes through enemies\n" +
                "Dash cooldown reduced on the ground");
           Tooltip.AddTranslation(GameCulture.Chinese, "挥动后玩家会冲刺到光标所指方向\n在地面时降低该武器冷却时间");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Прорывается сквозь врагов\n" +
                "Время между рывками короче на земле");
        }
        public override void SetDefaults()
        {
            item.width = 40;
            item.height = 40;

            item.melee = true;
            item.damage = 35;
            item.knockBack = 1;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.useTime = 60 / 4;
            item.useAnimation = 24;

            item.rare = 4;
            item.value = Item.sellPrice(0, 0, 50, 0);
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UnicornHorn, 1);
            recipe.AddIngredient(ItemID.SoulofLight, 25);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, mod.ProjectileType<OnsokuSlash>(),
                Color.HotPink, 0.9f, player.itemTime == 0 ? 0f : 1f);
        }

        public override bool UseItemFrame(Player player)
        {
            ModSabres.UseItemFrame(player, 0.9f, item.isBeingGrabbed);
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int height = 98;
            int length = 100;
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(255, 180, 210, 119);
            ModSabres.OnHitFX(player, target, crit, colour);
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (ModSabres.SabreIsChargedStriking(player, item))
            {
                target.AddBuff(mod.BuffType<Buffs.Reversal>(), 60);
            }
        }
    }

    public class OnsokuSlash : ModProjectile
    {
        public const int specialProjFrames = 5;
        bool sndOnce = true;
        int chargeSlashDirection = 1;
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = specialProjFrames;
        }
        public override void SetDefaults()
        {
            projectile.width = 100;
            projectile.height = 100;
            projectile.aiStyle = -1;
            projectile.timeLeft = 60;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }

        public override bool? CanCutTiles() { return SlashLogic == 0; }
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

        Vector2 preDashVelocity;
        bool firstFrame = true;
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
                    Main.PlaySound(2, player.Center, 28); sndOnce = false;
                    preDashVelocity = player.velocity; // Save velocity before dash
                }
            }

            if (SlashLogic == 0)
            {
                float dashFrameDuration = 6;
                float dashSpeed = 32f;
                int freezeFrame = 2;
                bool dashing = ModSabres.AIDashSlash(player, projectile, dashFrameDuration, dashSpeed, freezeFrame, ref preDashVelocity);

                if (dashing)
                {
                    Dust.NewDust(player.Center - new Vector2(6, 6), 4, 4, 20);

                    // Coloured line trail
                    Vector2 dashStep = player.position - player.oldPosition;
                    for (int i = 0; i < 8; i++)
                    {
                        Dust d = Main.dust[Dust.NewDust(player.Center - (dashStep / 8) * i, 
                            0, 0, 181, dashStep.X / 32, dashStep.Y / 32, 0, default(Color), 1.3f)];
                        d.noGravity = true;
                        d.velocity *= 0.1f;
                    }
                }

                // Calculate ending position dust
                if (firstFrame)
                {
                    firstFrame = false;

                    Vector2 endPosition = player.position;
                    Vector2 dashVector = projectile.velocity * dashSpeed;
                    for (int i = 0; i < dashFrameDuration * 2; i++)
                    {
                        Vector2 move = Collision.TileCollision(
                            endPosition, dashVector / 2,
                            player.width, player.height,
                            false, false, (int)player.gravDir);
                        if (move == Vector2.Zero) break;
                        endPosition += move;
                    }

                    // dash dust from the total distance over the duration
                    Vector2 totalDistanceStep = 
                        (endPosition + new Vector2(player.width / 2, player.height / 2)
                        - player.Center) / dashFrameDuration;
                    for (int i = 0; i < dashFrameDuration; i++)
                    {
                        Vector2 pos = player.Center + (totalDistanceStep * i) - new Vector2(4, 4);
                        for (int j = 0; j < 5; j++)
                        {
                            pos += totalDistanceStep * (j / 5f);
                            Dust d = Main.dust[Dust.NewDust(pos, 0, 0,
                                175, projectile.velocity.X, projectile.velocity.Y,
                                0, Color.White, 1f)];
                            d.noGravity = true;
                            d.velocity *= 0.05f;
                        }
                    }
                }
            }

            projectile.damage = 0;
            FrameCheck += 1f; // Framerate
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = mod.ItemType<Onsoku>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                null,//SlashLogic == 0f ? specialSlash : null,
                lighting,
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic,
                SlashLogic == 0f);
        }

    }
}