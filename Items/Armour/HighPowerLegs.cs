using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Legs)]
    public class HighPowerLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sahyun Trousers");
            DisplayName.AddTranslation(GameCulture.Chinese, "师贤裤子");

            Tooltip.SetDefault("10% increased melee critical strike chance\n" + 
                "125% increased divekick damage and knockback");
            Tooltip.AddTranslation(GameCulture.Chinese, "增加10%近战暴击率\n增加125%下踢伤害和击退\n提醒：下踢指的是你使用拳套在空中按下方向键攻击");
        }
        public override void SetDefaults()
        {
            item.defense = 7;
            item.value = Item.sellPrice(0, 4, 0, 0);
            item.rare = 5;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ItemID.SoulofFright, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 10;
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.divekickDamage += 1.25f;
            mpf.divekickKnockback += 1.25f;
        }
    }
}