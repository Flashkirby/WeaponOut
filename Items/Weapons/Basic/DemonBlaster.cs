using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Basic
{
    /// <summary>
    /// A flashier and higher DPS demon scythe/space gun hybrid
    /// Pierces infinitely but damage falls off at a distance
    /// 
    /// </summary>
    public class DemonBlaster : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Blaster");
            DisplayName.AddTranslation(GameCulture.Chinese, "恶魔冲击波");
            DisplayName.AddTranslation(GameCulture.Russian, "Демонический Бластер");

            Tooltip.SetDefault(
                "Fires an unholy ray");
            Tooltip.AddTranslation(GameCulture.Chinese, "发射一道不洁的射线");
            Tooltip.AddTranslation(GameCulture.Russian, "Стреляет тёмным лучом");

        }
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 18;
            item.scale = 0.9f;

            item.magic = true;
            item.mana = 8;
            item.damage = 38; //DPS 162
            item.knockBack = 0;
            item.autoReuse = true;

            item.noMelee = true;
            item.shoot = mod.ProjectileType<Projectiles.DemonBlast>();
            item.shootSpeed = 30;

            item.useStyle = 5;
            item.UseSound = SoundID.Item12;
            item.useTime = 14;
            item.useAnimation = 14;

            item.rare = 4;
            item.value = 18000;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 20);
            recipe.AddIngredient(ItemID.SoulofNight, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UseStyle(Player player)
        {
            PlayerFX.modifyPlayerItemLocation(player, -4, 0);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            lightColor.R = 255;
            lightColor.G = Math.Max((byte)119, lightColor.G);
            lightColor.B = 255;
            return lightColor;
        }
    }
}
