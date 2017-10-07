using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class ScrapFrame : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Frame");
            Tooltip.SetDefault(
                "Consuming combo will restore life");
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
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            int diff = mpf.OldComboCounter - mpf.ComboCounter;
            if (diff > 0)
            {
                Main.NewText("diff" + diff + ", time: " + mpf.comboTimer + "/" + mpf.comboTimerMax);
                PlayerFX.HealPlayer(player, 2 * diff, true);
            }
        }
    }
}
