using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System;

namespace WeaponOut.Projectiles
{
    public class SpiritGuardian : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starlight Guardian");
            Main.projFrames[projectile.type] = 24;
        }

        public override void SetDefaults()
        {

            projectile.width = 176;
            projectile.height = 96;

            projectile.penetrate = -1;
            projectile.timeLeft = projectile.timeLeft * 5;

            projectile.netImportant = true;
            projectile.friendly = false;
            projectile.minion = true;
            projectile.minionSlots = 0.0f;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            // So it doesn't conflict with fist NPC cooldown
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 6;
        }
        public override bool? CanCutTiles()
        { return false; }


        public enum Animation { Idle, Alert, Attacking};
        public Animation spriteAnimation;
        public const float aggroDist = 500;
        public const float chaseDist = 1000;
        public const int attackRange = 50;

        public int AILogicMode { get { return (int)projectile.ai[0]; } set { projectile.ai[0] = value; } }
        public int AIAttackingState { get { return (int)projectile.ai[1]; } set { projectile.ai[1] = value; } }

        public override void AI()
        {
            // Get active player, or disappear
            Player player = Main.player[projectile.owner];
            var pfx = player.GetModPlayer<PlayerFX>();
            if (!player.active)
            { projectile.active = false; return; }
            if (player.dead)
            { pfx.starlightGuardian = false; }

            // Stay around
            if (pfx.starlightGuardian)
            { projectile.timeLeft = 2; }

            // Default resting point at player
            float clampSpeed = -1f;
            float lerpSpeed = 0.2f;
            spriteAnimation = Animation.Idle;
            Vector2 targetCentre = player.Center;
            targetCentre = player.Center + new Vector2(-16f * player.direction, -24 * player.gravDir);
            projectile.direction = projectile.spriteDirection = player.direction;
            projectile.friendly = false;
            projectile.damage = 0;
            projectile.knockBack = 0f;


            if (pfx.starlightGuardianStanceChangeInput)
            {
                pfx.starlightGuardianStanceChangeInput = false;
                if (AILogicMode == 1)
                { AILogicMode = 0; }
                else if (AILogicMode == 0)
                { AILogicMode = 1; }
                projectile.netUpdate2 = true;
            }

            NPC target = null;

            switch (AILogicMode)
            {
                case 0:
                    // Mode 1: Attack player's last target
                    // Only attack what the player last hit, and follow it up to 250 away

                    if (pfx.lastNPCHitStarlightGuardian > -1)
                    {
                        target = Main.npc[pfx.lastNPCHitStarlightGuardian];
                        if (target.CanBeChasedBy(this, false) &&
                            Vector2.Distance(player.Center, target.Center) <= chaseDist)
                        {
                            targetCentre = MoveToAttack(player, target);

                            projectile.friendly = true;
                            projectile.damage = 22 + 76;
                            projectile.knockBack = 5f;
                        }
                        else
                        { target = null; pfx.lastNPCHitStarlightGuardian = -1; }
                    }

                    // Otherwise attack if the player is
                    if (target == null)
                    {
                        if (player.itemAnimation > 0)
                        {
                            targetCentre += new Vector2(player.direction * (attackRange), 32 * player.gravDir);
                            projectile.friendly = true;
                            projectile.damage = 22 + 76;
                            projectile.knockBack = 3f;
                        }
                    }

                    break;
                case 1:
                    // Mode 2: Watch player's back
                    // Only attacking targets closer than 250 that aren't what the player is hitting

                    // Resting position
                    spriteAnimation = Animation.Alert;
                    targetCentre = player.Center + new Vector2(-32f * player.direction, -16 * player.gravDir);
                    projectile.direction = projectile.spriteDirection = -player.direction;

                    //Get closest that player isn't hitting
                    float closestDist = aggroDist;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.whoAmI == pfx.lastNPCHitStarlightGuardian) continue;

                        Vector2 diff = npc.Center - (player.Center - new Vector2(player.direction * 64, 0));
                        float dist = Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
                        if (dist <= closestDist && npc.CanBeChasedBy(this, false))
                        {
                            target = npc;
                            closestDist = dist;
                        }
                    }

                    // No target, but player has hit something
                    if(target == null && pfx.lastNPCHitStarlightGuardian >= 0)
                    {
                        NPC npc = Main.npc[pfx.lastNPCHitStarlightGuardian];
                        Vector2 diff = npc.Center - (player.Center - new Vector2(player.direction * 64, 0));
                        float dist = Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
                        if (dist <= closestDist && npc.CanBeChasedBy(this, false))
                        {
                            target = npc;
                            closestDist = dist;
                            pfx.lastNPCHitStarlightGuardian = -1;
                        }
                    }

                    // Attack it

                    if (target != null)
                    {
                        targetCentre = MoveToAttack(player, target);
                        
                        projectile.friendly = true;
                        projectile.damage = 22 + 76;
                        projectile.knockBack = 5f;

                        // Only go so fast, or speed up to catch up with it
                        clampSpeed = Math.Max(24f, (target.oldPosition - target.position).Length() * 2);
                    }
                    else
                    { lerpSpeed /= 4; }

                    break;
            }

            // Move to position
            projectile.velocity = Vector2.Lerp(projectile.Center, targetCentre, lerpSpeed) - projectile.Center;
            if(clampSpeed > 0f)
            {
                if(projectile.velocity.Length() > clampSpeed)
                {
                    projectile.velocity.Normalize();
                    projectile.velocity *= clampSpeed;
                }
            }

            Lighting.AddLight(projectile.Center, 0.9f, 0.9f, 0.7f);

            if (projectile.damage > 0)
            { spriteAnimation = Animation.Attacking; }
            FindFrame();
        }

        private Vector2 MoveToAttack(Player player, NPC target)
        {
            if (target.Center.X >= player.Center.X)
            { projectile.direction = projectile.spriteDirection = 1; }
            else
            { projectile.direction = projectile.spriteDirection = -1; }

            return target.Center - new Vector2((attackRange + target.width / 2) * projectile.direction, 0);
        }

        public void FindFrame()
        {
            projectile.frameCounter++;

            int frameTime = 10;
            int frameMin = 0;
            int frameMax = 7;

            switch (spriteAnimation)
            {
                case Animation.Alert:
                    frameTime = 5;
                    frameMin = 8;
                    frameMax = 15;
                    break;
                case Animation.Attacking:
                    frameTime = 2;
                    frameMin = 16;
                    frameMax = 23;
                    break;
            }

            if (projectile.frameCounter >= frameTime)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
                if (projectile.frame < frameMin) projectile.frame = frameMax;
                else if (projectile.frame > frameMax) projectile.frame = frameMin;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D t = Main.projectileTexture[projectile.type];
            int frameHeight = t.Height / Main.projFrames[projectile.type];
            SpriteEffects effects = SpriteEffects.None;
            if (projectile.spriteDirection < 0) effects = SpriteEffects.FlipHorizontally;
            if (projectile.localAI[0] < 0) effects = effects | SpriteEffects.FlipVertically;
            Vector2 origin = new Vector2(t.Width / 2, frameHeight / 2);

            int length = Math.Min(10, 2 + (int)projectile.oldVelocity.Length());

            for (int i = length; i >= 0; i--)
            {
                Vector2 drawPos = projectile.Center - Main.screenPosition - projectile.oldVelocity * i * 0.5f;
                float trailOpacity = projectile.Opacity - 0.05f - (0.95f / length) * i;
                if (i != 0) trailOpacity /= 2f;
                if (trailOpacity > 0f)
                {
                    float colMod = 0.4f + 0.6f * trailOpacity;
                    spriteBatch.Draw(t,
                        drawPos.ToPoint().ToVector2(),
                        new Rectangle(0, frameHeight * projectile.frame, t.Width, frameHeight),
                        new Color(1f * colMod, 1f * colMod, 1f, 0.5f) * trailOpacity,
                        projectile.rotation,
                        origin,
                        projectile.scale * (1f + 0.02f * i),
                        effects,
                        0);
                }
            }
            return false;
        }
    }
}
