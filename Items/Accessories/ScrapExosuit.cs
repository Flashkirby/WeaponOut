using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class ScrapExosuit : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Exosuit");
            DisplayName.AddTranslation(GameCulture.Chinese, "废弃的机械服");
			DisplayName.AddTranslation(GameCulture.Russian, "Механический Экзоскелет");

            Tooltip.SetDefault(
                "Greatly reduces cooldown between dashes\n" +
                "Consuming combo will restore life\n" + 
                "Parrying with fists will steal life\n" + 
                "Greatly increases life regen when moving\n" + 
                "'Harness science!'");
            Tooltip.AddTranslation(GameCulture.Chinese, "减少冲刺（例如克苏鲁之盾的冲刺）的冷却时间\n消耗连击能量时恢复生命值\n使用拳套闪避敌人成功时窃取生命值\n移动时增加生命回复速度\n“运用科学！”");
			Tooltip.AddTranslation(GameCulture.Russian,
				"Значительно сокращает время между рывками\n" +
                "Поглощение заряда комбо восстанавливает здоровье\n" + 
                "Парирование руками крадёт здоровье\n" + 
                "Значительно восстанавливает здоровье при движении\n" + 
                "'Сила науки!'");

        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.defense = 8;
            item.rare = 6;
            item.accessory = true;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.expert = true;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<ScrapActuator>(), 1);
            recipe.AddIngredient(ModContent.ItemType<ScrapFrame>(), 1);
            recipe.AddIngredient(ModContent.ItemType<ScrapTransformer>(), 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        private int localCounter = 0;
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();

            mpf.parryLifesteal += 0.1f;

            if (player.dashDelay > 2) player.dashDelay -= 2;

            if (Math.Abs(player.velocity.X) > 4.5f)
            { player.lifeRegenCount += 8; } // healing per 2 seconds
            else if (Math.Abs(player.velocity.X) > 3f)
            { player.lifeRegenCount += 6; } // healing per 2 seconds
            else if (Math.Abs(player.velocity.X) > 1.5f)
            { player.lifeRegenCount += 4; } // healing per 2 seconds
            
            int diff = (localCounter - mpf.ComboCounter);
            if (diff > 0 && mpf.comboTimer > 0)
            {
                PlayerFX.HealPlayer(player, diff + 2, true);
            }
            localCounter = mpf.ComboCounter;

            if (!hideVisual)
            {
                Lighting.AddLight(player.Center, 1f, 0.2f, 0f);
            }
        }
    }
}
