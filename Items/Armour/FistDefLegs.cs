using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class FistDefLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dynasty Slippers");
			DisplayName.AddTranslation(GameCulture.Russian, "Башмаки Династии");
            Tooltip.SetDefault("5% increased melee critical strike chance\n" + 
                "Increases length of combo by 1 second");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Увеличивает шанс критического удара в ближнем бою на 5%\n" + 
                "+1 секунда длительности комбо");
        }
        public override void SetDefaults()
        {
            item.defense = 1;
            item.value = Item.buyPrice(0, 0, 25, 0);

            item.width = 18;
            item.height = 18;
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            if (!male) equipSlot = mod.GetEquipSlot("FistDefLegs_Female", EquipType.Legs);
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 5;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.comboResetTimeBonus += 60;
        }
    }
}