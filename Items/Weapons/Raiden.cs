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
    /// Yo it's like, a homing weapon or something.
    /// </summary>
    public class Raiden : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public const int focusTime = 60;
        public const int focusRadius = 256;

        public bool slashFlip = false;
        public bool focusSlash = false;

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
            item.useTime = 0;
            item.useAnimation = 30;

            item.rare = 4;
            item.value = 25000;

            slashFlip = false;
            focusSlash = false;
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

        public int getFocus(Player player)
        {
            return player.GetModPlayer<PlayerFX>(mod).itemSkillDelay;
        }
        public void updateFocus(Player player, int valueEquals)
        {
            player.GetModPlayer<PlayerFX>(mod).itemSkillDelay = valueEquals;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return false;
        }

        public override void HoldItem(Player player)
        {
            int focus = getFocus(player);

            // Reset style to swing when neutral
            if (player.itemAnimation == 0)
            {
                item.useStyle = 1;
                focusSlash = false;

                if ((Math.Abs(player.velocity.X) < 1.5f && player.velocity.Y == 0f
                && player.grapCount == 0
                && !player.pulley
                && !(player.frozen || player.webbed || player.stoned || player.noItems)
                )
                || getFocus(player) >= focusTime)
                {
                    if (getFocus(player) < focusTime + 1)
                    {
                        focus++;
                        updateFocus(player, focus);

                        Vector2 vector = player.Center;
                        vector.X += (float)Main.rand.Next(-2048, 2048) * 0.02f;
                        vector.Y += (float)Main.rand.Next(-2048, 2048) * 0.02f;


                        // Charging dust
                        Dust d = Main.dust[Dust.NewDust(
                            vector, 1, 1,
                            235, 0f, 0f, 0,
                            Color.White, 1f)];

                        d.velocity *= 0f;
                        d.scale = Main.rand.Next(70, 85) * 0.01f;
                        // This dust uses fadeIn for homing into players
                        d.fadeIn = Main.myPlayer + 1;
                    }
                }
                else
                {
                    focus = 0;
                    updateFocus(player, focus);
                }
            }
            else
            {
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    int focusType = 0; // focus attack
                    if (!focusSlash)
                    {
                        if (slashFlip)
                        { focusType = -1; } //normal slash flipped
                        else
                        { focusType = 1; } //normal slash 
                    }
                    Projectile.NewProjectile(
                    player.position.X,
                    player.position.Y,
                    0, 0,
                    mod.ProjectileType(item.name), 
                    0, 0f,
                    player.whoAmI,
                    focusType);
                }

                item.useStyle = 0;
                focus = 0;
                updateFocus(player, focus);
            }


            if (focus >= focusTime)
            {
                if (focus == focusTime)
                {
                    Main.PlaySound(2, player.position, 24);
                }

                int amount = 4;
                int alpha = 200;
                if (player.whoAmI == Main.myPlayer)
                {
                    amount = 16;
                    alpha = 100;
                }
                // Range display dust;
                for (int i = 0; i < amount; i++)
                {
                    Vector2 offset = new Vector2();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * focusRadius);
                    offset.Y += (float)(Math.Cos(angle) * focusRadius);
                    Dust dust = Main.dust[Dust.NewDust(
                        player.Center + offset - new Vector2(4, 4), 0, 0,
                        106, 0, 0, alpha, Color.White, 0.3f
                        )];
                    dust.velocity = player.velocity;
                    dust.fadeIn = 0.5f;
                    dust.noGravity = true;
                }

                // Display targets for client
                if (player.whoAmI == Main.myPlayer)
                {
                    List<NPC> targets = Projectiles.Raiden.GetTargettableNPCs(player.Center, focusRadius);
                    Vector2 last = player.Center;
                    for (int i = 0; i < targets.Count; i++)
                    {
                        Projectiles.Raiden.DrawDustToBetweenVectors(last, targets[i].Center, 106);
                        last = targets[i].Center;
                    }
                }
            }

            if(player.itemAnimation == 1) // when almost done, swap around
            { slashFlip = !slashFlip; }
        }

        public override bool HoldItemFrame(Player player) //called on player holding but not swinging
        {
            if (getFocus(player) >= focusTime) //ready to slash
            {
                player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                return true;
            }
            return false;
        }

        public override void UseStyle(Player player)
        {
            if(getFocus(player) >= focusTime)
            {
                focusSlash = true;
                slashFlip = false;
            }
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
            // Set hitboxes accordingly
            if (focusSlash)
            {
                noHitbox = !player.immuneNoBlink;
                if (!noHitbox)
                {
                    Main.SetCameraLerp(0.1f, 10);
                    player.attackCD = 0;
                }
                hitbox = player.getRect();
            }
            else
            {
                NormalHitBox(player, ref hitbox, ref noHitbox);
            }
        }

        private static void NormalHitBox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;
            // hit box only active during these frames
            if (anim <= 0.9f && anim > 0.6f)
            {
                int offsetX = 0;
                if (anim > 0.75f) //first half of wing, covers behind
                {
                    hitbox.Width = 64;
                    hitbox.Height = 92;
                    offsetX = 14;
                }
                else
                {
                    hitbox.Width = 96;
                    hitbox.Height = 112;
                    offsetX = 40;
                }
                // Center hitbox and offset accordingly
                hitbox.X = (int)player.Center.X - hitbox.Width / 2
                    + offsetX * player.direction;
                hitbox.Y = (int)player.Center.Y - hitbox.Height / 2;

                if (player.attackCD > 1) player.attackCD = 1;
            }
            else
            {
                noHitbox = true;
            }
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if(focusSlash)
            {
                damage *= 3;
                crit = true;
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (!focusSlash)
            {
                float ySpeed = 4f;
                Vector2 pos = hitbox.TopLeft();
                if (slashFlip)
                {
                    ySpeed = -4f;
                    pos = hitbox.BottomLeft();
                }
                pos -= new Vector2(4, 4);

                /*
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(pos, hitbox.Width, 0,
                        159, player.velocity.X * 0.5f, ySpeed,
                        0, Color.Green, 0.7f);
                }
                */
            }
        }
    }
}
