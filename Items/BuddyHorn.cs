using System;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class BuddyHorn : ModItem
    {
        public static short customGlowMask = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gathering Horn");
			DisplayName.AddTranslation(GameCulture.Russian, "Рог Призыва");
            Tooltip.SetDefault(
                "Summons a one way portal from your spawn to the current position\n" + 
                "Only for players on your team");
				Tooltip.AddTranslation(GameCulture.Russian, "Создаёт одноразовый портал от спавна к вашей локации\n" +
				"Работает только для вашей команды");
            customGlowMask = WeaponOut.SetStaticDefaultsGlowMask(this);
        }
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.useStyle = 4;
            item.useTime = 100;
            item.UseSound = mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/SummonHorn");
            item.useAnimation = 100;

            item.glowMask = customGlowMask;
            item.rare = 2;
            item.value = Item.sellPrice(0, 0, 25, 0);
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SunplateBlock, 50);
            recipe.AddTile(TileID.SkyMill);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool UseItem(Player player)
        {
            // Only let the player using it spawn projectiles
            // On other clients, a conflict will occur due to spawn positions and entrance despawns
            if (player.whoAmI == Main.myPlayer)
            {
                int offsetY = (int)(-11 * player.gravDir);

                int spawnX = Main.spawnTileX;
                int spawnY = Main.spawnTileY;
                player.FindSpawn();
                if (player.SpawnX > 0) spawnX = player.SpawnX;
                if (player.SpawnY > 0) spawnY = player.SpawnY;

                int entrance = mod.ProjectileType<Projectiles.BuddyPortalEntrance>();
                int exit = mod.ProjectileType<Projectiles.BuddyPortalExit>();

                foreach (Projectile projectile in Main.projectile)
                {
                    if (projectile.active && projectile.owner == player.whoAmI)
                    {
                        if (projectile.type == entrance) projectile.Kill();
                        if (projectile.type == exit) projectile.Kill();
                    }
                }

                //MP NOTE: Won't spawn more than 630ft away

                // Entrance
                Projectile.NewProjectile(new Vector2(
                    spawnX, spawnY).ToWorldCoordinates() - new Vector2(0, 40),
                    default(Vector2),
                    entrance, 0, 0f, player.whoAmI);
                // Exit
                Projectile.NewProjectile(new Vector2(
                    (int)player.Center.X,
                    (int)player.Center.Y + offsetY
                    ), default(Vector2),
                    exit, 0, 0f, player.whoAmI);
            }
            return true;
        }
    }
}
