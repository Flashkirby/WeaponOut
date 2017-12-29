using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistMartialHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Apprentice Headband");
            Tooltip.SetDefault("3% increased melee critical strike chance\n"
                + "Fighting bosses slowly empowers next melee attack, up to 400%");

            ModTranslation text;

            text = mod.CreateTranslation("FistMartialHeadPower");
            text.SetDefault("Negates fall damage"); // ItemTooltip.LuckyHorseshoe
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistMartialHeadDefence");
            text.SetDefault("Increases length of invincibility after taking damage"); // ItemTooltip.CrossNecklace
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistMartialHeadSpeed");
            text.SetDefault("Increases running speed by 10 mph");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("FistMartialHeadGi");
            text.SetDefault("50% increased uppercut and divekick damage");
            mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.defense = 1;
            item.value = Item.sellPrice(0, 0, 10, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 3;
            player.GetModPlayer<PlayerFX>().patienceDamage = 4f; // Can do up to 400%
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
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMartialHeadPower");
                    player.noFallDmg = true;
                    break;
                case 2:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMartialHeadDefence");
                    player.longInvince = true;
                    break;
                case 3:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMartialHeadSpeed");
                    player.accRunSpeed += 2f;
                    break;
                case 4:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistMartialHeadGi");
                    mpf.uppercutDamage += 0.5f;
                    mpf.divekickDamage += 0.5f;
                    break;
            }
        }
    }
}