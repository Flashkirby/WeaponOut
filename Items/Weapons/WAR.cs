using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// dedicated to the grandfather of a good friend.
    /// </summary>
    public class WAR : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableBasicContent;
        }

        public const int penetrateBonus = 4;

        public override void SetDefaults()
        {
            item.name = "Woods' Antimat Rifle";
            item.toolTip = "Bullets ignore cover\n'See ya space cowboy'";
            item.width = 80;
            item.height = 26;

            item.ranged = true;
            item.damage = 300;
            item.crit = 16;
            item.knockBack = 5;

            item.noMelee = true;
            item.useAmmo = AmmoID.Bullet;
            item.shoot = 14;
            item.shootSpeed = 16;

            item.useStyle = 5;
            item.UseSound = SoundID.Item38;
            item.useTime = 80;
            item.useAnimation = 80;

            item.rare = 9;
            item.value = 12000000;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SniperRifle, 1);
            recipe.AddIngredient(ItemID.FragmentVortex, 16);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UseStyle(Player player)
        {
            player.scope = true;
            PlayerFX.modifyPlayerItemLocation(player, -24, -2);
            modBullet(player);
        }

        public override bool HoldItemFrame(Player player)
        {
            player.scope = true;
            //modBullet(player);
            return false;
        }

        private int flipFlop;
        /// <summary>
        /// find and modify all bullets owned by the player.
        /// speed bullet by 3, add +3 penetrate
        /// </summary>
        /// <param name="player"></param>
        private void modBullet(Player player)
        {
            if (flipFlop < 1) { flipFlop = 1; } else { flipFlop = -1; }
            Projectile check = new Projectile();
            foreach (Projectile p in Main.projectile)
            {
                if (!p.active || 
                    !p.friendly || 
                    !p.ranged ||
                    p.arrow ||
                    p.npcProj ||
                    p.owner != player.whoAmI) continue;
                //if (Main.netMode == 1) Main.NewText("proj: " + p.name + " to be modded - " + p.penetrate + " | " + p.maxPenetrate);
                if (p.width == (int)(4 * p.scale) && p.height == (int)(4 * p.scale))//bullets are all this size
                {
                    check.SetDefaults(p.type);
                    //mod this projectile
                    if (check.maxPenetrate == p.maxPenetrate &&
                        check.MaxUpdates == p.MaxUpdates &&
                        check.timeLeft == (p.timeLeft + p.MaxUpdates)) //unmodded
                    {
                        p.maxPenetrate += penetrateBonus;
                        p.penetrate = p.maxPenetrate;
                        p.MaxUpdates = p.MaxUpdates * 2 + 2;
                        p.timeLeft /= 2;


                        for (int i = 0; i < 5; i++)
                        {
                            int smoke = Dust.NewDust(p.position - p.Size / 2 + p.velocity * 3, 1, 1, 31, 0, 0, 100, default(Color), 3f);
                            Main.dust[smoke].noGravity = true;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            int smoke = Dust.NewDust(p.position - p.Size / 2 + p.velocity * 5, 1, 1, 31, p.velocity.X * 0.5f, p.velocity.Y * 0.5f, 100, default(Color), 2f);
                            Main.dust[smoke].noGravity = true;
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            int smoke = Dust.NewDust(p.position - p.Size / 2 + p.velocity * 7, 1, 1, 31, p.velocity.X, p.velocity.Y, 100, default(Color), 1f);
                            Main.dust[smoke].noGravity = true;
                        }
                        p.netUpdate = true;

                    }

                    if (p.penetrate < p.maxPenetrate)
                    {
                        Main.PlaySound(2, (int)p.position.X, (int)p.position.Y, 37);
                        for (int i = 0; i < p.penetrate; i++)
                        {
                            Dust.NewDust(p.position, 0, 0, 130, p.oldVelocity.X * 0.2f, p.oldVelocity.Y * 0.2f, 100, default(Color), 1f);
                            Dust.NewDust(p.position, 0, 0, 130, p.oldVelocity.X * 0.4f, p.oldVelocity.Y * 0.4f, 100, default(Color), 1.1f);
                            Dust.NewDust(p.position, 0, 0, 130, p.oldVelocity.X * 0.6f, p.oldVelocity.Y * 0.6f, 100, default(Color), 1.2f);
                        }
                        p.maxPenetrate = p.penetrate;
                    }

                    p.tileCollide = p.penetrate <= penetrateBonus;
                    p.ignoreWater = p.tileCollide;

                    //interpolate between old and new position
                    float interpolate = p.MaxUpdates * 4;
                    Vector2 diffPos = ((p.position + p.velocity * p.extraUpdates) - p.oldPosition) / interpolate;
                    Vector2 position;
                    for (int i = 0; i < interpolate; i++)
                    {
                        position = (p.oldPosition + diffPos * i) - p.Size / 2;

                        //add tracer effects
                        int tracer = Dust.NewDust(position, 0, 0, 6, 0, 0, 100, default(Color), (p.penetrate * 0.5f + (Main.rand.Next(50) * 0.01f)));
                        Main.dust[tracer].noGravity = true;
                        Main.dust[tracer].velocity = new Vector2(0, 0);

                        if (!p.tileCollide)
                        {
                            //sine wave positions
                            Vector2 DustTopPos = position +
                                new Vector2((float)(15 * Math.Sin((i * 3.141f) / interpolate) * Math.Cos(p.rotation)),
                                (float)(15 * Math.Sin((i * 3.141f) / interpolate) * Math.Sin(p.rotation)));
                            Vector2 DustBotPos = position +
                                new Vector2((float)(-15 * Math.Sin((i * 3.141f) / interpolate) * Math.Cos(p.rotation)),
                                (float)(-15 * Math.Sin((i * 3.141f) / interpolate) * Math.Sin(p.rotation)));

                            int sineTop = Dust.NewDust(DustTopPos, 0, 0, 131 + flipFlop, 0, 0, 100, default(Color), 0.9f);
                            Main.dust[sineTop].noGravity = true;
                            Main.dust[sineTop].velocity = new Vector2(0, 0);
                            int sineBot = Dust.NewDust(DustBotPos, 0, 0, 131 - flipFlop, 0, 0, 100, default(Color), 0.9f);
                            Main.dust[sineBot].noGravity = true;
                            Main.dust[sineBot].velocity = new Vector2(0, 0);
                        }
                    }
                }
            }
        }
    }
}
