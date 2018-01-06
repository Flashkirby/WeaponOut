﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Normal jungle tier
    /// Blade of Grass?
    /// </summary>
    public class CoiledThorns : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Coiled Thorns");
			DisplayName.AddTranslation(GameCulture.Russian, "Шипастая Лоза");
        }
        public override void SetDefaults()
        {
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 18;
            item.useTime = 18;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 20;
            item.crit = 31; //crit chance on whips increase crit damage instead
            item.knockBack = 2f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 3;
            item.value = Item.sellPrice(0,0,60,0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableWhips) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.JungleSpores, 6);
            recipe.AddIngredient(ItemID.Vine, 2);
            recipe.AddIngredient(ItemID.Stinger, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0, 
                Main.rand.Next(-100, 100) * 0.001f * player.gravDir); //whip swinging
            return false;
        }
    }
}