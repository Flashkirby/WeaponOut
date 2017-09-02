using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class FistSpeedLegs : ModItem
    {
        public override bool Autoload(ref string name)
        {
            if (ModConf.enableFists)
            {
                if (!Main.dedServ)
                {
                    // Add the female leg variant
                    mod.AddEquipTexture(null, EquipType.Legs, "FistSpeedLegs_Female", "WeaponOut/Items/Armour/FistSpeedLegs_FemaleLegs");
                }
                return true;
            }
            return false;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boxing Shorts");
            Tooltip.SetDefault("5% increased movement speed\n" +
                "Reduces combo power cost by 1");
        }
        public override void SetDefaults()
        {
            item.defense = 0;
            item.value = Item.sellPrice(0, 0, 6, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Blinkroot, 3);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            if (!male) equipSlot = mod.GetEquipSlot("FistSpeedLegs_Female", EquipType.Legs);
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.05f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.comboCounterMaxBonus -= 1;
        }
    }
}