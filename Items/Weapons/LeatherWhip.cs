using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class LeatherWhip : ModItem
    {
        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Leather Whip";
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 18;
            item.useTime = 18;
            item.useSound = 19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.channel = true;
            item.damage = 4;
            item.crit = 36; //crit chance on whips increase crit damage instead
            item.knockBack = 2f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 0;
            item.value = Item.sellPrice(0,0,0,80);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
             recipe.AddIngredient(ItemID.Leather, 1); //for leather whip later
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0, 
                Main.rand.Next(-150, 150) * 0.001f * player.gravDir); //whip swinging
            return false;
        }
    }
}