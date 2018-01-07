using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class ChampionLaurels : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Champion Laurels");
            DisplayName.AddTranslation(GameCulture.Chinese, "冠军桂冠");
            DisplayName.AddTranslation(GameCulture.Russian, "Чемпионские Лавры");

            Tooltip.SetDefault("5% increased melee damage\n" +
                "Fighting bosses slowly empowers next melee attack, up to 2500%\n" +
                "'It ain't about how hard you hit'");//It's about HOW HARD YOU CAN GET HIT and keep moving forward
            Tooltip.AddTranslation(GameCulture.Chinese, "增加5%近战伤害\n与Boss战斗时，近战伤害会迅速提升\n最高为武器本身伤害的2500%，击中敌人后重新计算\n“关键不是你能打得多么厉害”");
            Tooltip.AddTranslation(GameCulture.Russian,
                "+5% урон ближнего боя\n" +
                "В битве с боссами следующая ближняя атака усиливается вплоть до 2500%\n" +
                "'Не важно, как сильно ты бьёшь'");

            ModTranslation text;

            text = mod.CreateTranslation("ChampionLaurelsPower");
            text.SetDefault("Divekicks will steal life");
            text.AddTranslation(GameCulture.Chinese, "下踢将窃取生命值");
            text.AddTranslation(GameCulture.Russian, "Атака в падении крадёт здоровье");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("ChampionLaurelsDefence");
            text.SetDefault("23 defense, damage taken is reduced by 5%,\ntemporarily reduces damage taken when not attacking");
            text.AddTranslation(GameCulture.Chinese, "23防御力，减少5%所承受的伤害\n在不攻击时暂时减少所承受的伤害");
            text.AddTranslation(GameCulture.Russian, "23 защиты, -5% получаемый урон, \nдополнительно снижает получаемый урон вне атаки");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("ChampionLaurelsSpeed");
            text.SetDefault("Maximum life acts as a second wind, restore maximum life with combos");
            text.AddTranslation(GameCulture.Chinese, "当体力为0时将触发“重振精力”Buff，你的生命栏将作为你的新生命值，可以通过连击进行恢复\n例子：假如你的最大生命值有500，生命值为0时你会有额外的500点体力");
            text.AddTranslation(GameCulture.Russian, "Максимальное здоровье - второе дыхание, восстанавливается через комбо");
            mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.defense = 20;
            item.value = Item.buyPrice(0, 25, 0, 0);
            item.rare = 7;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PlanteraMask);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeDamage += 0.05f;
            player.GetModPlayer<PlayerFX>().patienceDamage = 25f; // Can do up to 2500%
            player.GetModPlayer<PlayerFX>().patienceBuildUpModifier += 0.2f; // 75->90%
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
        }

        private byte armourSet = 0;
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            armourSet = 0;if (body.type == mod.ItemType<HighPowerBody>() &&
                legs.type == mod.ItemType<HighPowerLegs>())
            {
                armourSet = 1;
                return true;
            }
            else if (body.type == mod.ItemType<HighDefBody>() &&
                legs.type == mod.ItemType<HighDefLegs>())
            {
                armourSet = 2;
                return true;
            }
            else if (body.type == mod.ItemType<HighSpeedBody>() &&
                legs.type == mod.ItemType<HighSpeedLegs>())
            {
                armourSet = 3;
                return true;
            }
            return false;
        }
        
        public override void UpdateArmorSet(Player player)
        {
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();
            switch (armourSet)
            {
                case 1:
                    player.setBonus = WeaponOut.GetTranslationTextValue("ChampionLaurelsPower");
                    player.GetModPlayer<PlayerFX>().diveKickHeal += 0.03f;
                    pfx.patienceBuildUpModifier += 0.2f;
                    break;
                case 2:
                    player.setBonus = WeaponOut.GetTranslationTextValue("ChampionLaurelsDefence");
                    player.statDefense += 23;
                    player.endurance += 0.05f;
                    pfx.yomiEndurance += 0.45f;
                    break;
                case 3:
                    player.setBonus = WeaponOut.GetTranslationTextValue("ChampionLaurelsSpeed"); 
                    pfx.secondWind = true;
                    break;
            }
        }

        public override void ArmorSetShadows(Player player)
        {
            if (armourSet == 1)
            {
                player.armorEffectDrawShadowSubtle = true;
            }
            if (armourSet == 3)
            {
                player.armorEffectDrawShadow = true;
            }
        }
    }
}