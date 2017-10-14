using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class ColosseumHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gladiator Colosseum Helmet");
            Tooltip.SetDefault("8% increased melee damage\n'Let the games begin!'");
        }
        public override void SetDefaults()
        {
			item.width = 18;
			item.height = 18;
            item.defense = 5;
			item.value = 20000;
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.GladiatorHelmet, 1);
                recipe.AddIngredient(i == 0 ? ItemID.GoldBar : ItemID.PlatinumBar, 10);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this); recipe.AddRecipe();
            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemID.GladiatorBreastplate && legs.type == ItemID.GladiatorLeggings;
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("ItemTooltip.WarriorEmblem").Replace("15", "8");
            player.meleeDamage += 0.08f;
        }
    }
}
