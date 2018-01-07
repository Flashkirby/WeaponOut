using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Whips
{
    /// <summary>
    /// Not the garian sword :P
    /// </summary>
    public class PuzzlingCutter : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Puzzling Cutter");
            DisplayName.AddTranslation(GameCulture.Chinese, "令人费解的剑");
            DisplayName.AddTranslation(GameCulture.Russian, "Фрагментированный Резак");

            Tooltip.SetDefault("'No one said it was a sword'");
            Tooltip.AddTranslation(GameCulture.Chinese, "“没人会说它是一把剑”");
            Tooltip.AddTranslation(GameCulture.Russian, "Никто не говорил, что это меч");
        }
        public override void SetDefaults()
        {
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 17;
            item.useTime = 17;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.autoReuse = true;
            
            item.melee = true;
            item.damage = 35;
            item.crit = 36; //crit chance on whips increase crit damage instead
            item.knockBack = 2f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 5;
            item.value = Item.sellPrice(0,6,0,0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0,
                0.15f * player.gravDir + Main.rand.Next(-50, 50) * 0.001f * player.gravDir); //overhead swinging
            return false;
        }
    }
}