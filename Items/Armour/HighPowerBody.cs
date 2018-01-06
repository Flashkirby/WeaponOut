using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Body)]
    public class HighPowerBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sahyun Jacket");
			DisplayName.AddTranslation(GameCulture.Russian, "Накидка Мастера");
            Tooltip.SetDefault("15% increased melee damage\n" +
                "Taking damage whilst attacking builds combo\n" +
                "75% increased parry damage");
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