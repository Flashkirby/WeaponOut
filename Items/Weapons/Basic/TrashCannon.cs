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
    /// 4 shots, 4 in a busrt, 2 seconds
    /// </summary>
    public class TrashCannon : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trash Cannon");
            Tooltip.SetDefault(
                "'You might not, but your trash can'");
        }
        public override void SetDefaults()
        {
            item.width = 48;
            item.height = 36;
            item.scale = 0.9f;

            item.ranged = true;
            item.damage = 14;
            item.knockBack = 5;

            item.noMelee = true;
            item.useAmmo = AmmoID.Bullet;
            item.shoot = 14;
            item.shootSpeed = 5;

            item.useStyle = 5;
            item.useAnimation = 48; //4 shots
            item.useTime = 12;
            item.reuseDelay = 120 - item.useAnimation;// wait
            item.autoReuse = true;

            item.rare = 2;
            item.value = Item.buyPrice(0, 14, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddIngredient(ItemID.TrashCan, 1);
            recipe.AddIngredient(ItemID.Dynamite, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int i = 0; i < Main.rand.Next(4,6); i++)
            {
                Projectile.NewProjectile(position + new Vector2(8 * Main.rand.NextFloatDirection(), 8 * Main.rand.NextFloatDirection()), 
                    new Vector2(
                        speedX + 2 * Main.rand.NextFloatDirection(),
                        speedY + 2 * Main.rand.NextFloatDirection()),
                    type, damage, knockBack, player.whoAmI);
            }
            return false;
        }

        public override void HoldItem(Player player)
        {
            if(HoldItemSFX(player, item, 2, 14))
            {
                Gore.NewGore(player.Top, new Vector2(0, -2),
                    mod.GetGoreSlot("Gores/TrashCannon_Lid"), item.scale);
            }
        }

        public static int HoldItemSFXCounter;
        /// <summary>
        /// Allows sounds to be played in time with reuseDelay, with true returned on 1st frame
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static bool HoldItemSFX(Player player, Item item, int type, int style)
        {
            bool firstFrame = false;
            if (player.itemAnimation > 0 && HoldItemSFXCounter <= 1)
            {
                HoldItemSFXCounter = item.useAnimation + item.reuseDelay - 1;
                firstFrame = true;
            }
            if (HoldItemSFXCounter > 0)
            {
                int activeVolley = HoldItemSFXCounter - item.reuseDelay;
                if (activeVolley >= 0 && (activeVolley + 1) % item.useTime == 0)
                {
                    Main.PlaySound(type, player.position, style);
                }
                HoldItemSFXCounter--;
            }
            return firstFrame;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, -2);
        }
    }
}
