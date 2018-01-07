using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// So I heard you don't like the crystal vilethorn so we made it better
    /// Chain guillotines, or Lightred-Pink tier
    /// </summary>
    public class CrystalVileLash : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Vilelash");
            DisplayName.AddTranslation(GameCulture.Chinese, "碎魔晶鞭");
			DisplayName.AddTranslation(GameCulture.Russian, "Кристальный Бич");
        }
        public override void SetDefaults()
        {
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
        public override void AddRecipes() {
            if (!ModConf.EnableWhips) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrystalVileShard, 1);
            recipe.AddIngredient(ItemID.SoulofLight, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int i = -1; i < 2; i++)
            {
                Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0,
                    Main.rand.Next(-100, 100) * 0.001f * player.gravDir + i * 0.3f); //whip swinging
            }
            return false;
        }
    }
}