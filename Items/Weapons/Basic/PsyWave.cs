using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Basic
{
    public class PsyWave : ModItem
    {
        public override void SetStaticDefaults() 
        {
            DisplayName.SetDefault("Psy Wave");
            DisplayName.AddTranslation(GameCulture.Chinese, "精神波动");
            DisplayName.AddTranslation(GameCulture.Russian, "Пси-Волна");

            Tooltip.SetDefault(
                 "Cast a psionic orb");
            Tooltip.AddTranslation(GameCulture.Chinese, "释放异能光球");
            Tooltip.AddTranslation(GameCulture.Russian,
                "Пускает пси-шар");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.WaterBolt);
            item.useAnimation = 15;
            item.useTime = 15;
            item.UseSound = SoundID.Item24;
            
            item.damage = 40;
            item.knockBack = 0f;
            item.mana = 2;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1.5f;


            item.rare = 8;
            item.value = Item.sellPrice(0, 6, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SpellTome, 1);
            recipe.AddIngredient(ItemID.Ectoplasm, 8);
            recipe.AddTile(TileID.Bookcases);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}