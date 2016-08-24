using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class ManaBlast : ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Mana Wand";
            item.toolTip = "Casts a mana restoring star";
            item.width = 38;
            item.height = 14;
            item.scale = 0.9f;

            item.magic = true;
            item.mana = 10;
            item.damage = 13;
            item.knockBack = 2;
            item.autoReuse = true;

            item.noMelee = true;
            item.shoot = mod.ProjectileType("ManaBlast");
            item.shootSpeed = 7;

            item.useStyle = 5;
            item.useSound = 9;
            item.useTime = 17;
            item.useAnimation = 17;

            item.rare = 1;
            item.value = 5000;
        }
        public override void AddRecipes()
        {
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