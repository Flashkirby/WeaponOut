using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class FistPowerBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dobok Jacket");
            DisplayName.AddTranslation(GameCulture.Chinese, "道服上衣");
            DisplayName.AddTranslation(GameCulture.Russian, "Куртка Добок");

            Tooltip.SetDefault("5% increased melee damage\n" +
                "50% increased parry damage");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加5%近战伤害\n增加50%闪避伤害\n提醒：闪避指的是你使用拳套右键攻击接触敌人时所触发的攻击方式");
			Tooltip.AddTranslation(GameCulture.Russian,
				"+5% урон ближнего боя\n" +
                "+50% урон от парирования");

        }
        public override void SetDefaults()
        {
            item.defense = 2;
            item.value = Item.sellPrice(0, 0, 8, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddIngredient(ItemID.Cactus, 10);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.05f;
            ModPlayerFists.Get(player).parryDamage += 0.5f;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms)
        {
            drawHands = true;
        }
    }
}