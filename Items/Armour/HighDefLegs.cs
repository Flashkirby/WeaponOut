using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class HighDefLegs : ModItem
    {
        public override bool Autoload(ref string name)
        {
            if (ModConf.enableFists)
            {
                if (!Main.dedServ)
                {
                    // Add the female leg variant
                    mod.AddEquipTexture(null, EquipType.Legs, "HighDefLegs_Female", "WeaponOut/Items/Armour/HighDefLegs_FemaleLegs");
                }
                return true;
            }
            return false;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shaolin Trousers");
            Tooltip.SetDefault("14% increased melee critical strike chance\n" + 
                "Increases length of combo by 2 seconds");
        }
        public override void SetDefaults()
        {
            item.defense = 12;
            item.value = Item.sellPrice(0, 4, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            if (!male) equipSlot = mod.GetEquipSlot("HighDefLegs_Female", EquipType.Legs);
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 14;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.comboResetTimeBonus += 120;
        }
    }
}