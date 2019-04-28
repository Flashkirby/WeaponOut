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
    public static class RaidenUtils
    {
        public const int focusTime = 60;
        public const int focusTargets = 15;
        private const float focusRadius = 320f;
        private const float focusRadiusReductionSpeedFactor = 15f;

        public static int DustAmount(Player player) { return Main.clientPlayer == player ? 64 : 4; }

        public static float GetFocusRadius(Player player)
        {
            float speedFactor = Math.Max(Math.Abs(player.velocity.X), Math.Abs(player.velocity.Y));
            return Math.Max(64, focusRadius - speedFactor * focusRadiusReductionSpeedFactor);
        }

        public static void DrawDustRadius(Player player, float radius, int amount = 4)
        {
            // Range display dust;
            for (int i = 0; i < amount; i++)
            {
                Vector2 offset = new Vector2();
                double angle = Main.rand.NextDouble() * 2d * Math.PI;
                offset.X += (float)(Math.Sin(angle) * radius);
                offset.Y += (float)(Math.Cos(angle) * radius);

                Dust d = Dust.NewDustPerfect(player.Center + offset, 106, player.velocity, 200, default(Color), 0.3f);
                d.fadeIn = 0.5f;
                d.noGravity = true;
            }
        }

        public static void DrawOrderedTargets(Player player, List<NPC> targets)
        {
            // Display targets for client
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 last = player.Center;
                for (int i = 0; i < targets.Count; i++)
                {
                    DrawDustToBetweenVectors(last, targets[i].Center, 106, 5, 0.35f);
                    last = targets[i].Center;
                }

                // Crosshair
                if (targets.Count > 0)
                {
                    int velo = (int)(Main.rand.NextFloatDirection() * 16f);

                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            Dust d = Dust.NewDustPerfect(targets.Last().Center + new Vector2(velo * 4, y),
                                182, new Vector2(-velo, 0));
                            d.scale = 0.5f;
                            d.noGravity = true;

                            d = Dust.NewDustPerfect(targets.Last().Center + new Vector2(x, velo * 4),
                                182, new Vector2(0, -velo));
                            d.scale = 0.5f;
                            d.noGravity = true;
                        }
                    }
                }
            }
        }

        public static List<NPC> GetTargettableNPCs(Vector2 center, float radius, int limit = 15)
        {
            Dictionary<NPC, float> targets = new Dictionary<NPC, float>();
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.life == 0) continue;
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
                if (limit > 0) { targetList.Add(kvp.Key); }
                limit--;
            }

            return targetList;
        }
        public static void DrawDustToBetweenVectors(Vector2 start, Vector2 end, int dust, int amount = 5, float scale = 0.5f, bool noGravity = true)
        {
            Vector2 dustDisplace = new Vector2(-4, -4);
            Vector2 difference = end - start;
            for (int i = 0; i < amount; i++)
            {
                Dust d = Dust.NewDustPerfect(start + difference * Main.rand.NextFloat() + dustDisplace, dust, difference * 0.01f, 0, default(Color), scale);
                d.noGravity = noGravity;
            }
        }
    }

    /// <summary>
    /// Yo it's like, a homing weapon or something.
    /// </summary>
    public class Raiden : ModItem
    {

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
            item.useTime = 60 / 4;
            item.useAnimation = 17;

            item.rare = 7;
            item.value = 25000;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<Hayauchi>(), 1);
            recipe.AddIngredient(mod.ItemType<Onsoku>(), 1);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
            recipe.AddTile(TileID.AdamantiteForge);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            ModSabres.HoldItemManager(player, item, mod.ProjectileType<RaidenSlash>(),
                default(Color), 1f, player.itemTime == 0 ? 0f : 1f, customCharge, 8);

            if (player.itemTime == 0)
            {
                float radius = RaidenUtils.GetFocusRadius(player);
                List<NPC> targets = RaidenUtils.GetTargettableNPCs(player.Center, radius);

                RaidenUtils.DrawDustRadius(player, radius, RaidenUtils.DustAmount(player));
                RaidenUtils.DrawOrderedTargets(player, targets);
            }
        }

        public Action<Player, bool> customCharge = CustomCharge;
        public static void CustomCharge(Player player, bool flashFrame)
        {
            float chargeSize = player.itemTime * 4f; //roughly 15 * 4
            Vector2 dustPosition = player.Center + new Vector2(
                Main.rand.NextFloatDirection() * chargeSize,
                Main.rand.NextFloatDirection() * chargeSize
                );

            // Charging dust
            Dust d = Dust.NewDustPerfect(dustPosition, 235);
            d.scale = Main.rand.Next(70, 85) * 0.01f;
            d.fadeIn = player.whoAmI + 1;
        }

        public override bool HoldItemFrame(Player player) //called on player holding but not swinging
        {
            if (player.itemTime == 0 && false) //ready to slash targets
            {
                player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                return true;
            }
            return false;
        }

        public override bool UseItemFrame(Player player)
        {
            ModSabres.UseItemFrame(player, 0.9f, item.isBeingGrabbed);
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int height = 94;
            int length = 104;
            if (ModSabres.SabreIsChargedStriking(player, item))
            {
                hitbox = player.Hitbox;
                player.meleeDamage += 2f;
                noHitbox = player.itemAnimation < player.itemAnimationMax - 10;
            } 
            else
            {
                ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Color colour = new Color(0.4f, 0.8f, 0.1f);
            ModSabres.OnHitFX(player, target, crit, colour, true);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            float rotation = player.itemRotation + MathHelper.PiOver4;
            if (item.isBeingGrabbed) rotation -= MathHelper.PiOver2; // Reverse slash (upward) flip
            if (player.direction < 0) { rotation *= -1f; rotation += MathHelper.Pi; }
                Vector2 direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));

            Dust d = Dust.NewDustDirect(hitbox.Location.ToVector2() - new Vector2(4, 4), hitbox.Width, hitbox.Height, 159);
            d.color = Color.Green;
            d.scale = 0.5f;
            d.fadeIn = 0.8f;
            d.position -= direction * 25f;
            d.velocity = direction * 5f;
        }
        

        //public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        //{
        //    return false;
        //}

        //public override void HoldItem(Player player)
        //{
        //    int focus = getFocus(player);

        //    // Reset style to swing when neutral
        //    if (player.itemAnimation == 0)
        //    {
        //        item.useStyle = 1;

        //        if ((Math.Abs(player.velocity.X) < 1.5f && player.velocity.Y == 0f
        //        && player.grapCount == 0
        //        && !player.pulley
        //        && !(player.frozen || player.webbed || player.stoned || player.noItems)
        //        )
        //        || getFocus(player) >= focusTime)
        //        {
        //            if (getFocus(player) < focusTime + 1)
        //            {
        //                focus++;
        //                updateFocus(player, focus);

        //                Vector2 vector = player.Center;
        //                vector.X += (float)Main.rand.Next(-2048, 2048) * 0.02f;
        //                vector.Y += (float)Main.rand.Next(-2048, 2048) * 0.02f;


        //                // Charging dust
        //                Dust d = Main.dust[Dust.NewDust(
        //                    vector, 1, 1,
        //                    235, 0f, 0f, 0,
        //                    Color.White, 1f)];

        //                d.velocity *= 0f;
        //                d.scale = Main.rand.Next(70, 85) * 0.01f;
        //                // This dust uses fadeIn for homing into players
        //                d.fadeIn = player.whoAmI + 1;
        //            }
        //        }
        //        else
        //        {
        //            focus = 0;
        //            updateFocus(player, focus);
        //        }
        //    }
        //    else
        //    {
        //        if (player.itemAnimation == player.itemAnimationMax - 1)
        //        {
        //            int focusType = 0; // focus attack
        //            if (!focusSlash)
        //            {
        //                if (slashFlip)
        //                { focusType = -1; } //normal slash flipped
        //                else
        //                { focusType = 1; } //normal slash 
        //            }
        //            Projectile.NewProjectile(
        //            player.position.X,
        //            player.position.Y,
        //            0, 0,
        //            mod.ProjectileType<RaidenSlash>(), 
        //            0, 0f,
        //            player.whoAmI,
        //            focusType);
        //        }

        //        item.useStyle = 0;
        //        focus = 0;
        //        updateFocus(player, focus);
        //    }


        //    if (focus >= focusTime)
        //    {
        //        if (focus == focusTime)
        //        {
        //            Main.PlaySound(2, player.position, 24);
        //        }

        //        int amount = 4;
        //        int alpha = 200;
        //        if (player.whoAmI == Main.myPlayer)
        //        {
        //            amount = 16;
        //            alpha = 100;
        //        }
        //        // Range display dust;
        //        for (int i = 0; i < amount; i++)
        //        {
        //            Vector2 offset = new Vector2();
        //            double angle = Main.rand.NextDouble() * 2d * Math.PI;
        //            offset.X += (float)(Math.Sin(angle) * focusRadius);
        //            offset.Y += (float)(Math.Cos(angle) * focusRadius);
        //            Dust dust = Main.dust[Dust.NewDust(
        //                player.Center + offset - new Vector2(4, 4), 0, 0,
        //                106, 0, 0, alpha, Color.White, 0.3f
        //                )];
        //            dust.velocity = player.velocity;
        //            dust.fadeIn = 0.5f;
        //            dust.noGravity = true;
        //        }

        //        // Display targets for client
        //        if (player.whoAmI == Main.myPlayer)
        //        {
        //            List<NPC> targets = RaidenSlash.GetTargettableNPCs(player.Center, focusRadius);
        //            Vector2 last = player.Center;
        //            for (int i = 0; i < targets.Count; i++)
        //            {
        //                RaidenSlash.DrawDustToBetweenVectors(last, targets[i].Center, 106);
        //                last = targets[i].Center;
        //            }
        //        }
        //    }

        //    if(player.itemAnimation == 1) // when almost done, swap around
        //    { slashFlip = !slashFlip; }
        //}

        //public override bool HoldItemFrame(Player player) //called on player holding but not swinging
        //{
        //    if (getFocus(player) >= focusTime) //ready to slash
        //    {
        //        player.bodyFrame.Y = 4 * player.bodyFrame.Height;
        //        return true;
        //    }
        //    return false;
        //}

        //public override void UseStyle(Player player)
        //{
        //    if(getFocus(player) >= focusTime)
        //    {
        //        focusSlash = true;
        //        slashFlip = false;
        //    }
        //}

        //public override bool UseItemFrame(Player player)
        //{
        //    //counts down from 1 to 0
        //    float anim = player.itemAnimation / (float)(player.itemAnimationMax);
        //    int frames = player.itemAnimationMax - player.itemAnimation;

        //    // animation frames;
        //    int start, swing, swing2, end;

        //    if (slashFlip)
        //    {
        //        start = 4 * player.bodyFrame.Height;
        //        swing = 3 * player.bodyFrame.Height;
        //        swing2 = 2 * player.bodyFrame.Height;
        //        end = 1 * player.bodyFrame.Height;
        //    }
        //    else
        //    {
        //        start = 1 * player.bodyFrame.Height;
        //        swing = 2 * player.bodyFrame.Height;
        //        swing2 = 3 * player.bodyFrame.Height;
        //        end = 4 * player.bodyFrame.Height;
        //    }

        //    // Actual animation
        //    if (anim > 0.9)
        //    {
        //        player.bodyFrame.Y = start;
        //    }
        //    else if (anim > 0.8f)
        //    {
        //        player.bodyFrame.Y = swing;
        //    }
        //    else if (anim > 0.7f)
        //    {
        //        player.bodyFrame.Y = swing2;
        //    }
        //    else
        //    {
        //        player.bodyFrame.Y = end;
        //    }

        //        return true;
        //}

        //private int minFrames = 6;
        //public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        //{
        //    // Set hitboxes accordingly
        //    if (focusSlash)
        //    {
        //        noHitbox = !player.immuneNoBlink;
        //        if (!noHitbox)
        //        {
        //            player.attackCD = 0;
        //        }
        //        hitbox = player.getRect();
        //    }
        //    else
        //    {
        //        if (player.itemAnimation == player.itemAnimationMax - 1) minFrames = 6;
        //        NormalHitBox(player, ref hitbox, ref noHitbox, ref minFrames);
        //    }
        //}

        //private static void NormalHitBox(Player player, ref Rectangle hitbox, ref bool noHitbox, ref int minFrames)
        //{
        //    float anim = player.itemAnimation / (float)player.itemAnimationMax;
        //    // hit box only active during these frames
        //    if ((anim <= 0.9f && anim > 0.7f) || minFrames >= 0)
        //    {
        //        int offsetX = 0;
        //        if (anim > 0.8f) //first half of wing, covers behind
        //        {
        //            hitbox.Width = 64;
        //            hitbox.Height = 92;
        //            offsetX = 14;
        //        }
        //        else
        //        {
        //            hitbox.Width = 96;
        //            hitbox.Height = 112;
        //            offsetX = 40;
        //        }
        //        // Center hitbox and offset accordingly
        //        hitbox.X = (int)player.Center.X - hitbox.Width / 2
        //            + offsetX * player.direction;
        //        hitbox.Y = (int)player.Center.Y - hitbox.Height / 2;

        //        if (player.attackCD > 1) player.attackCD = 1;
        //        minFrames--;
        //    }
        //    else
        //    {
        //        noHitbox = true;
        //    }
        //}

        //public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        //{
        //    if(focusSlash)
        //    {
        //        damage *= 3;
        //        crit = true;
        //    }
        //}
    }

    public class RaidenSlash : ModProjectile
    {
        public const int specialProjFrames = 9;
        bool sndOnce = true;
        int chargeSlashDirection = 1;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Raiden");
            DisplayName.AddTranslation(GameCulture.Chinese, "雷电");
            Main.projFrames[projectile.type] = specialProjFrames;
        }
        public override void SetDefaults()
        {
            projectile.width = 104;
            projectile.height = 94;
            projectile.aiStyle = -1;
            projectile.timeLeft = 60;

            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.penetrate = -1;
        }
   
        public override bool? CanCutTiles() { return false; }
        public float FrameCheck
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }
        public float SlashLogic
        {
            get { return (int)projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (ModSabres.AINormalSlash(projectile, SlashLogic))
            {
                FrameCheck += 1f;
                targets = null;
            }
            else
            {
                // Charged attack
                ModSabres.AISetChargeSlashVariables(player, chargeSlashDirection);
                ModSabres.NormalSlash(projectile, player);

                // Play charged sound & set targets
                if (sndOnce)
                {
                    targets = RaidenUtils.GetTargettableNPCs(player.Center, RaidenUtils.GetFocusRadius(player), RaidenUtils.focusTargets);
                    if(targets.Count > 0)
                    {
                        Main.PlaySound(SoundID.Item71, projectile.Center); sndOnce = false;

                        //unstickCenter = player.Center;
                        if (targets.Last().Center.X > player.Center.X)
                        { player.direction = 1; }
                        else;
                        { player.direction = -1; }
                    }
                }

                if (targets != null && targets.Count > 0)
                {
                    ChargeSlashAI(player);
                }
                else
                {
                    SlashLogic = 1;
                    ModSabres.AINormalSlash(projectile, SlashLogic);
                    FrameCheck += 1f;
                    targets = null;
                }
            }
            projectile.damage = 0;
        }

        /// <summary> As there are 9 frames, gotta fit the number of targets within this window (roughly)</summary>
        public int totalTargetTime = 30;
        public List<NPC> targets = null;
        //private Vector2? unstickCenter = null;
        public void ChargeSlashAI(Player player)
        {
            setAnimationAndImmunities(player);

            float countf = targets.Count;
            int framesPerTarget = (int)Math.Max(1, (float)totalTargetTime / countf);
            float oneFrame = specialProjFrames / (framesPerTarget * countf); // calculates ro roughly 9 / 30 = 0.3

            // Get current frame, and current target in attack
            int i = (int)(FrameCheck / oneFrame);

            if (i >= framesPerTarget * countf)
            {
                // End frame
                player.velocity = new Vector2(projectile.direction * -7.5f, player.gravDir * -2);
                projectile.timeLeft = 0;

                //if(unstickCenter != null && Main.clientPlayer == player)
                //{
                //    player.Center = (Vector2)unstickCenter;
                //}

                return;
            }
            else
            {
                // Set camera lerp
                if (player.whoAmI == Main.myPlayer)
                { Main.SetCameraLerp(0.1f, 10); }

                // Get the target position, or wait if the target is invalid
                int iTarget = (int)MathHelper.Clamp((float)i / framesPerTarget, 0, countf - 1);
                NPC target = targets[iTarget];
                Vector2 targetBottom;
                if (target == null || !target.active || target.dontTakeDamage)
                { targetBottom = player.Bottom; target = null; }
                else
                { targetBottom = target.Bottom; }


                //if ( Main.clientPlayer == player)
                //{
                //    // If can't reach this point, set an emergency "unstick" position
                //    if (!Collision.CanHit(player.position, player.width, player.height, targetBottom, player.width, player.height))
                //    { unstickCenter = player.Center; }
                //    else
                //    { unstickCenter = null; }

                //    if(unstickCenter != null)
                //    {
                //        Main.NewText("can get stuck!");
                //    }
                //}

                Vector2 oldBottom = new Vector2(player.Bottom.X, player.Bottom.Y);
                Vector2 vecHeight = new Vector2(0, Player.defaultHeight / -2);

                // Tweening if there is time
                if (framesPerTarget > 1)
                {
                    Vector2 dist = (targetBottom - player.Bottom) * (1f / Math.Max(1, framesPerTarget / 2f));
                    player.Bottom = player.Bottom + dist;
                    player.velocity.Y = -player.gravDir * 1.5f;

                    int distFactor = (int)(dist.Length() / 4f);
                    RaidenUtils.DrawDustToBetweenVectors(oldBottom + vecHeight, player.Bottom + vecHeight, 106, distFactor, 2f);
                }

                int framesToNextKeyframe = Math.Max(0, ((iTarget + 1) * framesPerTarget - 1) - i);

                //// Snap to target on key frames, assuming they can be reached
                if (framesToNextKeyframe == 0)
                {
                    player.Bottom = targetBottom;

                    RaidenUtils.DrawDustToBetweenVectors(oldBottom + vecHeight, player.Bottom + vecHeight, 106, 2, 2f);
                }

                // Set slash
                Vector2 toTarget = targetBottom - player.Bottom;
                projectile.rotation = (float)Math.Atan2(toTarget.Y, toTarget.X);
                ModSabres.RecentreSlash(projectile, player);


                FrameCheck += oneFrame;
            }
        }

        private void setAnimationAndImmunities(Player player)
        {
            // freeze in swing
            player.itemAnimation = player.itemAnimationMax - 2;
            player.itemTime = player.itemAnimation;
            player.attackCD = 0;

            // Set immunities
            player.immune = true;
            player.immuneTime = Math.Max(player.immuneTime, 30); //half second
            player.immuneNoBlink = true;
            player.fallStart = (int)(player.position.Y / 16f);
            player.fallStart2 = player.fallStart;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            int weaponItemID = mod.ItemType<Raiden>();
            Color lighting = Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            return ModSabres.PreDrawSlashAndWeapon(spriteBatch, projectile, weaponItemID, lighting,
                null,//SlashLogic == 0f ? specialSlash : null,
                lighting,
                specialProjFrames,
                SlashLogic == 0f ? chargeSlashDirection : SlashLogic,
                SlashLogic == 0f);
        }
    }
}
