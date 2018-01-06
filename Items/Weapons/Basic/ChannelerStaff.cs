﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Basic
{

    /// <summary>
    /// A team support item that doubles your mana to an ally
    /// </summary>
    public class ChannelerStaff : ModItem
    {
        const bool devTest = true;
        public static Vector2 mouse
        {
            get
            {
                return Main.MouseWorld;
            }
        }

        public static short customGlowMask = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Channeler Staff");
			DisplayName.AddTranslation(GameCulture.Russian, "Посох Шамана");
            Tooltip.SetDefault(
                "Greatly increases mana regen when held\n" +
                "Cast a mana restoring ray to players on your team\n" +
                "Ray also increases magic damage");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Значительно регенерирует ману, если держать в руке\n" +
                "Выпускает луч, восстанавливающий ману ваших союзников\n" +
                "Луч также увеличивает магический урон");
            customGlowMask = WeaponOut.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults()
        {
            item.width = 42;
            item.height = 42;

            item.magic = true; //magic damage
            item.melee = false;
            item.noMelee = true;
            item.damage = 4;
            item.knockBack = 0;

            item.mana = 8;
            item.shoot = mod.ProjectileType<Projectiles.ManaRestoreBeam>();
            item.shootSpeed = 2;

            item.useStyle = 5; //aim
            Item.staff[item.type] = true; //rotate weapon, as it is a staff though this removes glow
            item.UseSound = SoundID.Item9;
            item.useAnimation = 24;
            item.useTime = 4;
            item.autoReuse = true;

            item.glowMask = customGlowMask;
            item.rare = 3;
            item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SunplateBlock, 100);
            recipe.AddTile(TileID.SkyMill);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            //basic mana regen behaviour addon (increases at count>=120)
            if (player.manaRegenDelay <= 0)
            {
                player.manaRegenCount += 50;
            }

            Lighting.AddLight(player.Center, 0.1f, 0.3f, 0.3f);
            //Show link to team mate from cursor
            Player p = getPlayerTeamAtCursor(player);
            if (p != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dist = p.Center - mouse;
                    int d = Dust.NewDust(
                        mouse + dist * Main.rand.NextFloat(),
                        1, 1, 175, dist.X * 0.4f, dist.Y * 0.4f, 255, Main.teamColor[player.team], 0.75f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.1f;
                }
            }
        }

        public override void UseStyle(Player player)
        {
            PlayerFX.modifyPlayerItemLocation(player, -14, 0);
            Lighting.AddLight(player.Center, 0.2f, 0.6f, 0.6f);
        }

        public override bool CanUseItem(Player player)
        {
            return getPlayerTeamAtCursor(player) != null;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Player p = getPlayerTeamAtCursor(player);
            if (p != null)
            {
                Projectile.NewProjectile(position.X + speedX * 15, position.Y + speedY * 15,
                    speedX, speedY, type, damage, knockBack, Main.myPlayer, p.whoAmI);
            }
            return false;
        }
        private Player getPlayerTeamAtCursor(Player player)
        {
            if (player.whoAmI == Main.myPlayer && (player.team != 0 || devTest))
            {
                foreach (Player p in Main.player)
                {
                    if (!p.active || p.dead) continue;
                    if (p.team == player.team && (p.whoAmI != Main.myPlayer || devTest))
                    {
                        if (Math.Abs(mouse.X - p.Center.X) < 80 &&
                            Math.Abs(mouse.Y - p.Center.Y) < 80)
                        {
                            return p;
                        }
                    }
                }
            }
            return null;
        }
    }
}
