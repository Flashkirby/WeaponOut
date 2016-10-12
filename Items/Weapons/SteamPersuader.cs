using System;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class SteamPersuader : ModItem
    {
        HelperDual dual;
        public override void SetDefaults()
        {
            item.name = "Steam Persuader";
            item.toolTip = "Right click to fire something else";
            item.width = 62;
            item.height = 20;

            item.useSound = 31;
            item.useStyle = 5;
            item.useAnimation = 12; //4 shots
            item.useTime = 3;
            item.reuseDelay = 14;
            item.autoReuse = true;

            item.ranged = true; //melee damage
            item.noMelee = true;
            item.damage = 7;
            item.knockBack = 1f;

            item.useAmmo = 14;
            item.shoot = 10;
            item.shootSpeed = 9f;

            item.rare = 4;
            item.value = Item.sellPrice(0, 0, 30, 0);

            dual = new HelperDual(item, true);
            dual.UseSound = 38;
            dual.UseAnimation = 28;
            dual.UseTime = 28;
            dual.ReuseDelay = 0;

            dual.KnockBack = 4f;

            dual.ShootSpeed = 6f;

            dual.setValues(false, true);
            //end by setting default values
        }

        public override bool AltFunctionUse(Player player) { return true; }
        public override void UseStyle(Player player)
        {
            dual.UseStyleMultiplayer(player);
            PlayerFX.modifyPlayerItemLocation(player, -28, -2);
        }
        public override bool CanUseItem(Player player)
        {
            dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player)
        {
            dual.HoldStyle(player);
            base.HoldStyle(player);
        }

        // Act like clockwork gun
        public override bool ConsumeAmmo(Player player)
        {
            if (player.itemAnimation < item.useAnimation - 1) { return false; } //only use first shot
            if (Main.rand.Next(10) == 1) { return false; } // if number is 1, don't use ammo also
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 0)
            {
                speedX += 0.5f * (Main.rand.NextFloat() - 0.5f);
                speedY += 0.5f * (Main.rand.NextFloat() - 0.5f);
                return true;
            }
            else
            {
                float veloX, veloY;
                int numShots = 4 + Main.rand.Next(3);
                for (int i = 0; i < numShots; i++)
                {
                    veloX = speedX + 5f * (Main.rand.NextFloat() - 0.5f);
                    veloY = speedY + 5f * (Main.rand.NextFloat() - 0.5f);
                    Projectile.NewProjectile(position, new Vector2(veloX, veloY),
                        type, damage, knockBack, player.whoAmI);
                }
                return false;
            }
        }
    }
}
