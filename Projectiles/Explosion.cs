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
        public override bool Autoload(ref string name) { return true; }
        #region Textures
        public static Texture2D textureTargetS;
        public static Texture2D textureTargetM;
        public static Texture2D textureTargetL;
        public static Texture2D textureTargetXL;
        public static Texture2D textureLaser;
        
        public float[] textureAlphas = new float[maxCharge];
        #endregion

        public const int maxCharge = 15;

        #region Tweaking Values
        public const int explosionSize = 100;

        public const int chargeTicksIdle = 3; //bonus ticks for standing still
        public const int chargeTicksMax = 48 * (1 + chargeTicksIdle); //3 ticks per update
        public const float chargeTickGameMax = (chargeTicksMax / chargeTicksIdle);
        public const int castTicksTime = 5; //time per charge spent casting
        public const int fireTicksTime = 3; //time per charge spent exploding (penetrating projs gives npcs 10tick immune)
        public const float manaIncrease = 0.5f;//additive mana multiplier per increase
        public const int manaMaintainCost = 2;//cost per tick to maintain full charge
        public const float explosionScale = 1.15f; //width * this^15
        public const float farDistance = 2000;
        public const float maxDistance = 2500;
        //10, 15, 20, 25 etc.

        //Titanium/Hallowed Headgear, Diamond Robe, 
        //Crystal Ball Buff, Arcane Band of Starpower, Arcane Mana Regeneration Band, 
        //Arcane Magic Cuffs, three Arcane Accessories (one extra with Demon Heart)
        //For 600 mana, with max reduced mana mana cost from 10 -> 8 via Masterful modifier
        //sum 8 * (1+k/2), k=0 to n
        //chg 1, 2   3   4   5   6    7    8    9    10   11   12   13   14   15   16
        //    8, 20, 36, 56, 80, 108, 140, 176, 216, 260, 308, 360, 416, 476, 540, 608
        //NOTE: it seems 400 is maximum mana allowed, Must consume a mana pot at tier 6 to cast full power
        //

        #endregion

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vermillion Runes");
            if(!Main.dedServ)
            {
                textureTargetS = mod.GetTexture("Projectiles/Explosion_Targetsm");
                textureTargetM = mod.GetTexture("Projectiles/Explosion_Targetmd");
                textureTargetL = mod.GetTexture("Projectiles/Explosion_Targetlg");
                textureTargetXL = mod.GetTexture("Projectiles/Explosion_Targetxl");
                textureLaser = mod.GetTexture("Projectiles/Explosion_Laser");
            }
        }
        public override void SetDefaults()
        {
            projectile.width = explosionSize;
            projectile.height = explosionSize;

            projectile.penetrate = -1;

            //projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.netImportant = true;
        }
        public override bool? CanCutTiles() { return false; }

        private int ChargeLevel { get { return (int)projectile.ai[0]; } set { projectile.ai[0] = value; } }
        private int ChargeTicks { get { return (int)projectile.ai[1]; } set { projectile.ai[1] = value; } }
        private int Damage { get { return (int)projectile.localAI[1]; } set { projectile.localAI[1] = value; } }
        private bool Casting { get { return ChargeTicks < 0; } }
        public override void AI()
        {
            //\/Main.NewText("Level: " + ChargeLevel + " | " + ChargeTicks + " (" + Casting + ")");
            Player player = Main.player[projectile.owner];
            projectile.velocity = Vector2.Zero;

            // Delete if owner is kill
            if (player.dead || !player.active)
            {
                projectile.timeLeft = 0;
                projectile.netUpdate = true;
                SpawnExplosionAndFade(true);
            }
            else { projectile.timeLeft++; }

            // Draw this over the player
            player.heldProj = projectile.whoAmI;

            // Reset damage
            if (projectile.damage > 0) //first time
            {
                // Midly affected by staff and magic byffs
                Damage = (projectile.damage + Items.Weapons.Basic.StaffOfExplosion.baseDamage * 9) / 10;
                projectile.damage = 0;
            }

            // Manage clientside for channelling
            if (Main.myPlayer == projectile.owner)
            {
                if (!player.channel && !Casting)
                {
                    // Set to casting
                    ChangeToCastState();
                }
            }

            if (!Casting)
            {
                // Charging
                if (ChargeLevel < maxCharge)
                {
                    // Charge ticks whilst not "stunned"
                    if (PlayerCanChannel(player))
                    {
                        float rise = 4.5f;
                        ChargeTicks++; // Increase charge, more if close and standing still
                        if (PlayerStandingStillChannel(player))
                        {
                            ChargeTicks += chargeTicksIdle;
                            rise += rise * chargeTicksIdle;
                        }
                        projectile.position.Y -= (float)projectile.height / (chargeTickGameMax * 25f);
                    }

                    // Upgrade charge
                    if (ChargeTicks >= chargeTicksMax)
                    {
                        ChargeTicks = 0; // Reset charge ticks
                        LevelUpCharge(player); // level or go to cast
                    }
                }
                else
                {
                    // Maintenance cost, or go to cast
                    ConsumeMana(player, manaMaintainCost);
                }

                // Update alphas for graphics
                for (int i = 0; i < Math.Min(textureAlphas.Length, ChargeLevel + 1); i++)
                {
                    if (i == ChargeLevel)
                    {
                        if (textureAlphas[i] < 1f) { textureAlphas[i] = ChargeTicks / (chargeTicksMax - 1f); }
                        if (textureAlphas[i] > 1f) { textureAlphas[i] = 1f; }
                    }
                    else
                    {
                        textureAlphas[i] = 1f;
                    }
                }
                PlayerCharging(player);
            }
            else
            {
                // Casting
                if (-ChargeTicks < (2 + ChargeLevel) * castTicksTime)
                {
                    ChargeTicks--;
                    PlayerCasting(player);
                }
                else
                {
                    SpawnExplosionAndFade(false);
                }
            }
        }

        private void SpawnExplosionAndFade(bool weaken)
        {
            // Spawn explosion, fade out and disappear
            if (projectile.alpha == 0)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(projectile.Center, projectile.velocity,
                        mod.ProjectileType<Explooosion>(),
                        CalculateDamage(weaken),
                        projectile.knockBack,
                        projectile.owner,
                        ChargeLevel,
                        projectile.width);
                }
            }
            projectile.alpha += 25;
            projectile.scale *= 0.95f;
            if (projectile.alpha > 255)
            {
                projectile.timeLeft = 0;
                projectile.netUpdate = true;
            }
        }

        private bool PlayerStandingStillChannel(Player player)
        {
            return player.velocity.X == 0 && player.velocity.Y == 0 &&
                              Vector2.Distance(player.Center, projectile.Center) <= farDistance;
        }
        private bool PlayerCanChannel(Player player) { return !player.dead && !player.frozen && !player.stoned && !player.webbed && !player.tongued && !player.silence; }
        private void LevelUpCharge(Player player)
        {
            if (!ConsumeMana(player, CalculateManaCost())) return;

            // Upgrade charge
            ChargeLevel++;

            // Resize projectile and recentre
            Vector2 centre = new Vector2(projectile.Center.X, projectile.Center.Y);
            projectile.Size = new Vector2(explosionSize, explosionSize); // Default width/height
            for (int i = 0; i < ChargeLevel; i++)
            {
                projectile.Size *= explosionScale;
            }
            projectile.Center = centre;

            // Ping noise
            if (Main.myPlayer == projectile.owner)
            {
                Main.PlaySound(25, player.position);
            }

            projectile.netUpdate = true;
        }
        private void ChangeToCastState()
        {
            ChargeTicks = -1;
            projectile.netUpdate = true;
        }

        private int CalculateManaCost()
        {
            return (int)(Main.player[projectile.owner].HeldItem.mana * (1f + (ChargeLevel + 1) / 2f));
        }
        private bool ConsumeMana(Player player, int manaCost)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            // Chug a potion
            if (player.statMana < manaCost && player.manaFlower)
            { player.QuickMana(); }

            // Can't channel? Force cast now them
            if (player.statMana < manaCost)
            { ChangeToCastState(); return false; }

            player.statMana -= manaCost;
            return true;
        }

        private int CalculateDamage(bool weaken)
        {
            int level = ChargeLevel;
            if (weaken) { level /= 3; }
            if (level > 0)
            {
                //goes up to x1000 damage
                Damage = Damage + (int)(Damage * Math.Pow(2, level * 10f / Explosion.maxCharge));
                projectile.knockBack *= 1 + (int)Math.Log10(level * 10);
            }

            //damage is reduced over a period of time due to fire time
            //in this case, since it hits every 10 ticks
            return Damage / (1 + (Explosion.fireTicksTime * (1 + ChargeLevel) / 10));
        }

        private void PlayerCharging(Player player)
        {
            // Grant beetle armour level aggro
            player.aggro += 900;

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

            //play noises
            projectile.frameCounter++;
            if (projectile.frameCounter > (12 + maxCharge - ChargeLevel / 2))
            {
                projectile.frameCounter = 0;
                Main.PlaySound(SoundID.Item34.WithVolume(0.4f + 0.05f * ChargeLevel), player.Center);
            }

            // Draw fancy pantsy staff dusts
            Vector2 staffTip = player.MountedCenter + new Vector2(
                player.inventory[player.selectedItem].width * 1.1f * (float)Math.Cos(player.itemRotation),
                player.inventory[player.selectedItem].width * 1.1f * (float)Math.Sin(player.itemRotation)
            ) * player.direction;
            Dust d = Dust.NewDustDirect(staffTip - new Vector2(3 - player.direction * 2 + ChargeLevel * 0.05f, 3 + ChargeLevel * 0.05f),
                0, 0, 174, 0, 0, 0, Color.White, 0.3f + (0.1f * ChargeLevel));
            d.noGravity = true;
            d.velocity *= 0.3f;
        }
        private bool playerStartCast = false;
        private void PlayerCasting(Player player)
        {
            if(!playerStartCast)
            {
                Main.PlaySound(2, player.position, 72);
                playerStartCast = true;
            }
            if (!PlayerCanChannel(player)) return;

            player.itemRotation = -1.5708f * player.direction;
            player.itemAnimation = player.itemAnimationMax / 2;
            player.itemTime = player.itemAnimationMax / 2;
            player.noKnockback = true;
            player.velocity /= 2f;
        }


        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            drawChargeCircles(spriteBatch);

            drawCastingCircles(spriteBatch);

            Player player = Main.player[projectile.owner];
            if (!Casting)
            {
                if(PlayerStandingStillChannel(player))
                { PlayerFX.drawMagicCast(player, spriteBatch, Color.OrangeRed); }
            }
            else
            {
                if (projectile.alpha == 0)
                { PlayerFX.drawMagicCast(player, spriteBatch, Color.OrangeRed); }
            }
            return false;
        }

        private void drawChargeCircles(SpriteBatch spriteBatch)
        {
            Vector2 circleVector;

            //first circle dust
            for (int i = 0; i < 2; i++)
            {
                float randAng = Main.rand.Next(-31416, 31417) * 0.0001f;
                circleVector = new Vector2(
                    50 * (float)Math.Cos(randAng) - 4,
                    50 * (float)Math.Sin(randAng) - 4
                    );
                //still dust
                int d = Dust.NewDust(projectile.Center + circleVector, 0, 0, 174, 0, 0, 0, Color.White, 0.1f);
                Main.dust[d].fadeIn = 0.4f;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.25f;
                //dust moving out
                d = Dust.NewDust(projectile.Center, 0, 0, 183, circleVector.X / 30, circleVector.Y / 30, 0, Color.White, 0.1f);
                Main.dust[d].fadeIn = 0.8f;
                Main.dust[d].noGravity = true;
            }

            ////-//Main.NewText("draw \n" + i + " with alpha \n" + alpha);
            float size;
            float alpha;
            Vector2 castCentre;
            float sizeCircle;
            float sizeOffset;
            float angle;
            for (int i = 0; i < Math.Min(textureAlphas.Length, ChargeLevel + 1); i++)
            {
                size = (float)(Explosion.explosionSize * Math.Pow(explosionScale, i));
                alpha = textureAlphas[i] * projectile.Opacity;
                switch (i)
                {
                    case 0: ///////////////////////////////////////////////////////////////////// initial circle
                        castCentre = projectile.Center - Main.screenPosition;
                        spriteBatch.Draw(textureTargetM,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.05f),
                            textureTargetM.Bounds.Center.ToVector2(),
                            size * projectile.scale / textureTargetM.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 1: ///////////////////////////////////////////////////////////////////// flat centre
                        castCentre = projectile.Center - Main.screenPosition;

                        spriteBatch.Draw(textureTargetS,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetS.Bounds.Center.ToVector2(),
                            new Vector2(size, size / 3) * projectile.scale / textureTargetS.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 2: ///////////////////////////////////////////////////////////////////// initial circle inner
                        castCentre = projectile.Center - Main.screenPosition;

                        spriteBatch.Draw(textureTargetS,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.05f),
                            textureTargetS.Bounds.Center.ToVector2(),
                            size * projectile.scale / textureTargetS.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 3: ///////////////////////////////////////////////////////////////////// backspin circle
                        castCentre = projectile.Center - Main.screenPosition;

                        spriteBatch.Draw(textureTargetS,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.08f),
                            textureTargetS.Bounds.Center.ToVector2(),
                            new Vector2(size, size / 3) * projectile.scale / textureTargetS.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 4: ///////////////////////////////////////////////////////////////////// orbiter circle 1
                        sizeCircle = size * (2 / 5f);
                        sizeOffset = size * (3 / 5f);
                        angle = (float)(Main.time % 62831) * -0.02f;
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            sizeOffset * 0.5f * (float)Math.Cos(angle),
                            sizeOffset * 0.5f * (float)Math.Sin(angle)
                        );

                        spriteBatch.Draw(textureTargetM,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.03f),
                            textureTargetM.Bounds.Center.ToVector2(),
                            new Vector2(sizeCircle, sizeCircle) * projectile.scale / textureTargetM.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 5: ///////////////////////////////////////////////////////////////////// orbiter circle 2
                        sizeCircle = size * (2 / 5f);
                        sizeOffset = size * (3 / 5f);
                        angle = (float)((Main.time + 20943) % 62831) * -0.02f;
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            sizeOffset * 0.5f * (float)Math.Cos(angle),
                            sizeOffset * 0.5f * (float)Math.Sin(angle)
                        );

                        spriteBatch.Draw(textureTargetM,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.02f),
                            textureTargetM.Bounds.Center.ToVector2(),
                            new Vector2(sizeCircle, sizeCircle) * projectile.scale / textureTargetM.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 6: ///////////////////////////////////////////////////////////////////// orbiter circle 3
                        sizeCircle = size * (2 / 5f);
                        sizeOffset = size * (3 / 5f);
                        angle = (float)((Main.time - 20943) % 62831) * -0.02f;
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            sizeOffset * 0.5f * (float)Math.Cos(angle),
                            sizeOffset * 0.5f * (float)Math.Sin(angle)
                        );

                        spriteBatch.Draw(textureTargetM,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.015f),
                            textureTargetM.Bounds.Center.ToVector2(),
                            new Vector2(sizeCircle, sizeCircle) * projectile.scale / textureTargetM.Width,
                            SpriteEffects.None,
                            0f);
                        break;

                    case 7: ///////////////////////////////////////////////////////////////////// larger circle
                        if (alpha > 0.3f) { alpha = 0.3f; }
                        castCentre = projectile.Center - Main.screenPosition;

                        spriteBatch.Draw(textureTargetL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            (float)(Main.time * 0.01f),
                            textureTargetL.Bounds.Center.ToVector2(),
                            size * projectile.scale / textureTargetL.Width,
                            SpriteEffects.None,
                            0f);

                        //second dust circle
                        for (int j = 0; j < 2; j++)
                        {
                            float randAng = Main.rand.Next(-31416, 31417) * 0.0001f;
                            circleVector = new Vector2(
                                textureTargetL.Width * 0.25f * (float)Math.Cos(randAng) - 20,
                                textureTargetL.Height * 0.25f * (float)Math.Sin(randAng) - 24
                                );
                            //still dust
                            int d = Dust.NewDust(projectile.Center + circleVector,
                                32, 32, 174, 0, 0, 0, Color.White, 0.1f);
                            Main.dust[d].fadeIn = 0.6f;
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity *= 0.25f;
                        }
                        break;
                    case 8: ///////////////////////////////////////////////////////////////////// top circle
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 7f);
                        sizeOffset = size * (6 / 7f);
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            0,
                            -sizeOffset * 0.5f
                        );

                        spriteBatch.Draw(textureTargetXL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetXL.Bounds.Center.ToVector2(),
                            new Vector2(size, sizeCircle) * projectile.scale / textureTargetXL.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 9: ///////////////////////////////////////////////////////////////////// bottom circle
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 6f);
                        sizeOffset = size * (5 / 6f);
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            0,
                            sizeOffset * 0.5f
                        );

                        spriteBatch.Draw(textureTargetXL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetXL.Bounds.Center.ToVector2(),
                            new Vector2(size, sizeCircle) * projectile.scale / textureTargetXL.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 10: ///////////////////////////////////////////////////////////////////// mid circle
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 7f);
                        castCentre = projectile.Center - Main.screenPosition;

                        spriteBatch.Draw(textureTargetL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetL.Bounds.Center.ToVector2(),
                            new Vector2(size, sizeCircle) * projectile.scale / textureTargetL.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 11: ///////////////////////////////////////////////////////////////////// guide topper 1
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 9f);
                        sizeOffset = size * (8 / 9f);
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            0,
                            -sizeOffset * 0.5f
                        );

                        spriteBatch.Draw(textureTargetS,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetS.Bounds.Center.ToVector2(),
                            new Vector2(size * 0.7f, sizeCircle) * projectile.scale / textureTargetS.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 12: ///////////////////////////////////////////////////////////////////// guide topper 2
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 9f);
                        sizeOffset = size * (8 / 9f);
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            0,
                            -sizeOffset * 0.5f
                        );

                        spriteBatch.Draw(textureTargetXL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetXL.Bounds.Center.ToVector2(),
                            new Vector2(size * 0.8f, sizeCircle) * projectile.scale / textureTargetXL.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 13: ///////////////////////////////////////////////////////////////////// guide topper 3
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 9f);
                        sizeOffset = size * (8 / 9f);
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            0,
                            -sizeOffset * 0.5f
                        );

                        spriteBatch.Draw(textureTargetXL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetXL.Bounds.Center.ToVector2(),
                            new Vector2(size * 0.85f, sizeCircle) * projectile.scale / textureTargetXL.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                    case 14: ///////////////////////////////////////////////////////////////////// guide topper 4
                        if (alpha > 0.5f) { alpha = 0.5f; }
                        sizeCircle = size * (1 / 9f);
                        sizeOffset = size * (8 / 9f);
                        castCentre = projectile.Center - Main.screenPosition
                            + new Vector2(
                            0,
                            -sizeOffset * 0.5f
                        );

                        spriteBatch.Draw(textureTargetXL,
                            castCentre,
                            null,
                            new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                            0,
                            textureTargetXL.Bounds.Center.ToVector2(),
                            new Vector2(size * 0.6f, sizeCircle) * projectile.scale / textureTargetXL.Width,
                            SpriteEffects.None,
                            0f);
                        break;
                }
            }
        }
        private void drawCastingCircles(SpriteBatch spriteBatch)
        {
            Player player = Main.player[projectile.owner];
            
            float size;
            float alpha;
            Vector2 castCentre;
            float sizeCircle;
            if (Casting)
            {
                //set up casting from player
                int distance = 120 + ChargeLevel * 3;


                //Casting circle
                alpha = projectile.Opacity;

                size = projectile.width;
                sizeCircle = size * (1 / 9f);
                castCentre = projectile.Center
                    + new Vector2(
                    0,
                    -size * 0.75f
                );

                spriteBatch.Draw(textureTargetL,
                    castCentre - Main.screenPosition,
                    null,
                    new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                    0,
                    textureTargetL.Bounds.Center.ToVector2(),
                    new Vector2(size, sizeCircle) * projectile.scale / textureTargetL.Width,
                    SpriteEffects.None,
                    0f);

                //EXPLOSION LASER
                //burst FX
                int d = Dust.NewDust(castCentre - new Vector2(2 + ChargeLevel * 0.2f, 0), 8, 0, 162,
                    0, 2, 0, Color.White, 1 + ChargeLevel * 0.4f);
                Main.dust[d].noGravity = true; //loses velocity very fast
                Main.dust[d].velocity.X *= ChargeLevel;

                // Only while not exploding
                if (projectile.alpha == 0)
                {
                    //from staff
                    drawLaser(spriteBatch, player.Top + new Vector2(0, -34), player.Top + new Vector2(0, -distance));

                    //to explosion
                    drawLaser(spriteBatch, castCentre, projectile.Center);
                }

                

                //dust fly into top circle
                Vector2 circleVector;
                float randAng = Main.rand.Next(-31416, 31417) * 0.0001f;
                circleVector = new Vector2(
                    58 * (float)Math.Cos(randAng),
                    10 * (float)Math.Sin(randAng)
                    );
                //dust spawn at cricle and move in
                d = Dust.NewDust(player.Top + new Vector2(-4, -distance - 4) + circleVector, 0, 0,
                    170, circleVector.X, circleVector.Y, 0, Color.White, 0.1f);
                Main.dust[d].fadeIn = 1f;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= -0.07f;


                //Player top circles
                spriteBatch.Draw(textureTargetS,
                    player.Top + new Vector2(0, -distance) - Main.screenPosition,
                    null,
                    new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                    0,
                    textureTargetS.Bounds.Center.ToVector2(),
                    new Vector2(player.width * 8, player.width * 1.6f) * projectile.scale / textureTargetS.Width,
                    SpriteEffects.None,
                    0f);
                if (ChargeLevel < 8) return; //STOP if less than 8 charge
                spriteBatch.Draw(textureTargetM,
                    player.Top + new Vector2(0, -distance * 0.6f) - Main.screenPosition,
                    null,
                    new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                    0,
                    textureTargetM.Bounds.Center.ToVector2(),
                    new Vector2(player.width * 5, player.width) * projectile.scale / textureTargetM.Width,
                    SpriteEffects.None,
                    0f);
                if (ChargeLevel < 12) return; //STOP if less than 12 charge
                spriteBatch.Draw(textureTargetL,
                    player.Top + new Vector2(0, -distance * 0.3f) - Main.screenPosition,
                    null,
                    new Color(1f * alpha, 1f * alpha, 1f * alpha, alpha),
                    0,
                    textureTargetL.Bounds.Center.ToVector2(),
                    new Vector2(player.width * 4, player.width * 0.8f) * projectile.scale / textureTargetL.Width,
                    SpriteEffects.None,
                    0f);
            }
        }

        private void drawLaser(SpriteBatch spritebatch, Vector2 start, Vector2 end)
        {
            try
            {
                ////-//Main.NewText("charge: \n" + ChargeTick + " / "  + chargeTime);
                float size = 5f;
                Utils.DrawLaser(
                    spritebatch,
                    textureLaser,
                    start - Main.screenPosition,
                    end - Main.screenPosition,
                    new Vector2(Math.Max(0.1f, size / (4f + Math.Abs( + ChargeTicks)))),
                    new Utils.LaserLineFraming(ExplosionLaser)); //uses delegate (see method below)
            }
            catch { }
        }
        //define which frames are used in each stage (0 = start, 1 = mid, 2 = end
        private void ExplosionLaser(int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame, out float distCovered, out Rectangle frame, out Vector2 origin, out Color color)
        {
            color = Color.White;
            if (stage == 0)
            {
                distCovered = 33f;
                frame = new Rectangle(0, 0, 22, 22);
                origin = frame.Size() / 2f;
                return;
            }
            if (stage == 1)
            {
                frame = new Rectangle(0, 22, 22, 22);
                distCovered = (float)frame.Height;
                origin = new Vector2((float)(frame.Width / 2), 0f);
                return;
            }
            if (stage == 2)
            {
                distCovered = 22f;
                frame = new Rectangle(0, 44, 22, 22);
                origin = new Vector2((float)(frame.Width / 2), 1f);
                return;
            }
            distCovered = 9999f;
            frame = Rectangle.Empty;
            origin = Vector2.Zero;
            color = Color.Transparent;
        }
    }
}