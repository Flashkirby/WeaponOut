using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Items.Weapons
{
    public class ImmaterialBlade : ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Immaterial Blade";
            item.toolTip = "'Light as a feather'";
            item.width = 40;
            item.height = 40;
            item.scale = 1.15f;

            item.melee = true;
            item.damage = 23;
            item.knockBack = 0;
            item.crit = 15;
            item.autoReuse = false;
            item.useTurn = false;

            item.useStyle = 1;
            item.UseSound = SoundID.Item15;
            item.useTime = 32;
            item.useAnimation = 32;

            item.rare = 2;
            item.value = 57500;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.Sapphire, 10);
            recipe.AddIngredient(ItemID.Diamond, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            player.armorPenetration += 60;
        }

        public override void UseStyle(Player player)
        {
            float cosRot = (float)Math.Cos(player.itemRotation - 0.78f * player.direction * player.gravDir);
            float sinRot = (float)Math.Sin(player.itemRotation - 0.78f * player.direction * player.gravDir);
            for (int i = 0; i < 8; i++)
            {
                float length = (item.width * 1.2f - i * item.width/9) * item.scale + 16; //length to base + arm displacement
                int dust = Dust.NewDust(
                    new Vector2(
                        (float)(player.itemLocation.X + length * cosRot * player.direction),
                        (float)(player.itemLocation.Y + length * sinRot * player.direction)), 
                    0, 0, 15,
                    player.velocity.X * 0.9f,
                    player.velocity.Y * 0.9f, 
                    100, 
                    Color.Transparent, 
                    1.5f);
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].noGravity = true;
            }
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            int length = Main.rand.Next(24);
            int dust = Dust.NewDust(
                    item.position + new Vector2(item.width - length - 5, length),
                    0, 0, 15,
                    item.velocity.X * 5f,
                    item.velocity.Y * 5f,
                    100,
                    Color.Transparent,
                    1.5f);
            Main.dust[dust].velocity *= 0.1f;
            Main.dust[dust].noGravity = true;
        }
    }
}
