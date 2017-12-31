using Terraria;
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

            Tooltip.SetDefault("6% increased melee critical strike chance\n"
                + "Fighting bosses slowly empowers next melee attack, up to 800%");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加6%近战暴击率\n与Boss战斗时，近战伤害会迅速提升\n最高为武器本身伤害的800%，击中敌人后重新计算");

            ModTranslation text;
            
            text = mod.CreateTranslation("FistVeteranHeadPower");
            text.SetDefault("Increases melee capabilities after being struck");
            text.AddTranslation(GameCulture.Chinese, "被伤害时增加近战能力");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistVeteranHeadPowerHardmode");
            text.SetDefault("Greatly increases melee capabilities after being struck");
            text.AddTranslation(GameCulture.Chinese, "被伤害时大幅增加近战能力");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistVeteranHeadDefence");
            text.SetDefault("Greatly increases life regeneration after being struck");
            text.AddTranslation(GameCulture.Chinese, "被伤害时大幅增加生命回复速度");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistVeteranHeadDefenceHardmode");
            text.SetDefault("Increases length of invincibility after taking damage, \n" +
            "greatly increases life regeneration after being struck");
            text.AddTranslation(GameCulture.Chinese, "受到伤害后的无敌时间增长\n被伤害时大幅增加生命回复速度");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistVeteranHeadSpeed");
            text.SetDefault("Build up momentum to smash into enemies, increased movement speed");
            text.AddTranslation(GameCulture.Chinese, "移动一定程度后获得“动量加速”状态猛撞敌人\n增加移动速度");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistVeteranHeadGi");
            text.SetDefault("100% increased uppercut and divekick damage\n" +
            "Reduces combo power cost by 1");
            text.AddTranslation(GameCulture.Chinese, "增加100%上勾拳和下踢伤害\n减少1点连击能量消耗");
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
            if (body.type == mod.ItemType<FistPowerBody>() &&
                legs.type == mod.ItemType<FistPowerLegs>())
            {
                armourSet = 1; hardMode = false;
                return true;
            }
            else if (body.type == mod.ItemType<HighPowerBody>() &&
                legs.type == mod.ItemType<HighPowerLegs>())
            {
                armourSet = 1; hardMode = true;
                return true;
            }
            else if (body.type == mod.ItemType<FistDefBody>() &&
                legs.type == mod.ItemType<FistDefLegs>())
            {
                armourSet = 2; hardMode = false;
                return true;
            }
            else if (body.type == mod.ItemType<HighDefBody>() &&
                legs.type == mod.ItemType<HighDefLegs>())
            {
                armourSet = 2; hardMode = true;
                return true;
            }
            else if (body.type == mod.ItemType<FistSpeedBody>() &&
                legs.type == mod.ItemType<FistSpeedLegs>())
            {
                armourSet = 3; hardMode = false;
                return true;
            }
            else if (body.type == mod.ItemType<HighSpeedBody>() &&
                legs.type == mod.ItemType<HighSpeedLegs>())
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
                        player.setBonus = "Increases melee capabilities after being struck";
                    }
                    else
                    {
                        player.setBonus = "Greatly increases melee capabilities after being struck";
                        player.GetModPlayer<PlayerFX>().doubleDamageUp = true;
                    }
                    player.GetModPlayer<PlayerFX>().taekwonCounter = true;
                    break;
                case 2:
                    if (!hardMode)
                    {
                        player.setBonus = "Greatly increases life regeneration after being struck";
                    }
                    else
                    {
                        player.setBonus = Language.GetTextValue("ItemTooltip.CrossNecklace") + ", greatly increases life regeneration after being struck";
                        player.longInvince = true;
                    }
                    player.GetModPlayer<PlayerFX>().rapidRecovery = true;
                    break;
                case 3:
                    player.setBonus = "Build up momentum to smash into enemies, increased movement speed";
                    player.accRunSpeed += 2f;
                    if (hardMode) { player.accRunSpeed += 1f; player.moveSpeed += 0.15f; }
                    player.GetModPlayer<PlayerFX>().buildMomentum = true;
                    break;
                case 4:
                    player.setBonus = "100% increased uppercut and divekick damage\n" + 
                        "Reduces combo power cost by 1";
                    mpf.uppercutDamage += 1f;
                    mpf.divekickDamage += 1f;
                    mpf.comboCounterMaxBonus -= 1;
                    break;
            }
        }
    }
}