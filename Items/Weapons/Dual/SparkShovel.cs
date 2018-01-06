using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Dual
{
    public class SparkShovel : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spark Shovel");
			DisplayName.AddTranslation(GameCulture.Russian, "Искрящаяся Лопата");
            Tooltip.SetDefault(
                "<right> to shoot a small spark");
				Tooltip.AddTranslation(GameCulture.Russian, "<right>, чтобы послать маленькую искру");
        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;

            item.autoReuse = true;
            item.pick = 35;

            item.UseSound = SoundID.Item1;
            item.useStyle = 5;
            item.useTurn = false;
            item.useAnimation = 28;
            item.useTime = 15;

            item.melee = true; //melee damage
            item.damage = 5;
            item.knockBack = 3f;

            item.mana = 2;
            item.shoot = ProjectileID.Spark;
            item.shootSpeed = 8f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 1;
            item.value = 5400;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableDualWeapons) return;
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.CopperPickaxe, 1);
                }
                else
                {
                    recipe.AddIngredient(ItemID.TinPickaxe, 1);
                }
                recipe.AddIngredient(ItemID.WandofSparking, 1);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public override bool AltFunctionUse(Player player) { return true; }
        
        // Light weight, less flexible, but much safer
        public override bool CanUseItem(Player player)
        {
            if (PlayerFX.DualItemCanUseItemAlt(player, this,
                28f / 15f, 1f,
                1f, 1f))
            {
                item.useStyle = 5; // Doesn't set for other clients normally
                item.UseSound = SoundID.Item8; // Doesn't play for other clients normally
                item.useTurn = false;
                item.magic = true;
                item.melee = false;
                item.noMelee = true;
                player.itemTime = 0;
                item.pick = 0;
                item.shoot = ProjectileID.Spark;
            }
            else
            {
                // we can take advantage of the fact that CanUseItem never gets called by
                // clients if it was an alt function
                item.useStyle = 1;
                item.UseSound = SoundID.Item1;
                item.useTurn = true;
                item.magic = false;
                item.melee = true;
                item.noMelee = false;
                player.manaCost = 0f;
                item.pick = 35;
                item.shoot = 0; // No projectile
            }
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            damage = (int)(damage * 8f / 5f);
            knockBack *= 1 / 3f;
            player.itemTime = player.itemAnimation;
            return player.itemAnimation == player.itemAnimationMax - 1;
        }
    }
}
