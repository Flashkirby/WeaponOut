using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistMartialHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Martial Headband");
            Tooltip.SetDefault("3% increased melee critical strike chance\n");
        }
        public override void SetDefaults()
        {
            item.defense = 0;
            item.value = Item.sellPrice(0, 0, 10, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.Cactus, 5);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 3;
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
            else if (body.type == ItemID.Gi)
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
                    player.setBonus = "Automatically after being struck";
                    player.GetModPlayer<PlayerFX>().taekwonCounter = true;
                    break;
                case 2:
                    player.setBonus = "Increases length of invincibility after taking damage";
                    player.longInvince = true;
                    break;
                case 3:
                    player.setBonus = "50% increased uppercut and divekick damage";
                    mpf.uppercutDamage += 0.5f;
                    mpf.divekickDamage += 0.5f;
                    break;
            }
        }
    }
}