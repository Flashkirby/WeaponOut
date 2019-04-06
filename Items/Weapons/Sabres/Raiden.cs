using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;

namespace WeaponOut.Items.Weapons.Sabres
{
    /// <summary>
    /// Yo it's like, a homing weapon or something.
    /// </summary>
    public class Raiden : ModItem
    {
        public const int focusTime = 60;
        public const int focusRadius = 256;

        public bool slashFlip = false;
        public bool focusSlash = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Raiden");
            DisplayName.AddTranslation(GameCulture.Chinese, "雷电");
            DisplayName.AddTranslation(GameCulture.Russian, "Рейден");

            Tooltip.SetDefault(
                 "Stand still to focus on nearby foes\n" +
                 "'Imbued with ancient arts'");
            Tooltip.AddTranslation(GameCulture.Chinese, "站着不动时会将注意力集中到附近敌人身上\n“注满了古代的武艺”");
            Tooltip.AddTranslation(GameCulture.Russian,
                "Стойте на месте, чтобы сфокусироваться на ближайших врагов\n" +
                "'Зачарован древним мастерством'");
        }
        public override void SetDefaults()
        {
            item.width = 40;
            item.height = 40;

            item.melee = true;
            item.damage = 55;
            item.knockBack = 5;
            item.autoReuse = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.useTime = 0;
            item.useAnimation = 17;

            item.rare = 7;
            item.value = 25000;

            slashFlip = false;
            focusSlash = false;
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<Hayauchi>(), 1);
            recipe.AddIngredient(mod.ItemType<Onsoku>(), 1);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.SetResult(this);
            recipe.AddRecipe();
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
                        d.fadeIn = player.whoAmI + 1;
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
                    mod.ProjectileType<RaidenSlash>(), 
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
                    List<NPC> targets = RaidenSlash.GetTargettableNPCs(player.Center, focusRadius);
                    Vector2 last = player.Center;
                    for (int i = 0; i < targets.Count; i++)
                    {
                        RaidenSlash.DrawDustToBetweenVectors(last, targets[i].Center, 106);
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
            else if (anim > 0.8f)
            {
                player.bodyFrame.Y = swing;
            }
            else if (anim > 0.7f)
            {
                player.bodyFrame.Y = swing2;
            }
            else
            {
                player.bodyFrame.Y = end;
            }

                return true;
        }

