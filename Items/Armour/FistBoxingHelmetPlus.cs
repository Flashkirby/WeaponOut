using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class FistBoxingHelmetPlus : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Prizefighter Helmet");
			DisplayName.AddTranslation(GameCulture.Russian, "Шлем Боксёра-Профессионала");
            Tooltip.SetDefault("Fighting bosses slowly empowers next melee attack, up to 1000%");
			Tooltip.AddTranslation(GameCulture.Russian "В битве с боссами следующая ближняя атака усиливается вплоть до 1000%");

            ModTranslation text;

            text = mod.CreateTranslation("FistBoxingHelmetPlusPower");
            text.SetDefault("10 defense per 100 missing life");
            text.AddTranslation(GameCulture.Russian "+10 защиты за каждые 100 потерянного здоровья");
			mod.AddTranslation(text);

            text = mod.CreateTranslation("FistBoxingHelmetPlusPowerHardmode");
            text.SetDefault("16 defense per 100 missing life");
            text.AddTranslation(GameCulture.Russian "+16 защиты за каждые 100 потерянного здоровья");
			mod.AddTranslation(text);

            text = mod.CreateTranslation("FistBoxingHelmetPlusDefence");
            text.SetDefault("Temporarily reduces damage taken when not attacking");
            text.AddTranslation(GameCulture.Russian, "Снижает получаемый урон вне атаки");
			mod.AddTranslation(text);

            text = mod.CreateTranslation("FistBoxingHelmetPlusSpeed");
            text.SetDefault("15 defense");
            text.AddTranslation(GameCulture.Russian, "15 защиты");
			mod.AddTranslation(text);

            text = mod.CreateTranslation("FistBoxingHelmetPlusSpeedHardmode");
            text.SetDefault("20 defense");
            text.AddTranslation(GameCulture.Russian, "20 защиты");
			mod.AddTranslation(text);

            text = mod.CreateTranslation("FistBoxingHelmetPlusGi");
            text.SetDefault("Makes fist parries easier and reduces combo power cost by 1");
            text.AddTranslation(GameCulture.Russian, "Облегчает парирование руками и снижает стоимость заряда комбо на 1");
			mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.defense = 8;
            item.value = Item.sellPrice(0, 3, 0, 0);
            item.rare = 3;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Bone, 20);
            recipe.AddIngredient(ItemID.BlackThread, 3);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PlayerFX>().patienceDamage = 10f; // Can do up to 1000%
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawAltHair = true;
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
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPlusPower");
                        player.GetModPlayer<PlayerFX>().barbariousDefence = 10;
                    }
                    else
                    {
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPlusPowerHardmode");
                        player.GetModPlayer<PlayerFX>().barbariousDefence = 6;
                    }
                    break;
                case 2:
                    // Not so useful, but very good for not dying in expert (double damage lmao)
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPlusDefence");
                    if (!hardMode)
                    { player.GetModPlayer<PlayerFX>().yomiEndurance += 0.3f; }
                    else
                    { player.GetModPlayer<PlayerFX>().yomiEndurance += 0.45f; }
                    break;
                case 3:
                    if (!hardMode)
                    {// Decent armour for protecting against most attacks (11 dmg reduction)
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPlusSpeed");
                        player.statDefense += 15;
                    }
                    else
                    {// Hardmode armour gets an extra 10 dmg reduction
                        player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPlusSpeedHardmode");
                        player.statDefense += 20;
                    }
                    break;
                case 4:
                    player.setBonus = WeaponOut.GetTranslationTextValue("FistBoxingHelmetPlusGi");
                    mpf.longParry = true;
                    mpf.comboCounterMaxBonus -= 1;
                    break;
            }
        }
    }
}