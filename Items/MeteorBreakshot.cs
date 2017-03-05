using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    /// <summary>
    /// Breaks into 3 shots
    /// </summary>
    public class MeteorBreakshot : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetDefaults()
        {
            item.name = "Meteoric Breakshot";
            item.toolTip = "Creates several meteoric shards on impact ";
            item.width = 14;
            item.height = 14;
            item.maxStack = 999;
            item.consumable = true;

            item.ranged = true;
            item.ammo = AmmoID.Bullet;
            item.shoot = mod.ProjectileType("MeteorBreakshot");
            item.shootSpeed = 2.5f;
            item.damage = 9;
            item.knockBack = 3f;

            item.rare = 2;
            item.value = 20;

        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteorShot, 50);
            recipe.AddIngredient(ItemID.Hellstone);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 50);
            recipe.AddRecipe();
        }
    }
}
