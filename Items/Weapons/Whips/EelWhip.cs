using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Swordfish
    /// </summary>
    public class EelWhip : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableWhips;
        }

        private bool increaseDamage;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eel Slapper");
        }
        public override void SetDefaults()
        {
			item.width = 40;
			item.height = 40;

            item.useStyle = 5;
            item.useAnimation = 22;
            item.useTime = 22;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 22; // For balancing, damage should try to match when base*(bonuscrit/2)
            item.crit = 18; //damage = 2 * (base * bonus crit)        120 damage
            item.knockBack = 4f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 8f; //projectile length

            item.rare = 2;
            item.value = Item.sellPrice(0,0,50,0);
        }
        public override void AddRecipes()
        {
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