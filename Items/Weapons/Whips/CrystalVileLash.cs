using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// So I heard you don't like the crystal vilethorn so we made it better
    /// Chain guillotines, or Lightred-Pink tier
    /// </summary>
    public class CrystalVileLash : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableWhips;
        }
        
        public override void SetDefaults()
        {
            item.name = "Crystal Vilelash";
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 28;
            item.useTime = 28;
            item.UseSound = SoundID.Item101;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.autoReuse = true;
            
            item.melee = true;
            item.damage = 52;
            item.crit = 2; //crit chance on whips increase crit damage instead
            item.knockBack = 1.5f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 5;
            item.value = Item.sellPrice(0, 7, 0, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrystalVileShard, 1);
            recipe.AddIngredient(ItemID.SoulofLight, 5);
            recipe.AddTile(TileID.MythrilAnvil);
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