using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Swordfish
    /// </summary>
    public class EelWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eel Slapper");
            DisplayName.AddTranslation(GameCulture.Chinese, "鳝鱼打手");
            DisplayName.AddTranslation(GameCulture.Russian, "Боевой Угорь");

            Tooltip.SetDefault("'Banned from fish-slapping dance routines'");
            Tooltip.AddTranslation(GameCulture.Chinese, "“禁止跳鱼拍舞”");
            Tooltip.AddTranslation(GameCulture.Russian, "Запрещён на танце шлёпанья рыбами");
        }
        public override void SetDefaults()
        {
			item.width = 40;
			item.height = 40;

            item.useStyle = 5;
            item.useAnimation = 22;
            item.useTime = 22;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.damage = 22; // For balancing, damage should try to match when base*(bonuscrit/2)
            item.crit = 18; //damage = 2 * (base * bonus crit)        120 damage
            item.knockBack = 4f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 8f; //projectile length

            item.rare = 2;
            item.value = Item.sellPrice(0,0,50,0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0, 
                Main.rand.Next(-200, 200) * 0.001f * player.gravDir); //whip swinging
            return false;
        }
    }
}