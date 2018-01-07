using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class DemonBloodCure : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Transfusion Potion");
			DisplayName.AddTranslation(GameCulture.Russian, "Зелье Переливания");
            Tooltip.SetDefault(
                "Removes effects granted by the Frenzy Heart");
				Tooltip.AddTranslation(GameCulture.Russian, "Снимает эффект Сердца Безумия");
        }
        public override void SetDefaults()
        {
            item.UseSound = SoundID.Item3;
            item.useStyle = 2;
            item.useTurn = true;
            item.useAnimation = 17;
            item.useTime = 17;
            item.maxStack = 30;
            item.consumable = true;
            item.width = 14;
            item.height = 24;
            item.expert = true;
            item.value = Item.sellPrice(0, 2, 0, 0);
            item.rare = 4;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddIngredient(mod.ItemType<DemonBlood>());
            recipe.AddIngredient(ItemID.PixieDust);
            recipe.AddIngredient(ItemID.Daybloom);
            recipe.AddIngredient(ItemID.Shiverthorn);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override bool CanUseItem(Player player)
        {
            return player.GetModPlayer<PlayerFX>().demonBlood && Main.expertMode;
        }
        public override bool UseItem(Player player)
        {
            player.GetModPlayer<PlayerFX>().demonBlood = false;
            return true;
        }
    }
}
