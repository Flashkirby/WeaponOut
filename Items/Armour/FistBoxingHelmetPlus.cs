using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistBoxingHelmetPlus : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Prize Fighter Helmet");
        }
        public override void SetDefaults()
        {
            item.defense = 6;
            item.value = Item.sellPrice(0, 3, 0, 0);
            item.rare = 3;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Bone, 20);
            recipe.AddIngredient(ItemID.BlackThread, 3);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawAltHair = true;
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
                    player.setBonus = "Divekicks will steal life";
                    player.GetModPlayer<PlayerFX>().diveKickHeal += 0.04f;
                    break;
                case 2:
                    // Not so useful, but very good for not dying in expert (double damage lmao)
                    player.setBonus = "Temporarily reduces damage taken when not attacking";
                    player.GetModPlayer<PlayerFX>().yomiEndurance += 0.6f;
                    break;
                case 3:
                    // Decent armour for protecting against most attacks, (11 dmg reduction)
                    player.setBonus = Language.GetTextValue("ArmorSetBonus.Wood").Replace("1", "15");
                    player.statDefense += 15;
                    break;
                case 4:
                    player.setBonus = "Makes fist parries easier and reduces combo power cost by 1";
                    mpf.longParry = true;
                    mpf.comboCounterMaxBonus -= 1;
                    break;
            }
        }
    }
}