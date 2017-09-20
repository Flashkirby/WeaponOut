using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class HighSpeedLegs : ModItem
    {
        public override bool Autoload(ref string name)
        {
            if (ModConf.enableFists)
            {
                if (!Main.dedServ)
                {
                    // Add the female leg variant
                    mod.AddEquipTexture(null, EquipType.Legs, "HighSpeedLegs_Female", "WeaponOut/Items/Armour/HighSpeedLegs_FemaleLegs");
                }
                return true;
            }
            return false;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Champion Guards");
            Tooltip.SetDefault("10% increased movement speed\n" +
                "50% increased increased divekick damage\n" +
                "Reduces combo power cost by 2");
        }
        public override void SetDefaults()
        {
            item.defense = 9;
            item.value = Item.sellPrice(0, 4, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ItemID.SoulofSight, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            if (!male) equipSlot = mod.GetEquipSlot("HighSpeedLegs_Female", EquipType.Legs);
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.1f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.divekickDamage += 0.5f;
            mpf.comboCounterMaxBonus -= 2;
        }
    }
}