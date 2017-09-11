using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistMasterHead : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Master Headband");
            Tooltip.SetDefault("9% increased melee critical strike chance\n"
                + "Fighting bosses slowly empowers next melee attack, up to 2000%");
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
            player.meleeCrit += 6;
            player.GetModPlayer<PlayerFX>().patienceDamage = 20f; // Can do up to 2000%
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
            switch (armourSet)
            {
                case 1:
                    player.setBonus = "Combo attacks deal additional damage based on enemy life";
                    player.GetModPlayer<PlayerFX>().millstone = true;
                    break;
                case 2:
                    player.setBonus = "Taking damage grants yin, dealing damage grants yang, \nat the end of a combo yin increases melee damage, \nyang restores a portion of missing life";
                    player.GetModPlayer<PlayerFX>().yinyang = true;
                    break;
                case 3:
                    string button = Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");
                    player.setBonus = "Build up momentum and double tap " + button + " to leap towards a location,\n"
                        + "Increases running speed by 15 mph";
                    player.GetModPlayer<PlayerFX>().buildMomentum = true;
                    player.GetModPlayer<PlayerFX>().momentumDash = true;
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