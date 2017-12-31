using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Basic
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

            Tooltip.SetDefault(
                "Dashes through enemies\n" +
                "Dash cooldown reduced on the ground");
            Tooltip.AddTranslation(GameCulture.Chinese, "挥动后玩家会冲刺到光标所指方向\n在地面时降低该武器冷却时间");
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

            item.shoot = mod.ProjectileType<Projectiles.OnsokuSlash>();
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
}