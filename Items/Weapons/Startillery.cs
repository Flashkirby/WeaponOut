using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Star cannon converion for the frugal
    /// </summary>
    public class Startillery : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableBasicContent;
        }

        public const int penetrateBonus = 4;

        public override void SetStaticDefaults()
        {
            item.CloneDefaults(ItemID.StarCannon);
            DisplayName.SetDefault("Startillery Staff");
        }
        public override void SetDefaults()
        {
            item.damage = 230;
            item.useTime = 80;
            item.useAnimation = 81;
            item.UseSound = SoundID.DD2_BallistaTowerShot;
            item.shootSpeed += 10f;
        }
        public override void AddRecipes()
        {
            //Conversion to, should be expensive enough to dissuade replacement as reforging
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.StarCannon, 1);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
            //Conversion from
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(this, 1);
            recipe.AddIngredient(ItemID.FallenStar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(ItemID.StarCannon);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position -= new Vector2(speedX * 2, speedY * 2);
            return true;
        }

        public override void HoldItem(Player player)
        {
            foreach(Projectile p in Main.projectile)
            {
                if(p.active && p.owner == player.whoAmI && p.type == ProjectileID.FallingStar && p.damage > 150 && p.damage < 1000)
                {
                    Gore g = Main.gore[Gore.NewGore(p.position, new Vector2(p.velocity.X * 0.05f, p.velocity.Y * 0.05f), Main.rand.Next(16, 18), 1f)];
                    g.velocity.Y += 2;
                    for (int i = 0; i < 2; i++)
                    {
                        Dust d = Main.dust[Dust.NewDust(p.position, p.width, p.height, 57 + Main.rand.Next(2), p.velocity.X * 0.3f, p.velocity.Y * 0.3f, 150, default(Color), 1.2f)];
                        d.velocity *= Main.rand.NextFloatDirection() * 0.5f;
                    }
                }
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4, 6);
        }
    }
}
