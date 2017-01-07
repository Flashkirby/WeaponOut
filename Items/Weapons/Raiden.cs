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
    /// Dash around like some kind of... cyborg ninja
    /// </summary>
    public class Raiden : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public const int focusTime = 60;
        public const int focusRange = 128;

        private bool slashFlip = false;

        public override void SetDefaults()
        {
            item.name = "Raiden";
            item.toolTip = "Stand still to focus on nearby foes";
            item.width = 40;
            item.height = 40;

            item.melee = true;
            item.damage = 35;
            item.knockBack = 5;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.useTime = 30;
            item.useAnimation = 30;

            item.shoot = mod.ProjectileType("Hayauchi");
            item.shootSpeed = 16f;

            item.rare = 4;
            item.value = 25000;

            slashFlip = false;
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(mod.ItemType<Hayauchi>(), 1);
                recipe.AddIngredient(mod.ItemType<Onsoku>(), 1);
                recipe.AddIngredient(ItemID.HallowedBar, 5);
                recipe.AddTile(TileID.AdamantiteForge);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public int getPatience(Player player)
        {
            return player.GetModPlayer<PlayerFX>(mod).itemSkillDelay;
        }
        public void updatePatience(Player player, int valueEquals)
        {
            player.GetModPlayer<PlayerFX>(mod).itemSkillDelay = valueEquals;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (getPatience(player) >= focusTime)
            {
                Projectile.NewProjectile(
                    player.position.X,
                    player.position.Y, 0, 0,
                    mod.ProjectileType(item.name),
                    player.direction, 0, player.whoAmI);
            }
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Reset style to swing when neutral
            if (player.itemAnimation == 0)
            { item.useStyle = 1; }
            else { item.useStyle = 0; }

            if(player.itemAnimation == 1) // when almost done, swap around
            { slashFlip = !slashFlip; }
        }

        public override bool UseItemFrame(Player player)
        {
            //counts down from 1 to 0
            float anim = player.itemAnimation / (float)(player.itemAnimationMax);
            int frames = player.itemAnimationMax - player.itemAnimation;

            // animation frames;
            int start, swing, swing2, end;

            if (slashFlip)
            {
                start = 4 * player.bodyFrame.Height;
                swing = 3 * player.bodyFrame.Height;
                swing2 = 2 * player.bodyFrame.Height;
                end = 1 * player.bodyFrame.Height;
            }
            else
            {
                start = 1 * player.bodyFrame.Height;
                swing = 2 * player.bodyFrame.Height;
                swing2 = 3 * player.bodyFrame.Height;
                end = 4 * player.bodyFrame.Height;
            }

            // Actual animation
            if (anim > 0.9)
            {
                player.bodyFrame.Y = start;
            }
            else if (anim > 0.75f)
            {
                player.bodyFrame.Y = swing;
            }
            else if (anim > 0.6f)
            {
                player.bodyFrame.Y = swing2;
            }
            else
            {
                player.bodyFrame.Y = end;
            }

                return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {

            float anim = player.itemAnimation / (float)player.itemAnimationMax;
            // hit box only active during these frames
            if (anim <= 0.9f && anim > 0.6f)
            {
                int offsetX = 0;
                if(anim > 0.75f) //first half of wing, covers behind
                {
                    hitbox.Width = 48;
                    hitbox.Height = 92;
                    offsetX = 6;
                }
                else
                {
                    hitbox.Width = 80;
                    hitbox.Height = 96;
                    offsetX = 32;
                }
                // Center hitbox and offset accordingly
                hitbox.X = (int)player.Center.X - hitbox.Width / 2
                    + offsetX * player.direction;
                hitbox.Y = (int)player.Center.Y - hitbox.Height / 2;

                if(player.attackCD > 1) player.attackCD = 1;
            }
            else
            {
                noHitbox = true;
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            float ySpeed = 4f;
            Vector2 pos = hitbox.TopLeft();
            if (slashFlip)
            {
                ySpeed = -4f;
                pos = hitbox.BottomLeft();
            }
            pos -= new Vector2(4, 4);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(pos, hitbox.Width, 0,
                    159, player.velocity.X * 0.5f, ySpeed,
                    0, Color.Green, 0.7f);
            }
        }
    }
}
