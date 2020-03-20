﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistVeteranHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wanderer Headband");
            DisplayName.AddTranslation(GameCulture.Chinese, "浪人头带");
            DisplayName.AddTranslation(GameCulture.Russian, "Повязка Странника");

            Tooltip.SetDefault("6% increased melee critical strike chance\n"
                + "Fighting bosses slowly empowers next melee attack, up to 800%");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加6%近战暴击率\n与Boss战斗时，近战伤害会迅速提升\n最高为武器本身伤害的800%，击中敌人后重新计算");
            Tooltip.AddTranslation(GameCulture.Russian,
				"+6% шанс критического удара в ближнем бою\n"
                + "В битве с боссами следующая ближняя атака усиливается вплоть до 800%");


            ModTranslation text;

            text = mod.CreateTranslation("FistVeteranHeadPower");
            text.SetDefault("Increases melee capabilities after being struck");
            text.AddTranslation(GameCulture.Chinese, "被伤害时增加近战能力");
            text.AddTranslation(GameCulture.Russian, "Ближний бой усиливается при получении урона");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistVeteranHeadPowerHardmode");
            text.SetDefault("Greatly increases melee capabilities after being struck");
            text.AddTranslation(GameCulture.Chinese, "被伤害时大幅增加近战能力");
            text.AddTranslation(GameCulture.Russian, "Значительно усиливает ближний бой при получении урона");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistVeteranHeadDefence");
            text.SetDefault("Greatly increases life regeneration after being struck");
            text.AddTranslation(GameCulture.Chinese, "被伤害时大幅增加生命回复速度");
            text.AddTranslation(GameCulture.Russian, "Значительно ускоряет восстановление здоровья при получении урона");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistVeteranHeadDefenceHardmode");
            text.SetDefault("Increases length of invincibility after taking damage, \n" +
            "greatly increases life regeneration after being struck");
            text.AddTranslation(GameCulture.Chinese, "受到伤害后的无敌时间增长\n被伤害时大幅增加生命回复速度");
            text.AddTranslation(GameCulture.Russian, "Продлевает время неуязвимости после получения урона, \n" +
                "значительно ускоряет восстановление здоровья при получении урона");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistVeteranHeadSpeed");
            text.SetDefault("Build up momentum to smash into enemies, increased movement speed");
            text.AddTranslation(GameCulture.Chinese, "移动一定程度后获得“动量加速”状态猛撞敌人\n增加移动速度");
            text.AddTranslation(GameCulture.Russian, "Наберите скорость, чтобы броситься на врагов, \n" +
                "увеличивает скорость бега");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistVeteranHeadGi");
            text.SetDefault("100% increased uppercut and divekick damage\n" +
                "Reduces combo power cost by 1");
            text.AddTranslation(GameCulture.Chinese, "增加100%上勾拳和下踢伤害\n减少1点连击能量消耗");
            text.AddTranslation(GameCulture.Russian, "+100% урон в прыжке и падении\n" +
                "-1 стоимость заряда комбо");
            mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.defense = 4;
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = 3;

            item.width = 18;
            item.height = 18;
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 6;
            player.GetModPlayer<PlayerFX>().patienceDamage = 8f; // Can do up to 800%
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
        }

        private byte armourSet = 0;
        private bool hardMode = false;
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            armourSet = 0;
            if (body.type == ModContent.ItemType<FistPowerBody>() &&
                legs.type == ModContent.ItemType<FistPowerLegs>())
            {
                armourSet = 1; hardMode = false;
                return true;
            }
            else if (body.type == ModContent.ItemType<HighPowerBody>() &&
                legs.type == ModContent.ItemType<HighPowerLegs>())
            {
                armourSet = 1; hardMode = true;
                return true;
            }
            else if (body.type == ModContent.ItemType<FistDefBody>() &&
                legs.type == ModContent.ItemType<FistDefLegs>())
            {
                armourSet = 2; hardMode = false;
                return true;
            }
            else if (body.type == ModContent.ItemType<HighDefBody>() &&
                legs.type == ModContent.ItemType<HighDefLegs>())
            {
                armourSet = 2; hardMode = true;
                return true;
            }
            else if (body.type == ModContent.ItemType<FistSpeedBody>() &&
                legs.type == ModContent.ItemType<FistSpeedLegs>())
            {
                armourSet = 3; hardMode = false;
                return true;
            }
            else if (body.type == ModContent.ItemType<HighSpeedBody>() &&
                legs.type == ModContent.ItemType<HighSpeedLegs>())
            {
                armourSet = 3; hardMode = true;
                return true;
            }
            else if (body.type == ItemID.Gi)
            {
                armourSet = 4;
                return true;
            }
            return false;
        }
        
        public override void UpdateArmorSet(Player player)
        {
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            switch (armourSet)
            {
                case 1:
                    if (!hardMode)
                    {
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistVeteranHeadPower");
                    }
                    else
                    {
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistVeteranHeadPowerHardmode");
                        player.GetModPlayer<PlayerFX>().doubleDamageUp = true;
                    }
                    player.GetModPlayer<PlayerFX>().taekwonCounter = true;
                    break;
                case 2:
                    if (!hardMode)
                    {
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistVeteranHeadDefence");
                    }
                    else
                    {
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistVeteranHeadDefenceHardmode");
                        player.longInvince = true;
                    }
                    player.GetModPlayer<PlayerFX>().rapidRecovery = true;
                    break;
                case 3:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistVeteranHeadSpeed");
                    player.accRunSpeed += 2f;
                    if (hardMode) { player.accRunSpeed += 1f; player.moveSpeed += 0.15f; }
                    player.GetModPlayer<PlayerFX>().buildMomentum = true;
                    break;
                case 4:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistVeteranHeadGi");
                    mpf.uppercutDamage += 1f;
                    mpf.divekickDamage += 1f;
                    mpf.comboCounterMaxBonus -= 1;
                    break;
            }
        }
    }
}