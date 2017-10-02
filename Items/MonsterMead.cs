using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class MonsterMead : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Monster Mead");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Ale);
            item.buffType = mod.BuffType<Buffs.TipsyMead>();
            item.buffTime = 3600 * 5; // 5 mins
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Mug);
            recipe.AddIngredient(ItemID.Gel);
            recipe.AddIngredient(ItemID.Lens);
            recipe.AddIngredient(ItemID.Stinger);
            recipe.AddTile(TileID.Kegs);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
}
