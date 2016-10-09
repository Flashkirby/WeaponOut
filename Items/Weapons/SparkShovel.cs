using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class SparkShovel : ModItem
    {
        HelperDual dual;
        public override void SetDefaults()
        {
            item.name = "Spark Shovel";
            item.toolTip = "Right click to shoot a small spark";
            item.width = 32;
            item.height = 32;

            item.autoReuse = true;
            item.pick = 35;

            item.useSound = 1;
            item.useStyle = 1; //swing
            item.useTurn = true; //face player dir
            item.useAnimation = 16;
            item.useTime = 15;

            item.melee = true; //melee damage
            item.damage = 5;
            item.knockBack = 3f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 1;
            item.value = 5400;

            dual = new HelperDual(item, true);
            dual.UseSound = 8;
            dual.UseStyle = 5;
            dual.UseTurn = false;
            dual.UseAnimation = 28;
            dual.UseTime = 28;

            dual.Magic = true;
            dual.NoMelee = true;
            dual.Damage = 8;
            dual.KnockBack = 1f;

            dual.Mana = 2;
            dual.Shoot = ProjectileID.Spark;
            dual.ShootSpeed = 8f;

            dual.setValues(false, true);
            //end by setting default values
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.CopperPickaxe, 1);
                }
                else
                {
                    recipe.AddIngredient(ItemID.TinPickaxe, 1);
                }
                recipe.AddIngredient(ItemID.WandofSparking, 1);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public override bool AltFunctionUse(Player player) { return true; }
        public override void UseStyle(Player player)
        {
            dual.UseStyleMultiplayer(player);
            if (player.altFunctionUse > 0) PlayerFX.modifyPlayerItemLocation(player, -4, 0);
        }
        public override bool CanUseItem(Player player)
        {
            dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player)
        {
            dual.HoldStyle(player);
            base.HoldStyle(player);
        }
    }
}
