using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class SteamPersuader : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableDualWeapons;
        }

        HelperDual dual;
        HelperDual Dual { get { if (dual == null) { HelperDual.OnCraft(this); } return dual; } }
        public override void SetDefaults()
        {
            item.name = "Steam Persuader";
            item.toolTip = @"No knockback on normal shots
Four round burst
Only the first shot consumes ammo
10% chance to not consume ammo
<right> to fire a spread shot";
            item.width = 62;
            item.height = 20;

            item.UseSound = SoundID.Item31;
            item.useStyle = 5;
            item.useAnimation = 12; //4 shots
            item.useTime = 3;
            item.reuseDelay = 14;//12 + 14 = 26 usetime
            item.autoReuse = true;

            item.ranged = true; //melee damage
            item.noMelee = true;
            item.damage = 21;
            item.knockBack = 8f;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = 10;
            item.shootSpeed = 9f;

            item.rare = 5;
            item.value = Item.sellPrice(0, 3, 0, 0);

            dual = new HelperDual(item, true);
            dual.UseSound = SoundID.Item38;
            dual.UseAnimation = 28;
            dual.UseTime = 28;
            dual.ReuseDelay = 0;

            dual.KnockBack = 8f;

            dual.ShootSpeed = 6f;

            dual.FinishDefaults();
            //end by setting default values
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ClockworkAssaultRifle, 1);
            recipe.AddIngredient(ItemID.Cog, 7);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void OnCraft(Recipe recipe)
        {
            HelperDual.OnCraft(this);
            base.OnCraft(recipe);
        }

        public override bool AltFunctionUse(Player player) { return true; }
        public override void UseStyle(Player player)
        {
            Dual.UseStyleMultiplayer(player);
            PlayerFX.modifyPlayerItemLocation(player, -28, -2);
        }
        public override bool CanUseItem(Player player)
        {
            Dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player)
        {
            Dual.HoldStyle(player);
            base.HoldStyle(player);
        }

        // Act like clockwork gun
        public override bool ConsumeAmmo(Player player)
        {
            if (player.itemAnimation < item.useAnimation - 1) { return false; } //only use first shot
            if (Main.rand.Next(10) == 0) { return false; } // if number is 0, don't use ammo also
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            //Main.NewText(player.itemAnimation + " == " + (player.itemAnimationMax - 1));
            if (player.altFunctionUse == 0)
            {
                speedX += 0.5f * (Main.rand.NextFloat() - 0.5f);
                speedY += 0.5f * (Main.rand.NextFloat() - 0.5f);
                knockBack = 0f;
                return true;
            }
            else if (player.itemAnimation == player.itemAnimationMax -1)
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
            }
            return false;
        }
    }
}
