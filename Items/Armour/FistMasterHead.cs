using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistMasterHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Master Headband");
            DisplayName.AddTranslation(GameCulture.Chinese, "大师头带");

            Tooltip.SetDefault("9% increased melee critical strike chance\n" +
                "Fighting bosses slowly empowers next melee attack, up to 2000%\n" +
                "'A true master never stops learning'");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加9%近战暴击率\n与Boss战斗时，近战伤害会迅速提升\n最高为武器本身伤害的2000%，击中敌人后重新计算\n“真正的大师永远不会停下学习的步伐...”");

            ModTranslation text;
            
            text = mod.CreateTranslation("FistMasterHeadPower");
            text.SetDefault("Combo attacks deal additional damage based on enemy life");
            text.AddTranslation(GameCulture.Chinese, "连续攻击会基于敌人生命值造成额外伤害");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistMasterHeadDefence");
            text.SetDefault(
            "Taking damage grants yin, dealing damage grants yang, \n" +
            "at the end of a combo yin increases melee damage, \n" +
            "yang restores a portion of missing life");
            text.AddTranslation(GameCulture.Chinese, "承受伤害时出现“阴”状态，造成伤害时出现“阳”状态\n“阴”状态在连击结束后增加近战伤害\n“阳”状态将恢复在连击时损失的生命\n效果强度将由玩家损失的生命决定");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistMasterHeadSpeed");
            text.SetDefault("Build up momentum and double tap $BUTTON to leap towards a location,\n"
             +"Increases running speed by 15 mph");
            text.AddTranslation(GameCulture.Chinese, "移动一定程度后获得“动量加速”状态，此时双击下方向键可以在空中跳跃\n移动速度增加15mph");
            mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.defense = 10;
            item.value = Item.buyPrice(0, 50, 0, 0);
            item.rare = 8;

            item.width = 18;
            item.height = 18;
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 9;
            player.GetModPlayer<PlayerFX>().patienceDamage = 20f; // Can do up to 2000%
            player.GetModPlayer<PlayerFX>().patienceBuildUpModifier += 0.2f; // 75->90%
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
        }

        private byte armourSet = 0;
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            armourSet = 0;
            if (body.type == mod.ItemType<HighPowerBody>() &&
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
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMasterHeadPower");
                    pfx.millstone = true;
                    pfx.patienceBuildUpModifier += 0.1f;
                    break;
                case 2:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMasterHeadDefence");
                    pfx.yinyang = true;
                    break;
                case 3:
                    string button = Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMasterHeadSpeed").Replace("$BUTTON", button);
                    pfx.buildMomentum = true;
                    pfx.momentumDash = true;
                    player.accRunSpeed += 3f;
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