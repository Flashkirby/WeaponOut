using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class Reverb : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableBasicContent;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reverb");
            Tooltip.SetDefault(
                "<right> to cast reverse bolt");
        }
        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 14;
            item.scale = 0.9f;

            item.magic = true;
            item.mana = 10;
            item.damage = 43;
            item.knockBack = 6f;
            item.autoReuse = true;
            
            item.shoot = mod.ProjectileType("Reverb");
            item.shootSpeed = 14;

            item.useStyle = 5;
            Item.staff[item.type] = true;
            item.UseSound = SoundID.Item28;
            item.useTime = 20;
            item.useAnimation = 20;

            item.rare = 5;
            item.value = Item.sellPrice(0, 4, 0, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UnicornHorn, 2);
            recipe.AddIngredient(ItemID.SoulofSight, 15);
            recipe.AddIngredient(ItemID.SoulofLight, 3);
            recipe.AddIngredient(ItemID.SoulofNight, 3);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void UseStyle(Player player)
        {
            PlayerFX.modifyPlayerItemLocation(player, -4, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int direction = (player.altFunctionUse == 0) ? 1 : -1;

            if (direction < 0)
            {
                position.X += speedX * item.useTime;
                position.Y += speedY * item.useTime;
            }

            Vector2 trueVelo = new Vector2(speedX * direction, speedY * direction);
            Projectile.NewProjectile(position, trueVelo, type,
                damage, knockBack, player.whoAmI, 0f, direction);
            return false;
        }
    }
}