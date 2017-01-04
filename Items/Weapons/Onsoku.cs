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
    /// Stand (mostly) still to charge a slash, messes with hitboxes etc.
    /// drawstrike does quad damage, with added crit for a total of x8
    /// 35 * 8 == 280
    /// Draw Strike speed = 80 + 20 + 15 == 115
    /// Draw Strike DPS = 146
    /// hey, its me, jetstream sammy
    /// </summary>
    public class Onsoku : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public static Vector2 mouse
        {
            get
            {
                return Main.MouseWorld;
            }
        }
        float targetRange = 256;
        float selectRange = 64;
        float dashSpeed = 10f;
        NPC target = null;

        public override void SetDefaults()
        {
            item.name = "Onsoku";
            item.toolTip = "Dash and strike towards nearby foes";
            item.toolTip2 = "Like a leaf in the wind";
            item.width = 46;
            item.height = 46;

            item.melee = true;
            item.damage = 30;
            item.knockBack = 3;

            item.useStyle = 1;
            item.UseSound = SoundID.Item71;
            item.useTime = 20;
            item.useAnimation = 20;

            item.rare = 5;
            item.value = 25000;
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.BladeofGrass, 1);
                recipe.AddIngredient(ItemID.AnkletoftheWind, 1);
                recipe.AddIngredient(ItemID.Muramasa, 1);
                recipe.AddTile(TileID.AdamantiteForge);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public override bool CanUseItem(Player player)
        {
            return target != null;
        }

        public override void HoldItem(Player player) //called when holding but not swinging
        {
            target = null;
            if (player.whoAmI == Main.myPlayer)
            {
                foreach (NPC n in Main.npc)
                {
                    if (!n.active ||
                        n.life <= 0 ||
                        n.townNPC ||
                        n.friendly ||
                        n.dontTakeDamage
                        //|| n.immortal
                        ) continue;


                    if (Math.Abs(mouse.X - n.Center.X) < selectRange &&
                        Math.Abs(mouse.Y - n.Center.Y) < selectRange)
                    {
                        if (Math.Abs(player.Center.X - n.Center.X) < targetRange &&
                            Math.Abs(player.Center.X - n.Center.X) < targetRange)
                        {
                            target = n;
                        }
                    }
                }
            }
        }

        public override bool HoldItemFrame(Player player) //called on player holding but not swinging
        {
            if (target != null) //ready to slash
            {
                player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                return true;
            }
            return false;
        }

        public override void UseStyle(Player player)
        {
            int frame = player.itemAnimationMax - player.itemAnimation;
            if (frame == 1)
            {
                dashVelocity = target.Center - player.Center;

                // teleport to npc
                Vector2 teleportLoc = target.Bottom - new Vector2(0, player.height);
                player.Teleport(teleportLoc, 1, 0);
                NetMessage.SendData(65, -1, -1, "", 0, (float)player.whoAmI, teleportLoc.X, teleportLoc.Y, 1, 0, 0);
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 4);

                // set dash velocity
                dashVelocity.Normalize();
                dashVelocity *= dashSpeed;
            }
            if (frame > 2 && frame < 6) // frames 3-5
            {
                Main.NewText(frame + ": Dash Frame, immune = " + player.immuneTime);

                // Make player temporarily immune
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 6);
                player.immuneNoBlink = true;
                player.fallStart = (int)(player.position.Y / 16f);
                player.fallStart2 = player.fallStart;
                
                // player dashes
                player.velocity = dashVelocity; // set dash
                player.direction = Math.Sign(dashVelocity.X);
            }
        }

        //Changes the hitbox of this melee weapon when it is used.
        Vector2 dashVelocity = new Vector2();
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int frame = player.itemAnimationMax - player.itemAnimation;

            if(frame > 2 && frame < 6) // frames 3-5
            {
                //centre the hitbox on the enemy
                hitbox = player.getRect();

                // set attack CD to hit 2 enemies
                if(player.attackCD > 1) player.attackCD = 1;
            }
            else
            {
                noHitbox = true;
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // TODO: make trailing effect
            for (int i = 0; i < 20; i++)
            {
                int d = Dust.NewDust(player.Center - new Vector2(4, 4), 0, 0,
                    131, dashVelocity.X, dashVelocity.Y);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
