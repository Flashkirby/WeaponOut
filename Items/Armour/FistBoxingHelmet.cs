using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistBoxingHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boxing Helmet");
            DisplayName.AddTranslation(GameCulture.Chinese, "拳击头盔");

            Tooltip.SetDefault("Fighting bosses slowly empowers next melee attack, up to 500%");
            Tooltip.AddTranslation(GameCulture.Chinese, "与Boss战斗时，近战伤害会迅速提升\n最高为武器本身伤害的500%，击中敌人后重新计算");

            ModTranslation text;
            
            text = mod.CreateTranslation("FistBoxingHelmetPower");
            text.SetDefault("3 defense");
            text.AddTranslation(GameCulture.Chinese, "3防御力");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistBoxingHelmetDefence");
            text.SetDefault("6 defense");
            text.AddTranslation(GameCulture.Chinese, "6防御力");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistBoxingHelmetSpeed");
            text.SetDefault("4 defense");
            text.AddTranslation(GameCulture.Chinese, "4防御力");
            mod.AddTranslation(text);
            
            text = mod.CreateTranslation("FistBoxingHelmetGi");
            text.SetDefault("Makes fist parries easier");
            text.AddTranslation(GameCulture.Chinese, "使用拳套进行闪避更加容易");
            mod.AddTranslation(text);

        }
        public override void SetDefaults()
        {
            item.defense = 4;
            item.value = Item.sellPrice(0, 0, 10, 0);

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.Gel, 30);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PlayerFX>().patienceDamage = 5f; // Can do up to 500%
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
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPower");
                    player.statDefense += 3;
                    break;
                case 2:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetDefence");
                    player.statDefense += 6;
                    break;
                case 3:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetSpeed");
                    player.statDefense += 4;
                    break;
                case 4:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetGi");
                    mpf.longParry = true;
                    break;
            }
        }
    }
}