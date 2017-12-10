using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Dual
{
    public class Capacitor : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Capacitor");
            Tooltip.SetDefault(
                "<right> to cast a frost bolt\n" + 
                "Melee attacks grant 80% reduced mana cost");
        }
        public override void SetDefaults()
        {
            item.width = 40;
            item.height = 40;
            item.scale = 1.15f;

            item.autoReuse = true;

            item.UseSound = SoundID.Item1;
            item.useStyle = 5;
            item.useAnimation = 16;
            item.useTime = 16;

            item.melee = true; //melee damage
            item.damage = 17;
            item.knockBack = 4f;

            item.mana = 16;
            item.shoot = ProjectileID.FrostBoltStaff;
            item.shootSpeed = 14f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 2;
            item.value = Item.buyPrice(0, 2);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableDualWeapons) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IceBlade, 1);
            recipe.AddIngredient(ItemID.FallenStar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player) { return true; }

        public override bool CanUseItem(Player player)
        {
            if (PlayerFX.DualItemCanUseItemAlt(player, this,
                1f, 1f,
                1f, 16f / 21f))
            {
                item.useStyle = 5; // Doesn't set for other clients normally
                item.UseSound = null; // Doesn't play for other clients normally
                item.magic = true;
                item.melee = false;
                item.noMelee = true;
                item.shoot = ProjectileID.FrostBoltStaff;
            }
            else
            {
                // we can take advantage of the fact that CanUseItem never gets called by
                // clients if it was an alt function
                item.useStyle = 1;
                item.UseSound = SoundID.Item1;
                item.magic = false;
                item.melee = true;
                item.noMelee = false;
                player.manaCost = 0f;
                item.shoot = 0; // No projectile
            }
            return true;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            player.AddBuff(WeaponOut.BuffIDManaReduction, 180);//3 second buff
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            damage = (int)(damage * 30f / 16f);
            knockBack = 0f;
            return true;
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            if (player.FindBuffIndex(WeaponOut.BuffIDManaReduction) != -1)
            {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 15, (player.velocity.X * 0.2f) + (player.direction * 3), player.velocity.Y * 0.2f, 100, Color.White, 1.3f);
                Main.dust[d].noGravity = true;
            }
            else
            {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 15, (player.velocity.X * 0.2f) + (player.direction * 3), player.velocity.Y * 0.2f, 100, Color.White, 0.6f);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
