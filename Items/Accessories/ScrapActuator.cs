using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class ScrapActuator : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Actuator");
            Tooltip.SetDefault(
                "Reduces cooldown between dashes\n" +
                "Increases life regen when moving");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 5;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.dashDelay > 1) player.dashDelay --;
            if (Math.Abs(player.velocity.X) > 1.5f)
            {
                player.lifeRegenCount += 2; // healing per 2 seconds
            }
        }
    }
}
