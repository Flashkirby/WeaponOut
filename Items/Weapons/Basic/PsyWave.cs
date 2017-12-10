using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Basic
{
    public class PsyWave : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psy Wave");
            Tooltip.SetDefault(
                "Cast a psionic orb");
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