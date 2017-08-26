using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public class FightingSash : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pugilist Sash"); //Ceratopsian Shield
            Tooltip.SetDefault(
                "Restores lost life at the end of a combo\n" +
                "Restores up to 25% of maximum life\n" + 
                "Grants 20 damage knockback immunity");
        }
        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 28;
            item.rare = 1;
            item.accessory = true;
            item.value = Item.buyPrice(0, 1, 0, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.FallenStar, 5);
            recipe.AddIngredient(ItemID.GoldCoin, 1);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX modPlayer = player.GetModPlayer<PlayerFX>(mod);
            modPlayer.DamageKnockbackThreshold += 20;

            if (player.whoAmI == Main.myPlayer)
            {
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>(mod);
                if (mpf.ComboCounter > 0)
                {
                    player.AddBuff(mod.BuffType<Buffs.FightingSpirit>(), 2);
                }
            }
        }
    }
}
