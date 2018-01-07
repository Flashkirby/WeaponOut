using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Accessories
{
    public class LunarEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Luminous Emblem");
            DisplayName.AddTranslation(GameCulture.Chinese, "辉赫徽章");
			DisplayName.AddTranslation(GameCulture.Russian, "Космическая Эмблема");

            Tooltip.SetDefault(
                "Supercharges weapons to their lunar potential\n" +
                "Increases combat capabilities" +
                "'One more dance'");
            Tooltip.AddTranslation(GameCulture.Chinese, "激发武器的月之潜力\n提高作战能力\n“再跳一次舞”");
			Tooltip.AddTranslation(GameCulture.Russian, 
				"Заряжает оружие космической энегрией\n" +
                "Улучшает все виды оружия" +
                "'Ещё один танец'");

        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.rare = 10;
            item.value = Item.sellPrice(0, 50, 0, 0);
            item.accessory = true;
            item.expert = true;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableEmblems) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<HeliosphereEmblem>());
            recipe.AddIngredient(mod.ItemType<WormholeEmblem>());
            recipe.AddIngredient(mod.ItemType<AccretionEmblem>());
            recipe.AddIngredient(mod.ItemType<PerihelionEmblem>());
            recipe.AddIngredient(mod.ItemType<SupernovaEmblem>());
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, -1);
            player.meleeSpeed += 0.12f;
            player.magmaStone = true;
            player.ammoCost80 = true;
            player.statManaMax2 += 20;
            player.thrownVelocity += 0.3f;
            player.maxMinions += 1;
            if (hideVisual) return;
            player.GetModPlayer<PlayerFX>(mod).lunarRangeVisual = true;
            player.GetModPlayer<PlayerFX>(mod).lunarMagicVisual = true;
            player.GetModPlayer<PlayerFX>(mod).lunarThrowVisual = true;

            Vector2 hand = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
            if (player.direction != 1) {
                hand.X = (float)player.bodyFrame.Width - hand.X;
            }
            if (player.gravDir != 1f) {
                hand.Y = (float)player.bodyFrame.Height - hand.Y;
            }
            hand -= new Vector2((float)(player.bodyFrame.Width - player.width), (float)(player.bodyFrame.Height - 42)) / 2f;
            Vector2 dustPos = player.RotatedRelativePoint(player.position + hand, true) - player.velocity;

            for (int i = 0; i < 3; i++) {
                Dust d = Dust.NewDustDirect
                    (player.Center, 0, 0, DustID.RainbowMk2, (float)(player.direction * 2), 0f,
                    100, Main.DiscoColor, 0.9f);
                d.position = dustPos + player.velocity;
                d.velocity = Utils.RandomVector2(Main.rand, -0.5f, 0.5f) + player.velocity * 0.5f;
                d.noGravity = true;
            }
        }
    }
}