        private int minFrames = 6;
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // Set hitboxes accordingly
            if (focusSlash)
            {
                noHitbox = !player.immuneNoBlink;
                if (!noHitbox)
                {
                    player.attackCD = 0;
                }
                hitbox = player.getRect();
            }
            else
            {
                if (player.itemAnimation == player.itemAnimationMax - 1) minFrames = 6;
                NormalHitBox(player, ref hitbox, ref noHitbox, ref minFrames);
            }
        }

        private static void NormalHitBox(Player player, ref Rectangle hitbox, ref bool noHitbox, ref int minFrames)
        {
            float anim = player.itemAnimation / (float)player.itemAnimationMax;
            // hit box only active during these frames
            if ((anim <= 0.9f && anim > 0.7f) || minFrames >= 0)
            {
                int offsetX = 0;
                if (anim > 0.8f) //first half of wing, covers behind
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
                minFrames--;
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

    public class RaidenSlash : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Raiden");
            DisplayName.AddTranslation(GameCulture.Chinese, "雷电");
            Main.projFrames[projectile.type] = 9;
        }
        public override void SetDefaults()
        {
            projectile.width = 104;
            projectile.height = 94;
            projectile.aiStyle = -1;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }

        public bool FocusSlash { get { return projectile.ai[0] == 0; } }
        public float SlashDirection { get { return projectile.ai[0]; } }
        public float FrameCheck
        {
            get { return projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public const int dashTime = 3;

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (FocusSlash)
            {
                FocusAttack(player);
            }

            if (!FocusSlash)
            {
                NormalSlash(player);
            }

            projectile.damage = 0;

            FrameCheck++;
        }

        public List<NPC> allTargets;
        private void FocusAttack(Player player)
        {
            // Centre the projectile on player
            projectile.Center = player.Center;

            if (FrameCheck == 0)
            {
                allTargets = GetTargettableNPCs(player.Center, Raiden.focusRadius);

                if (allTargets.Count <= 0)
                {
                    projectile.ai[0] = 1f;
                    return;
                }

                if (player.whoAmI == Main.myPlayer)
                { Main.SetCameraLerp(0.1f, 10); }
            }
            else
            {
                int currentTarget = (int)((FrameCheck - 1) / dashTime);
                if (currentTarget >= allTargets.Count)
                {
                    projectile.Kill();
                    return;
                }

                player.immuneNoBlink = false;

                bool skip = false;
                // Guarantee hit the target
                if (FrameCheck % dashTime == 0)
                {
                    // Only teleport if can hit
                    NPC npc = allTargets[currentTarget];
                    if (npc.active &&
                        Collision.CanHitLine(player.Center, 1, 1, npc.position, npc.width, npc.height))
                    {
                        player.Bottom = allTargets[currentTarget].Bottom;
                    }
                    else
                    {
                        FrameCheck += dashTime - 1;
                        skip = true;
                    }
                }
                if (currentTarget >= allTargets.Count)
                {
                    projectile.timeLeft = 0;
                    return;
                }
                if (skip)
                { return; }

                //LERP the camera
                if (player.whoAmI == Main.myPlayer)
                { Main.SetCameraLerp(0.1f, 10); }

                // Draw dash dust
                DrawDustToBetweenVectors(player.Center, allTargets[currentTarget].Center, 159,
                10, 1.4f, true);


                Vector2 nextPosition = allTargets[currentTarget].Bottom;
                if (currentTarget * dashTime == FrameCheck - 1)
                {
                    Main.PlaySound(2, player.Center, 71);
                    DrawDustToBetweenVectors(player.Center, allTargets[currentTarget].Center, 106,
                        30, 2f, true);

                    // Set directions
                    projectile.direction = 1;
                    if (nextPosition.X < projectile.position.X) projectile.direction = -1;
                    player.direction = projectile.direction;
                }

                //teleport closer to each NPC
                Vector2 dashCentre = nextPosition - player.Bottom;
                Vector2 dashAmount = dashCentre / Math.Max(1, dashTime - 1);
                player.Bottom += dashAmount;
                player.velocity = new Vector2(player.direction * 5, player.gravDir * -2);

                // freeze in swing
                player.itemAnimation = player.itemAnimationMax - 2;
                player.itemTime = player.itemAnimationMax - 2;

                // Set immunities
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 30); //half second
                player.immuneNoBlink = true;
                player.fallStart = (int)(player.position.Y / 16f);
                player.fallStart2 = player.fallStart;
            }
        }

        private void NormalSlash(Player player)
        {
            // Centre the projectile on player
            projectile.Center = player.Center;

            // move to intended side, then pull back to player width
            projectile.position.X += (projectile.width / 2) - Player.defaultWidth * player.direction;
            projectile.spriteDirection = player.direction;

            if (player.direction < 0) projectile.position.X -= projectile.width;

            projectile.frame = (int)FrameCheck * 2;
            if (projectile.frame > Main.projFrames[projectile.type])
            {
                projectile.timeLeft = 0;
            }
        }

        public override bool PreKill(int timeLeft)
        {
            if (FocusSlash)
            {
                // Reverse direction to avoid recently struck enemy
                Player player = Main.player[projectile.owner];
                player.velocity.X *= -1.5f;
                player.direction *= -1;
            }
            return true;
        }


        public static List<NPC> GetTargettableNPCs(Vector2 center, float radius)
        {
            Dictionary<NPC, float> targets = new Dictionary<NPC, float>();
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() || npc.type == NPCID.TargetDummy)
                {
                    float distance = (center - npc.Center).Length();
                    if (distance <= radius)
                    {
                        if (Collision.CanHitLine(center, 1, 1, npc.position, npc.width, npc.height))
                        {
                            targets.Add(npc, distance);
                        }
                    }
                }
            }

            // Sort the list by distance
            List<NPC> targetList = new List<NPC>(targets.Count);
            var ie = targets.OrderBy(pair => pair.Value).Take(targets.Count);

            foreach (KeyValuePair<NPC, float> kvp in ie)
            {
                targetList.Add(kvp.Key);
            }

            return targetList;
        }

        public static void DrawDustToBetweenVectors(Vector2 vector1, Vector2 vector2, int dust, int amount = 5, float scale = 0.5f, bool noGravity = true)
        {
            Vector2 dustDisplace = new Vector2(-4, -4);
            Vector2 difference = vector2 - vector1;
            for (int i = 0; i < amount; i++)
            {
                Dust d = Main.dust[Dust.NewDust(
                    vector1 + difference * Main.rand.NextFloat() + dustDisplace,
                    0, 0, dust)];
                d.noGravity = noGravity;
                d.velocity = difference * 0.01f;
                d.scale = scale;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Don't draw if false
            if (FocusSlash) return false;

            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameCount = Main.projFrames[projectile.type];

            int frameHeight = texture.Height / frameCount;

            // Flip Horziontally
            SpriteEffects spriteEffect = SpriteEffects.None;
            spriteEffect = SpriteEffects.None;
            if (projectile.spriteDirection < 0)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
            }

            // Flip Vertically
            Player player = Main.player[projectile.owner];
            float gravDir = player.gravDir * SlashDirection;
            if (gravDir <= 0)
            {
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                { spriteEffect = spriteEffect | SpriteEffects.FlipVertically; }
                else
                { spriteEffect = SpriteEffects.FlipVertically; }
            }

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight),
                lightColor,
                player.bodyRotation,
                new Vector2(texture.Width / 2, frameHeight / 2),
                projectile.scale,
                spriteEffect,
                0f);
            return false;
        }
    }
}
