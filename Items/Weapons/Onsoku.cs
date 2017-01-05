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
    /// Stand (mostly) still to charge a slash, messes with hitboxes etc.
    /// drawstrike does quad damage, with added crit for a total of x8
    /// 35 * 8 == 280
    /// Draw Strike speed = 80 + 20 + 15 == 115
    /// Draw Strike DPS = 146
    /// hey, its me, jetstream sammy
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
            item.toolTip = "Dash forward and strike a single foe";
            item.toolTip2 = "'Like a leaf in the wind'";
            item.width = 38;
            item.height = 42;

            item.melee = true;
            item.damage = 30;
            item.knockBack = 3;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.useTime = 20;
            item.useAnimation = 20;

            item.shoot = mod.ProjectileType<Projectiles.OnsokuSlash>();
            item.shootSpeed = 16f;

            item.rare = 5;
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
                player.GetModPlayer<PlayerFX>(mod).dashingSpecialAttack = PlayerFX.dashingSpecialAttackOnsoku;
                return true;
            }
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation == 0)
            {
                item.useStyle = 1;
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if(player.GetModPlayer<PlayerFX>(mod).dashingSpecialAttack == PlayerFX.dashingSpecialAttackOnsoku)
            {
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
