using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighPowerBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sahyun Jacket");
            DisplayName.AddTranslation(GameCulture.Chinese, "师贤上衣");
            DisplayName.AddTranslation(GameCulture.Russian, "Накидка Мастера");

            Tooltip.SetDefault("15% increased melee damage\n" +
                "Taking damage whilst attacking builds combo\n" +
                "75% increased parry damage");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加15%近战伤害\n在攻击敌人时承受攻击也增加连击能量\n增加75%闪避伤害\n提醒：闪避指的是你使用拳套右键攻击接触敌人时所触发的攻击方式");
            Tooltip.AddTranslation(GameCulture.Russian, "+15% урон ближнего боя\n" +
				"Получение урона во время атаки заряжает комбо\n" +
				"+75% урон от парирования");

        }
        public override void SetDefaults()
        {
            item.defense = 8;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.15f;
            ModPlayerFists.Get(player).divekickDamage += 0.75f;
            player.GetModPlayer<PlayerFX>().angryCombo = true;
        }

        public override void DrawHands(ref bool drawHands, ref bool drawArms)
        {
            drawHands = true;
        }
    }
}