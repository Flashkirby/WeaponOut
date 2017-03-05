using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// For kiedev
    /// 4 shots, 4 in a busrt, 2 seconds
    /// </summary>
    public class TrashCannon : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public const int penetrateBonus = 4;

        public override void SetDefaults()
        {
            item.name = "Trash Cannon";
            item.toolTip = "'You might not, but your trash can'";
            item.width = 48;
            item.height = 36;
            item.scale = 0.9f;

            item.ranged = true;
            item.damage = 14;
            item.crit = 16;
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
        public override void AddRecipes()
        {
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

        private static int volleySFX;
        public override void HoldItem(Player player)
        {
            if(player.itemAnimation > 0 && volleySFX <= 1)
            {
                volleySFX = item.useAnimation + item.reuseDelay - 1;

                Gore.NewGore(player.Top, new Vector2(0, -2),
                    mod.GetGoreSlot("Gores/TrashCannon_Lid"), item.scale);
            }
            if(volleySFX > 0)
            {
                int activeVolley = volleySFX - item.reuseDelay;
                if (activeVolley >= 0 && (activeVolley + 1) % item.useTime == 0)
                {
                    Main.PlaySound(2, player.position, 14);
                }
                volleySFX--;
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, -2);
        }
    }
}
