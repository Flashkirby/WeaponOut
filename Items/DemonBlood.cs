using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items
{
    public class DemonBlood : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Frenzy Heart");
            DisplayName.AddTranslation(GameCulture.Chinese, "狂乱魔心");
			DisplayName.AddTranslation(GameCulture.Russian, "Сердце Безумия");

            Tooltip.SetDefault(
                "Permanently grants a new melee healing ability\n" +
                "Damage taken can be recovered with melee attacks");
            Tooltip.AddTranslation(GameCulture.Chinese, "永久给予新的战斗吸血能力\n受到一定程度的伤害后触发效果");
			Tooltip.AddTranslation(GameCulture.Russian, "Открывает способность лечения ближним боем\n" +
				"Потерянное здоровье можно восстановить ближними атаками");
        }
        public override void SetDefaults()
        {
            item.maxStack = 99;
            item.consumable = true;
            item.width = 18;
            item.height = 18;
            item.useStyle = 4;
            item.useTime = 30;
            item.UseSound = SoundID.Item4;
            item.useAnimation = 30;
            item.rare = 4;
            item.expert = true;
            item.value = Item.sellPrice(0, 2, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.GetModPlayer<PlayerFX>().demonBlood && Main.expertMode;
        }
        public override bool UseItem(Player player)
        {
            player.GetModPlayer<PlayerFX>().demonBlood = true;
            return true;
        }
    }
}
