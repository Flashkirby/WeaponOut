using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Earliest game whip for corruption, low base but high crit
    /// Silver Broadsword
    /// What Categpry is a whip?
    /// Mid-Low damage for tier by default
    /// Poorer knockback than spears - not designed to protect
    /// High tier damage on crit tip - requires spacing and skill
    /// </summary>
    public class LeatherWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Leather Whip");
            DisplayName.AddTranslation(GameCulture.Chinese, "皮鞭");
            DisplayName.AddTranslation(GameCulture.Russian, "Кожаный Кнут");

            Tooltip.SetDefault(
                 "Only deals critical hits at the tip\n" +
                 "Critical strike chance boosts critical damage");
            Tooltip.AddTranslation(GameCulture.Chinese, "只能在鞭子的顶端触发暴击\n暴击将增加暴击伤害");
            Tooltip.AddTranslation(GameCulture.Russian,
                "Критические удары только концом плети\n" +
                "Шанс критического удара увеличивает критический урон");
        }
        public override void SetDefaults()
        {
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 18;
            item.useTime = 18;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 7;
            item.crit = 46; //crit chance on whips increase crit damage instead
            item.knockBack = 3.5f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 0;
            item.value = Item.sellPrice(0,0,0,80);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableWhips) return;
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