using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class RushCharm : ModItem
    {
        public override void SetStaticDefaults() // Eye of Cthulu
        {
            DisplayName.SetDefault("Rush Charm");
            DisplayName.AddTranslation(GameCulture.Chinese, "冲锋咒文");

            Tooltip.SetDefault(
                "Reduces cooldown between dashes");
            Tooltip.AddTranslation(GameCulture.Chinese, "减少冲刺（例如克苏鲁之盾的冲刺）的冷却时间");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.dashDelay > 1) player.dashDelay--;
        }
    }
}
