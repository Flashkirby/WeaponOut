using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Basic
{
    /// <summary>
    /// for Kiedev
    /// </summary>
    public class BoneZone : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strongbone");
            Tooltip.SetDefault(
                "Shoots a powerful, high velocity bullet\n" +
                "'Skele-tons of damage!'");
        }
        public override void SetDefaults()
        {
            item.width = 56;
            item.height = 30;
            item.scale = 0.9f;

            item.ranged = true;
            item.useAmmo = AmmoID.Bullet;
            item.damage = 66;
            item.knockBack = 4f;
            item.autoReuse = true;

            item.noMelee = true;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 12;

            item.useStyle = 5;
            item.UseSound = SoundID.Item41;
            item.useTime = 30;
            item.useAnimation = item.useTime + 1; // Prevent auto-reuse time desync

            item.rare = 3;
            item.expert = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BoneGlove, 1);
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (type == ProjectileID.Bullet) type = ProjectileID.BulletHighVelocity;
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, -2);
        }
    }
}
