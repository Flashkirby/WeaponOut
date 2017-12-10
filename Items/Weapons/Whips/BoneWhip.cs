using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Bone sword style, built to send enemies to tip naturally
    /// Dungeon tier (Muramasa)
    /// </summary>
    public class BoneWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bone Whip");
            Tooltip.SetDefault("'Bad to the bone'");
        }
        public override void SetDefaults()
        {
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 14;
            item.useTime = 14;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 14;
            item.crit = 64; //crit chance on whips increase crit damage instead
            item.knockBack = 4f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 5f; //projectile length

            item.rare = 2;
            item.value = Item.sellPrice(0,0,50,0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableWhips) return;
            ModRecipe recipe = new ModRecipe(mod);
             recipe.AddIngredient(ItemID.Bone, 60);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0, 
                Main.rand.Next(-200, 200) * 0.001f * player.gravDir); //whip swinging
            return false;
        }
    }
}