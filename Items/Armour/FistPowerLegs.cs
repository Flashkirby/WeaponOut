using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class FistPowerLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dobok Pants");
			DisplayName.AddTranslation(GameCulture.Russian, "Штаны Добок");
            Tooltip.SetDefault("5% increased melee damage\n" + 
                "100% increased divekick damage and knockback");
				Tooltip.AddTranslation(GameCulture.Russian,
				"+5% урон ближнего боя\n" + 
                "+100% урон и отбрасывание в падении");
        }
        public override void SetDefaults()
        {
            item.defense = 1;
            item.value = Item.sellPrice(0, 0, 6, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ItemID.Cactus, 8);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.05f;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.divekickDamage += 1f;
            mpf.divekickKnockback += 1f;
        }
    }
}