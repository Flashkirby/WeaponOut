using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class Fist : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableFists;
        }

        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Knuckleduster";
            item.toolTip = "Damage scales with chest defense";
            item.useStyle = FistStyle.useStyle;
            item.useAnimation = 19;//actually treated as -2
            item.width = 28;
            item.height = 28;
            item.damage = 2;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item7;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.anyIronBar = true;
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool UseItemFrame(Player player)
        {
            FistStyle.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = FistStyle.UseItemHitbox(player, ref hitbox, 4);
        }
    }
}
