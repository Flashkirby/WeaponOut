using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Bone sword style
    /// </summary>
    public class BoneWhip : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableWhips;
        }

        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Bone Whip";
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 24;
            item.useTime = 24;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.channel = true;
            item.damage = 13;
            item.crit = 21; //crit chance on whips increase crit damage instead
            item.knockBack = 2f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

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