using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons.Basic
{
    /// <summary>
    /// For kiedev
    /// 6 shots a second, 10 shots, 1.5 second cooldown
    /// </summary>
    public class ScrapSalvo : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableBasicContent;
        }

        public const int penetrateBonus = 4;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Salvo");
            Tooltip.SetDefault(
                "'Nothing subtle about this one'");
        }
        public override void SetDefaults()
        {
            item.width = 60;
            item.height = 30;
            item.scale = 0.9f;

            item.ranged = true;
            item.damage = 26; // 19 speed / 5 shots per trigger = 3.8 speed
            item.knockBack = 10f;

            item.noMelee = true;
            item.useAmmo = AmmoID.Bullet;
            item.shoot = 14;
            item.shootSpeed = 8f;

            item.useStyle = 5;
            item.useTime = 10;
            item.useAnimation = item.useTime * 10; //10 shots
            item.reuseDelay = 190 - item.useAnimation;// wait
            item.autoReuse = true;

            item.rare = 7;
            item.value = Item.buyPrice(0, 50, 0, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            float knockBackMult = 1 - (float)player.itemAnimation / item.useAnimation;
            for (int i = -2; i < 3; i++)
            {
                Projectile.NewProjectile(position + new Vector2(8 * Main.rand.NextFloatDirection(), 8 * Main.rand.NextFloatDirection()), 
                    new Vector2(speedX, speedY).RotatedBy(i * 0.11d) * (1f + knockBackMult * 0.5f),
                    type, damage, knockBack * 1.5f * knockBackMult, player.whoAmI);
            }
            return false;
        }

        public override void HoldItem(Player player)
        {
            TrashCannon.HoldItemSFX(player, item, 2, 36);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4, -2);
        }
    }
}
