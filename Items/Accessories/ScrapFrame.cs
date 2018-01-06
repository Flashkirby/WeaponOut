﻿using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    public class ScrapFrame : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Frame");
			DisplayName.AddTranslation(GameCulture.Russian, "Механический Каркас");
            Tooltip.SetDefault(
                "Consuming combo will restore life");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Поглощение комбо восстанавливает здоровье");
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

        private int localCounter = 0;
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            int diff = (localCounter - mpf.ComboCounter) / 2;
            if (diff > 0 && mpf.comboTimer > 0)
            {
                PlayerFX.HealPlayer(player, diff, true);
            }
            localCounter = mpf.ComboCounter;
        }
    }
}
