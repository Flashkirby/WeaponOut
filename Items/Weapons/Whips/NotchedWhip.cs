using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Corruption whip, high speed. Whip damage variates between 1 lower and higher tier
    /// Blue tier
    /// </summary>
    public class NotchedWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vilelash");
			DisplayName.AddTranslation(GameCulture.Russian, "Порченная Плеть");
        }
        public override void SetDefaults()
        {
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 28;
            item.useTime = 28;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 15; // For balancing, damage should try to match when base*(bonuscrit/2)
            item.crit = 21; //damage = 2 * (base * bonus crit)
            item.knockBack = 4; // Testing effectiveness against hoplite with Vilethorn
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 1;
            item.value = Item.sellPrice(0,0,25,0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableWhips) return;
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