using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// A flashier and higher DPS demon scythe/space gun hybrid
    /// Pierces infinitely but damage falls off at a distance
    /// 
    /// </summary>
    public class DemonBlaster : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }
        public override void SetDefaults()
        {
            item.name = "Demon Blaster";
            item.toolTip = "Fires an unholy ray";
            item.width = 32;
            item.height = 18;
            item.scale = 0.9f;

            item.magic = true;
            item.mana = 10;
            item.damage = 38; //DPS 162
            item.knockBack = 0;
            item.autoReuse = true;

            item.noMelee = true;
            item.shoot = mod.ProjectileType("DemonBlast");
            item.shootSpeed = 30;

            item.useStyle = 5;
            item.UseSound = SoundID.Item12;
            item.useTime = 14;
            item.useAnimation = 14;

            item.rare = 4;
            item.value = 18000;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SpaceGun, 1);
            recipe.AddIngredient(ItemID.DemonScythe, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UseStyle(Player player)
        {
            PlayerFX.modifyPlayerItemLocation(player, -4, 0);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            lightColor.R = 255;
            lightColor.G = Math.Max((byte)119, lightColor.G);
            lightColor.B = 255;
            return lightColor;
        }
    }
}
