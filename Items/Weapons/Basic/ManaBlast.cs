using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Basic
{
    public class ManaBlast : ModItem
    {
        public override void SetStaticDefaults() 
        {
            DisplayName.SetDefault("Mana Wand");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔力杖");
            DisplayName.AddTranslation(GameCulture.Russian, "Трость Маны");

            Tooltip.SetDefault(
            "Casts a mana restoring star");
            Tooltip.AddTranslation(GameCulture.Chinese, "释放吸取魔力值的星星");
            Tooltip.AddTranslation(GameCulture.Russian, "Пускает звезду, крадущую ману");
        }
        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 14;
            item.scale = 0.9f;

            item.magic = true;
            item.mana = 10;
            item.damage = 13;
            item.knockBack = 2;
            item.autoReuse = true;

            item.noMelee = true;
            item.shoot = mod.ProjectileType<Projectiles.ManaBlast>();
            item.shootSpeed = 7;

            item.useStyle = 5;
            item.UseSound = SoundID.Item9;
            item.useTime = 17;
            item.useAnimation = 17;

            item.rare = 1;
            item.value = 5000;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ManaCrystal, 1);
            recipe.AddIngredient(ItemID.Wood, 3);
            recipe.anyWood = true;
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UseStyle(Player player)
        {
            PlayerFX.modifyPlayerItemLocation(player, -4, -5);
        }
    }
}