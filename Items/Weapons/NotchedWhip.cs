using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Acts a bit like the solar eruption
    /// ai0 = time out?
    /// local ai0 = projectile rotation
    /// </summary>
    public class NotchedWhip : ModItem
    {
        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Vilelash";
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
            item.damage = 11;
            item.crit = 21; //crit chance on whips increase crit damage instead
            item.knockBack = 2f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 1;
            item.value = Item.sellPrice(0,0,25,0);
        }
        public override void AddRecipes()
        {
            // recipe.AddIngredient(ItemID.Leather, 1); //for leather whip later
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Vilethorn, 1);
            recipe.AddIngredient(ItemID.DemoniteBar, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0, 
                Main.rand.Next(-50, 50) * 0.001f * player.gravDir); //whip swinging
            return false;
        }
    }
}