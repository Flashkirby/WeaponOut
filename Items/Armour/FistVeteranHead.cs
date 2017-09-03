using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistVeteranHead : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wanderer Headband");
            Tooltip.SetDefault("6% increased melee critical strike chance");
        }
        public override void SetDefaults()
        {
            item.defense = 2;
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = 3;

            item.width = 18;
            item.height = 18;
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 6;
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = true;
        }

        private byte armourSet = 0;
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            armourSet = 0;
            if (body.type == mod.ItemType<FistPowerBody>() &&
                legs.type == mod.ItemType<FistPowerLegs>())
            {
                armourSet = 1;
                return true;
            }
            else if (body.type == mod.ItemType<FistDefBody>() &&
                legs.type == mod.ItemType<FistDefLegs>())
            {
                armourSet = 2;
                return true;
            }
            else if (body.type == mod.ItemType<FistSpeedBody>() &&
                legs.type == mod.ItemType<FistSpeedLegs>())
            {
                armourSet = 3;
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
                    player.setBonus = "Increases melee capabilities after being struck";
                    player.GetModPlayer<PlayerFX>().taekwonCounter = true;
                    break;
                case 2:
                    player.setBonus = "Greatly increases life regeneration after being struck";
                    player.GetModPlayer<PlayerFX>().rapidRecovery = true;
                    break;
                case 3:
                    player.setBonus = "Build up momentum to smash into enemies";
                    player.accRunSpeed += 2f;
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