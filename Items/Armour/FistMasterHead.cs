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
            Tooltip.SetDefault("9% increased melee critical strike chance\n" +
                "Fighting bosses slowly empowers next melee attack, up to 2000%\n" +
                "'A true master never stops learning'");

            ModTranslation text;

            text = mod.CreateTranslation("FistMasterHeadPower");
            text.SetDefault("Combo attacks deal additional damage based on enemy life");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistMasterHeadDefence");
            text.SetDefault(
                "Taking damage grants yin, dealing damage grants yang, \n" + 
                "at the end of a combo yin increases melee damage, \n" + 
                "yang restores a portion of missing life");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistMasterHeadSpeed");
            text.SetDefault("Build up momentum and double tap $BUTTON to leap towards a location,\n"
                        + "Increases running speed by 15 mph");
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