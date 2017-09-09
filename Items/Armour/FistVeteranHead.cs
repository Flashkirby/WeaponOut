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
            Tooltip.SetDefault("6% increased melee critical strike chance\n"
                + "Fighting bosses slowly empowers next melee attack, up to 800%");
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