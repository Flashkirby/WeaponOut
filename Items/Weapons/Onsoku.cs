using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Dash around like some kind of... cyborg ninja
    /// </summary>
    public class Onsoku : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetDefaults()
        {
            item.name = "Onsoku";
            item.toolTip = "Dashes through enemies";
            item.toolTip2 = "Land after dashing to recharge";
            item.width = 38;
            item.height = 42;

            item.melee = true;
            item.damage = 30;
            item.knockBack = 3;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.useTime = 24;
            item.useAnimation = 24;

            item.shoot = mod.ProjectileType<Projectiles.OnsokuSlash>();
            item.shootSpeed = 16f;

            item.rare = 4;
            item.value = 25000;
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.Muramasa, 1);
                recipe.AddIngredient(ItemID.SoulofLight, 15);
                recipe.AddTile(TileID.AdamantiteForge);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.GetModPlayer<PlayerFX>(mod).dashingSpecialAttack == 0)
            {
                // Allow dash projectile only if landed since last dash
                player.GetModPlayer<PlayerFX>(mod).dashingSpecialAttack = PlayerFX.dashingSpecialAttackOnsoku;
                return true;
            }
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Reset style to swing when neutral
            if (player.itemAnimation == 0)
            {
                item.useStyle = 1;
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if(player.GetModPlayer<PlayerFX>(mod).dashingSpecialAttack == PlayerFX.dashingSpecialAttackOnsoku)
            {
                // Dash with self as hitbox, only when invincible via projectile
                noHitbox = !player.immuneNoBlink;
                if(!noHitbox)
                {
                    Main.SetCameraLerp(0.1f, 10);
                    player.attackCD = 0;
                }
                hitbox = player.getRect();
            }
        }
    }
}
