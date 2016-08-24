using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// ai[0] = charge
    /// ai[1] = explosion state (charging, released, exploding
    /// localai[0] = charging
    /// localai[1] = basedmg
    /// damage * (charge ^ 2) x35-870-3500
    /// log (charge * 10) x1-1.84-2.17
    /// 
    /// Possibly increase size by projectile velocity
    /// </summary>
    public class Explosion : ModProjectile
    {
        public const int chargeTicksMax = 240; //5 ticks per update
        public const int castTicksTime = 5; //time per charge spent casting
        public const int fireTicksTime = 3; //time per charge spent exploding (penetrating projs gives npcs 10tick immune)
        public const float manaIncrease = 0.5f;//additive mana multiplier per increase
        public const float explosionScale = 1.15f; //width * this^15
        //10, 15, 20, 25 etc.

        private int ChargeLevel { get { return (int)projectile.ai[0]; } set { projectile.ai[0] = value; } }
        private int ExplosionState { get { return (int)projectile.ai[1]; } set { projectile.ai[1] = value; } }
        private int ChargeTick { get { return (int)projectile.localAI[0]; } set { projectile.localAI[0] = value; } }
        private int Damage { get { return (int)projectile.localAI[1]; } set { projectile.localAI[1] = value; } }
        private int manaCost { get { return (int)(Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].mana * (1f + (ChargeLevel + 1) / 2f)); } }

        public const int maxCharge = 15;
        //Titanium/Hallowed Headgear, Diamond Robe, 
        //Crystal Ball Buff, Arcane Band of Starpower, Arcane Mana Regeneration Band, 
        //Arcane Magic Cuffs, three Arcane Accessories (one extra with Demon Heart)
        //For 600 mana, with max reduced mana mana cost from 10 -> 8 via Masterful modifier
        //sum 8 * (1+k/2), k=0 to n
        //chg 1, 2   3   4   5   6    7    8    9    10   11   12   13   14   15   16
        //    8, 20, 36, 56, 80, 108, 140, 176, 216, 260, 308, 360, 416, 476, 540, 608
        //NOTE: it seems 400 is maximum mana allowed, Must consume a mana pot at tier 6 to cast full power
        //

        private Vector2 zoomDiff = new Vector2();

        public override void SetDefaults()
        {
            projectile.name = "Explosion"; //great explosion, mega explosion, extreme explosion, ultra explosion, grand explosion
            projectile.width = 100;
            projectile.height = 100;

            projectile.timeLeft = 3600;
            projectile.penetrate = -1;

            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            player.heldProj = projectile.whoAmI; //set to this

            projectile.velocity = Vector2.Zero;
            if (ExplosionState == 0f)
            {
                chargeState(player);
            }
            else if (ExplosionState == 1f)
            {
                castState(player);
            }
            else if (ExplosionState == 2f)
            {
                fireState(player);
            }
        }


        private static bool canChannel(Player player)
        {
            return !player.dead && !player.frozen && !player.stoned && !player.webbed && !player.tongued;
        }
        /// <summary>
        /// Does all the "is holding and facing this projectile" things
        /// </summary>
        /// <param name="player"></param>
        private void playerFaceProjectileChannel(Player player)
        {
            if (projectile.damage != 0)
            {
                Damage = projectile.damage;
                projectile.damage = 0;

                Main.NewText("Damage: " + Damage);
            }

            Vector2 vectorDiff = player.Center - projectile.Center;
            if (vectorDiff.X < 0f)
            {
                player.ChangeDir(1);
                projectile.direction = 1;
            }
            else
            {
                player.ChangeDir(-1);
                projectile.direction = -1;
            }

            //stop generating new projectiles
            player.itemAnimation = player.itemAnimationMax;
            player.itemTime = player.itemAnimationMax;

            //point at projectile
            player.itemRotation = (vectorDiff * -1f * (float)projectile.direction).ToRotation();
            projectile.spriteDirection = ((vectorDiff.X > 0f) ? -1 : 1);
        }



        private void chargeState(Player player)
        {
            Vector2 staffTip = player.MountedCenter + new Vector2(
                player.inventory[player.selectedItem].width * 1.1f * (float)Math.Cos(player.itemRotation),
                player.inventory[player.selectedItem].width * 1.1f * (float)Math.Sin(player.itemRotation)
            ) * player.direction;
            int d = Dust.NewDust(staffTip - new Vector2(3 - player.direction * 2 + ChargeLevel * 0.05f, 3 + ChargeLevel * 0.05f),
                0, 0, 174, 0, 0, 0, Color.White, 0.3f + (0.1f * ChargeLevel));
            Main.dust[d].noGravity = true;
            Main.dust[d].velocity *= 0.3f;

            //use manaflower
            if (manaCost > player.statMana)
            {
                if (player.manaFlower)
                {
                    player.QuickMana();
                }
            }

            //cancelled if no longer channelling or can't act, also always channel if moving
            if ((player.channel || player.velocity.X != 0 || player.velocity.Y != 0) 
                && canChannel(player) && ChargeLevel < maxCharge && player.statMana >= manaCost)
            {
                //hold player usage
                playerFaceProjectileChannel(player);
                ChargeTick += 1; //increase charge a bit
                if (player.velocity.X == 0 && player.velocity.Y == 0)
                {
                    ChargeTick += 4; //increase charge

                    //extra dust to indicate charging faster
                    d = Dust.NewDust(staffTip - new Vector2(3 - player.direction * 2 + ChargeLevel * 0.05f, 3 + ChargeLevel * 0.05f),
                        0, 0, 174, 0, 0, 0, Color.White, 0.5f + (0.1f * ChargeLevel));
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.7f;
                }
                if (ChargeTick >= chargeTicksMax)
                {
                    ChargeTick = 0;
                    ChargeLevel++;
                    projectile.timeLeft += chargeTicksMax;

                    float recentre = projectile.width;
                    projectile.Size *= explosionScale;
                    recentre = (projectile.width - recentre) / 2f;
                    projectile.Center -= new Vector2(recentre, recentre * 1.75f);

                    player.CheckMana(manaCost, true);

                    Main.NewText("Increase... tier " + ChargeLevel + " | current manacost: " + manaCost);
                }
            }
            else
            {
                ExplosionState = 1;
                projectile.netUpdate = true;
                Main.NewText("Changing to CastState");
            }

            for (int i = 0; i < ChargeLevel + 1; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0, 0);
            }
        }
        
        private bool start = true;
        private void castState(Player player)
        {
            int chargeTime = (1 + ChargeLevel) * castTicksTime;
            if (start) { ChargeTick = chargeTime; start = false; }

            //cancelled if can't act
            if (canChannel(player))
            {
                player.itemRotation = -1.5708f * player.direction;
            }

            if (ChargeTick > 0)
            {
                ChargeTick--;

                //player stays still to cast
                player.itemAnimation = player.itemAnimationMax / 2;
                player.itemTime = player.itemAnimationMax / 2;
                player.noKnockback = true;
                player.velocity /= 2f;

                // -- casting code --
                
                Vector2 staffTip = player.MountedCenter + new Vector2(
                    player.inventory[player.selectedItem].width * 1.1f * (float)Math.Cos(player.itemRotation),
                    player.inventory[player.selectedItem].width * 1.1f * (float)Math.Sin(player.itemRotation)
                ) * player.direction;

                //glow down
                Main.NewText("casting... " + ((float)ChargeTick / castTicksTime));
                int d = Dust.NewDust(staffTip - new Vector2(3 - player.direction * 2 + ChargeLevel * 0.05f, 3 + ChargeLevel * 0.05f),
                    0, 0, 174, 0, 0, 0, Color.White, 0.5f + (0.1f * ((float)ChargeTick / castTicksTime)));
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = new Vector2(Main.dust[d].velocity.X * 0.1f, -10f + Main.dust[d].velocity.Y);
            }
            else
            {
                Main.NewText("Changing to FireState");
                ExplosionState = 2;
                projectile.netUpdate = true;
            }
        }
        
        private void fireState(Player player)
        {
            if (ChargeTick >= 0)
            {
                ChargeTick = -1;
                projectile.timeLeft = fireTicksTime * (1 + ChargeLevel);

                if (ChargeLevel > 0)
                {
                    //goes up to x1000 damage
                    Damage = Damage + (int)(Damage * Math.Pow(2, ChargeLevel * 10f / maxCharge));
                    projectile.knockBack *= 1 + (int)Math.Log10(ChargeLevel * 10);
                }

                //damage is reduced over a period of time due to fire time
                //in this case, since it hits every 10 ticks
                int divDamage = Damage / (1 + (projectile.timeLeft / 10));
                projectile.damage = divDamage;
                Main.NewText("lifetime: " + projectile.timeLeft + " | dmg: " + divDamage);
            }

            for (int i = 0; i < (ChargeLevel + 1) * 3; i++)
            {
                int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0, 0, 0, Color.White, 1 + ChargeLevel * 0.5f);
                Main.dust[d].fadeIn = 0.1f;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 1.5f;
            }

        }
    }
}
