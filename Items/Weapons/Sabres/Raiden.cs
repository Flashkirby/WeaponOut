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

        public static int DustAmount(Player player) { return player.whoAmI == Main.myPlayer ? 32 : 2; }

        public static float GetFocusRadius(Player player)
        {
            float speedFactor = Math.Max(Math.Abs(player.velocity.X), Math.Abs(player.velocity.Y / 2));
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
                            Dust d = Dust.NewDustPerfect(last + new Vector2(velo * 4, y),
                                182, new Vector2(-velo, 0));
                            d.scale = 0.5f;
                            d.noGravity = true;

                            d = Dust.NewDustPerfect(last + new Vector2(x, velo * 4),
                                182, new Vector2(0, -velo));
                            d.scale = 0.5f;
                            d.noGravity = true;
                        }
                    }
                }
            }
        }

        public static List<NPC> GetTargettableNPCs(Vector2 center, Vector2 targetCentre, float radius, int limit = 15)
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
                        float distanceToFocus = (targetCentre - npc.Center).Length();
                        if (Collision.CanHit(center - new Vector2(12, 12), 24, 24, npc.position, npc.width, npc.height))
                        {
                            targets.Add(npc, distanceToFocus);
                        }
                    }
                }
            }

            // Sort the list by distance (inner to outer)
            List<NPC> targetList = new List<NPC>(targets.Count);
            var ie = targets.OrderBy(pair => pair.Value).Take(targets.Count);

            foreach (KeyValuePair<NPC, float> kvp in ie)
            {
                if (limit > 0) { targetList.Add(kvp.Key); }
                limit--;
            }

            // Invert (outer to inner)
            targetList.Reverse();

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
            item.value = Item.sellPrice(0, 3, 0, 0);
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

                RaidenUtils.DrawDustRadius(player, radius, RaidenUtils.DustAmount(player));

                if(Main.myPlayer == player.whoAmI)
                {
                    Vector2 mouse = new Vector2(Main.screenPosition.X + Main.mouseX, Main.screenPosition.Y + Main.mouseY);
                    List<NPC> targets = RaidenUtils.GetTargettableNPCs(player.Center, mouse, radius, RaidenUtils.focusTargets);
                    RaidenUtils.DrawOrderedTargets(player, targets);
                }
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
            ModSabres.UseItemHitboxCalculate(player, item, ref hitbox, ref noHitbox, 0.9f, height, length);
            if (ModSabres.SabreIsChargedStriking(player, item))
            {
                player.meleeDamage += 2f;
                noHitbox = player.itemAnimation < player.itemAnimationMax - 10;
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
                    Vector2 mouse;
                    if (Main.myPlayer == player.whoAmI)
                    { mouse = new Vector2(Main.screenPosition.X + Main.mouseX, Main.screenPosition.Y + Main.mouseY); }
                    else
                    { mouse = player.Center + new Vector2(player.direction * 256); } // an estimation
                    targets = RaidenUtils.GetTargettableNPCs(player.Center, mouse, RaidenUtils.GetFocusRadius(player), RaidenUtils.focusTargets);
                    if(targets.Count > 0)
                    {
                        Main.PlaySound(SoundID.Item71, projectile.Center); sndOnce = false;


                        // Set up initial ending position as where we started
                        if (player.whoAmI == Main.myPlayer)
                        { endingPositionCenter = player.Center; }

                        // Set ending slash direction
                        if (targets.Last().Center.X > player.Center.X)
                        { player.direction = 1; }
                        else
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
        private Vector2? endingPositionCenter = null;
        public void ChargeSlashAI(Player player)
        {
            if (targets.Count == 1) totalTargetTime = 15;

            setAnimationAndImmunities(player);

            float countf = targets.Count;
            int framesPerTarget = (int)Math.Max(1, (float)totalTargetTime / countf);
            float oneFrame = specialProjFrames / (framesPerTarget * countf); // calculates ro roughly 9 / 30 = 0.3

            // Get current frame, and current target in attack
            int i = (int)(FrameCheck / oneFrame);

            if (i >= framesPerTarget * countf)
            {
                // End frame
                player.Center = (Vector2)endingPositionCenter;
                player.velocity = new Vector2(projectile.direction * -7.5f, player.gravDir * -2);
                projectile.timeLeft = 0;

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
                {
                    // Set target
                    targetBottom = target.Bottom;
                    // and rotate slash
                    Vector2 toTarget = targetBottom - player.Bottom;
                    projectile.rotation = (float)Math.Atan2(toTarget.Y, toTarget.X);
                }

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
                ModSabres.RecentreSlash(projectile, player);


                // Clientside unstick code, don't bother for others in MP
                if (player.whoAmI == Main.myPlayer)
                { UpdateValidEndingPosition(player); }
                else { endingPositionCenter = player.Center; }

                FrameCheck += oneFrame;
            }
        }

        private void UpdateValidEndingPosition(Player player)
        {
            // Check if the player is inside tiles
            bool stuck = false;
            try
            {
                Point targetTileBottom = player.Bottom.ToTileCoordinates();
                for (int y = targetTileBottom.Y - 1; y >= targetTileBottom.Y - 3; y--)
                {
                    int x = targetTileBottom.X;

                    //Main.NewText("check X " + x + " ? pX" + player.position.ToTileCoordinates().X);//DEBUG
                    Dust.NewDustPerfect(new Vector2(x * 16 + 8, y * 16 + 8), 127);//DEBUG

                    if (Main.tile[x, y] != null && Main.tile[x, y].active() && Main.tileSolid[Main.tile[x, y].type])
                    {
                        stuck = true; break;
                    }
                    if (stuck) break;
                }
            }
            catch { stuck = true; }

            // If we're fine, we can update the position
            if (!stuck)
            { endingPositionCenter = player.Center; }
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
