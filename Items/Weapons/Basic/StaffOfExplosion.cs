using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Basic
{
    public class StaffOfExplosion : ModItem
    {
        public const int baseDamage = 40;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Staff of Explosion");
            DisplayName.AddTranslation(GameCulture.Chinese, "爆裂法杖");
			DisplayName.AddTranslation(GameCulture.Russian, "Посох Взрыва");
            
            Tooltip.SetDefault(
                "Create a powerful explosion at a location\n" +
                "Increase channel speed by standing still\n" +
                "Enemies are more likely to target you while casting");
            Tooltip.AddTranslation(GameCulture.Chinese, "在光标位置施放一个强大的爆炸点\n按住的时间越久，爆炸威力越高\n站立不动时，增加蓄力速度\n在施放时敌人更可能以你为目标");
			Tooltip.AddTranslation(GameCulture.Russian,
			    "Создаёт мощный взрыв на выбранном месте\n" +
                "Заряжается быстрее, если не двигаться\n" +
                "Враги чаще предпочитают атаковать вас во время заряда");
        }

        public override void SetDefaults()
        {
            item.width = 52;
            item.height = 14;
            item.scale = 1f;

            item.magic = true;
            item.channel = true;
            item.mana = 10;
            item.damage = baseDamage; //damage * (charge ^ 2) *1(0) - *25(8) - *160(11) - *1000(15)
            item.knockBack = 3f; //up to x2.18
            item.autoReuse = true;

            item.noMelee = true;
            Item.staff[item.type] = true; //rotate weapon, as it is a staff
            item.shoot = ModContent.ProjectileType<Projectiles.Explosion>();
            item.shootSpeed = 1;

            item.useStyle = 5;
            item.UseSound = SoundID.Item8;
            item.useTime = 60;
            item.useAnimation = 60;

            item.rare = 8;
            item.value = Item.sellPrice(0, 10, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.WandofSparking, 1); // Surface chest
            recipe.AddIngredient(ItemID.RubyStaff, 1); // Extractinator, or Gold ore world
            recipe.AddIngredient(ItemID.MeteorStaff, 1); // Meteorite
            recipe.AddIngredient(ItemID.InfernoFork, 1); // Inferno caster
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UseStyle(Player player)
        {
            PlayerFX.modifyPlayerItemLocation(player, -4, -5);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position = Main.MouseWorld;
            return true;
        }
    }
}