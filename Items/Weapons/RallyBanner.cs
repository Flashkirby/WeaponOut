using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Provides AoE banner bonus to players "nearby".
    /// Buff DPS pays itself back in a 7-8 player team,
    /// </summary>
    public class RallyBanner : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public const int buffRadius = 1024;
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Umbrella);
            item.name = "Rally Banner";
            item.width = 28;
            item.height = 48;
            item.toolTip = "Increases stats for your team whilst held";

            item.value = 3000;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 5);
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.anyWood = true;
            recipe.AddIngredient(ItemID.FallenStar, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (player.team == 0)
            {
                // Only buff self if no team
                player.AddBuff(mod.BuffType<Buffs.RallyBanner>(), 2);
            }
            else
            {
                foreach (Player p in Main.player)
                {
                    if (!p.active || p.dead || p.team == 0) continue;
                    if (p.team == player.team)
                    {
                        // In bounding box
                        if (p.position.X >= player.position.X - buffRadius &&
                            p.position.X <= player.position.X + buffRadius &&
                            p.position.Y >= player.position.Y - buffRadius &&
                            p.position.Y <= player.position.Y + buffRadius
                            )
                        {
                            player.AddBuff(mod.BuffType<Buffs.RallyBanner>(), 2);
                        }
                    }
                }

            }
        }

        public override void HoldStyle(Player player)
        {
            player.itemRotation += (0.79f * (float)player.direction) * player.gravDir;
            player.itemLocation.X -= (float)(item.width / 2) * (float)player.direction;
        }
    }
}
