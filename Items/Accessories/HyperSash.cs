using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public class HyperSash : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hyper Sash");
            Tooltip.SetDefault(
                "Restores lost life at the end of a combo\n" +
                "Restores up to 26% of maximum life\n" +
                "Bosses are slower to follow you\n" + 
                "Grants immunity to knockback");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 7;
            item.accessory = true;
            item.value = Item.buyPrice(0, 3, 0, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<FightingSash>(), 1);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 2);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();
            pfx.sashMaxLifeRecoverMult = Math.Max(0.26f, pfx.sashMaxLifeRecoverMult);
            pfx.ghostPosition = true;
            player.noKnockback = true;
        }
    }
}
