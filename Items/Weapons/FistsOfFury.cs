using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class FistsOfFury : ModItem
    {
        public FistStyle fist;
        public override void SetDefaults()
        {
            fist = new FistStyle(item, 5);

            item.name = "Fists of Fury";
            item.toolTip2 = "Unleashes a fiery blast";
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.autoReuse = true;
            item.useAnimation = 30;
            item.useTime = 60;

            item.width = 28;
            item.height = 28;
            item.damage = 15;
            item.shoot = mod.ProjectileType("FistFlameBlast");
            item.shootSpeed = 6;
            item.knockBack = 4f;
            item.useSound = 32;

            item.value = Item.sellPrice(0, 0, 24, 0);
            item.rare = 2;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool UseItemFrame(Player player)
        {
            FistStyle.UseItemFrame(player);
            FistStyle.UseItemFramePauseCharge(player, item);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = FistStyle.UseItemHitbox(player, ref hitbox, 12);
            if (!noHitbox)
            {
                Vector2 velo = FistStyle.GetFistVelocity(player) * -2f + player.velocity * 0.5f;
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 174, velo.X, velo.Y);
                Main.dust[d].noGravity = true;
                for (int i = 0; i < 10; i++)
                {
                    d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 174, velo.X * 1.2f, velo.Y * 1.2f);
                    Main.dust[d].noGravity = true;
                }
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (fist.OnHitNPC(player, target, true))
            {
                //set on fire
                target.AddBuff(BuffID.OnFire, 300);
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            knockBack = 0f;
            return true;
        }
    }
}
