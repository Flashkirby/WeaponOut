using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Dual
{
    public class SteamPersuader : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Steam Persuader");
			DisplayName.AddTranslation(GameCulture.Russian, "Паровой Угнетатель");
            Tooltip.SetDefault(
                "No knockback on normal shots\n" +
                "Four round burst\n" +
                "Only the first shot consumes ammo\n" +
                "10% chance to not consume ammo\n" +
                "<right> to fire a spread shot");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Обычные выстрелы не отбрасывают\n" +
                "Залп из 4-х пуль\n" +
                "Только 1-й выстрел тратит боеприпасы\n" +
                "10% шанс не потратить боеприпасы\n" +
                "<right>, чтобы выстрелить вразброс");
        }
        public override void SetDefaults()
        {
            item.width = 62;
            item.height = 20;

            item.UseSound = SoundID.Item31;
            item.useStyle = 5;
            item.useAnimation = 12; //4 shots
            item.useTime = 3;
            item.reuseDelay = 14;//12 + 14 = 26 usetime
            item.autoReuse = true;

            item.ranged = true; //melee damage
            item.noMelee = true;
            item.damage = 21;
            item.knockBack = 8f;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = 10;
            item.shootSpeed = 9f;

            item.rare = 5;
            item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableDualWeapons) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ClockworkAssaultRifle, 1);
            recipe.AddIngredient(ItemID.Cog, 7);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        public override bool AltFunctionUse(Player player) { return true; }

        // Light weight, less flexible, but much safer
        public override bool CanUseItem(Player player)
        {
            float altAnim = 32f;
            float mainTime = 3f;
            if (PlayerFX.DualItemCanUseItemAlt(player, this,
                1f, 1f,
                // 32 + 9(extra shots) / 12, 3f/32f -> ~27
                (altAnim + 3f * mainTime) / (altAnim / mainTime), mainTime / altAnim))
            {
                item.UseSound = SoundID.Item38; // Doesn't play for other clients normally
                item.reuseDelay = 0;
                player.itemTime = 0; // gotta reset anytime we mess with item time divider
            }
            else
            {
                // we can take advantage of the fact that CanUseItem never gets called by
                // clients if it was an alt function
                item.UseSound = SoundID.Item31;
                item.reuseDelay = 14;
            }
            return true;
        }

        // Act like clockwork gun
        public override bool ConsumeAmmo(Player player)
        {
            if (player.itemAnimation < item.useAnimation - 1) { return false; } //only use first shot
            if (Main.rand.Next(10) == 0) { return false; } // if number is 0, don't use ammo also
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            //Main.NewText("time = " + player.itemTime);
            //Main.NewText("anim = " + player.itemAnimation);
            if (player.altFunctionUse == 0 || player.itemAnimation < player.itemAnimationMax - 1)
            {
                speedX += 0.5f * (Main.rand.NextFloat() - 0.5f);
                speedY += 0.5f * (Main.rand.NextFloat() - 0.5f);
                knockBack = 0f;
                return true;
            }
            else if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                speedX *= 6 / 9f;
                speedY *= 6 / 9f;

                float veloX, veloY;
                int numShots = Main.rand.Next(4, 7); // 4-6 shots
                for (int i = 0; i < numShots; i++)
                {
                    veloX = speedX + 5f * (Main.rand.NextFloat() - 0.5f);
                    veloY = speedY + 5f * (Main.rand.NextFloat() - 0.5f);
                    Projectile.NewProjectile(position, new Vector2(veloX, veloY),
                        type, damage, knockBack, player.whoAmI);
                }
            }
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-22, 2);
        }
    }
}
