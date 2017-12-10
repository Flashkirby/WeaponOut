using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Earliest game whip for crimson, with better, more consistant damage than leather but slower
    /// Tungsten Broadsword Tier
    /// </summary>
    public class Whiplash : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Whiplash");
            Tooltip.SetDefault(
                "Only deals critical hits at the tip\n" +
                "Critical strike chance boosts critical damage");
        }
        public override void SetDefaults()
        {
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 21;
            item.useTime = 22;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 11;
            item.crit = 11; //crit chance on whips increase crit damage instead
            item.knockBack = 5f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 0;
            item.value = Item.sellPrice(0,0,0,80);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableWhips) return;
            ModRecipe recipe = new ModRecipe(mod);
             recipe.AddIngredient(ItemID.Vertebrae, 5);
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