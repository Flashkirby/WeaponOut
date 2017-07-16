using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class MirrorBadge : ModItem
    {
        public const int reflectDelay = 90;

        public override bool Autoload(ref string name)
        {
            return ModConf.enableAccessories;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mirror Badge");
            Tooltip.SetDefault(
                "Immunity to petrification\n" +
                "Reflects most projectiles\n" +
                "'A mark of courage, polished to perfection'");
        }
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 20;
            item.rare = 8;
            item.value = Item.sellPrice(0, 3, 0, 0);
            item.accessory = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PocketMirror, 1);
            recipe.AddIngredient(ItemID.ShroomiteBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Stoned] = true;

            PlayerFX pFX = player.GetModPlayer<PlayerFX>(mod);
            if (pFX.reflectingProjectileDelay <= 0)
            {
                player.AddBuff(WeaponOut.BuffIDMirrorBarrier, 2);
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (Main.rand.Next(15) == 0)
            {
                int dustIndex = Dust.NewDust(item.position, item.width, item.height, 43, 0, 0, 100, Color.White, 0.3f);
                Main.dust[dustIndex].velocity *= 0.1f;
                Main.dust[dustIndex].fadeIn = 1f;
            } 
            return true;
        }
    }
}
