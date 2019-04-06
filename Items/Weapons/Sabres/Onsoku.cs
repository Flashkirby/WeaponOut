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

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.useTime = 140;
            item.useAnimation = 24;

            item.shoot = mod.ProjectileType<OnsokuSlash>();
            item.shootSpeed = 16f;

            item.rare = 4;
            item.value = Item.sellPrice(0, 0, 50, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UnicornHorn, 1);
            recipe.AddIngredient(ItemID.SoulofLight, 25);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            // Reset style to swing when neutral
            if (player.itemAnimation == 0)
            {
                item.useStyle = 1;
            }
            if (player.itemTime > 0)
            {
                if (player.itemTime == 1) PlayerFX.ItemFlashFX(player, 175);
                if (player.velocity.Y == 0)
                {
                    for (int i = 0; i < 3; i++) // 3 extra recharge speed
                    {
                        if (player.itemTime > 0) player.itemTime--;
                        if (player.itemTime == 1) PlayerFX.ItemFlashFX(player, 175);
                    }
                }
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.itemAnimation == player.itemAnimationMax - 1)
            {
                return true;
            }
            player.itemTime = 0; //don't try otherwise
            return false;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            foreach (Projectile p in Main.projectile)
            {
                if (p.active && p.owner == player.whoAmI && p.type == item.shoot)
                {
                    // Dash with self as hitbox, only when invincible via projectile
                    noHitbox = !player.immuneNoBlink;
                    if (!noHitbox)
                    {
                        Main.SetCameraLerp(0.1f, 10);
                        player.attackCD = 0;
                    }
                    hitbox = player.getRect();
                }
            }
        }
    }

    public class OnsokuSlash : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public override void SetDefaults()
        {
            projectile.melee = true;
            projectile.width = Player.defaultWidth;
            projectile.height = Player.defaultHeight;

            projectile.penetrate = -1;
        }

        public float UpdateCount { get { return projectile.ai[0]; } set { projectile.ai[0] = value; } }
        public float DashCount { get { return projectile.ai[0] - 20; } }
        public Vector2 dashStep;
        public const float dashStepCount = 6;
        public const float dashStepDelay = 8;

        bool playedLocalSound = false;
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (player.dead || !player.active)
            {
                projectile.timeLeft = 0;
                return;
            }

            // Get dash location
            if (UpdateCount == 0)
            {
                for (int i = 0; i < dashStepCount * 8; i++)
                {
                    Vector2 move = Collision.TileCollision(
                        projectile.position, projectile.velocity / 2,
                        projectile.width, projectile.height,
                        true, true, (int)player.gravDir);
                    if (move == Vector2.Zero) break;
                    projectile.position += move / 2;
                }
                dashStep = (projectile.Center - player.Center) / dashStepCount;


                // dash dust
                for (int i = 0; i < dashStepCount; i++)
                {
                    Vector2 pos = player.Center + (dashStep * i) - new Vector2(4, 4);
                    for (int j = 0; j < 5; j++)
                    {
                        pos += dashStep * (j / 5f);
                        Dust d = Main.dust[Dust.NewDust(pos, 0, 0,
                            175, projectile.velocity.X, projectile.velocity.Y,
                            0, Color.White, 1f)];
                        d.noGravity = true;
                        d.velocity *= 0.05f;
                    }
                }
                projectile.velocity = Vector2.Zero;
            }

            // Dash towards location
            if (UpdateCount >= dashStepDelay)
            {
                if (UpdateCount == dashStepDelay)
                {
                    if (!playedLocalSound)
                    {
                        Main.PlaySound(2, player.Center, 28);
                        playedLocalSound = true; // Stop multiplayer sound bug
                    }

                    dashStep = (projectile.Center - player.Center) / dashStepCount;
                    player.inventory[player.selectedItem].useStyle = 3;
                }

                // freeze in swing
                player.itemAnimation = player.itemAnimationMax - 2;

                // dash, change position to influence camera lerp
                player.position += Collision.TileCollision(player.position,
                    dashStep / 2,
                    player.width,
                    player.height,
                    true, true, (int)player.gravDir);
                player.velocity = Collision.TileCollision(player.position,
                    dashStep * 0.8f,
                    player.width,
                    player.height,
                    true, true, (int)player.gravDir);

                // Set immunities
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 2);
                player.immuneNoBlink = true;
                player.fallStart = (int)(player.position.Y / 16f);
                player.fallStart2 = player.fallStart;

                //point in direction
                if (dashStep.X > 0) player.direction = 1;
                if (dashStep.X < 0) player.direction = -1;

                if (UpdateCount >= dashStepDelay + dashStepCount - 1)
                {
                    projectile.timeLeft = 0;
                }

                Vector2 pos = player.Center - new Vector2(4, 4);
                for (int i = 0; i < 10; i++)
                {
                    pos -= dashStep * (i / 20f);
                    Dust d = Main.dust[Dust.NewDust(pos, 0, 0,
                        181, projectile.velocity.X, projectile.velocity.Y,
                            0, default(Color), 1.3f)];
                    d.noGravity = true;
                    d.velocity *= 0.1f;
                }
            }
            else
            {
                // slow until move
                player.velocity *= 0.8f;
            }

            //Dust.NewDust(projectile.position, projectile.width, projectile.height, 20);

            UpdateCount++;
        }

        public override void Kill(int timeLeft)
        {
            Player player = Main.player[projectile.owner];
            player.velocity = dashStep / dashStepCount;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false; // slide not stop on tiles
        }
    }
}