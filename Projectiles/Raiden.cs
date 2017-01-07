using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class Raiden : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.name = "Raiden";
            projectile.width = 104;
            projectile.height = 94;
            projectile.aiStyle = -1;

            Main.projFrames[projectile.type] = 1;

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

            if(FrameCheck == 0)
            {
                allTargets = GetTargettableNPCs(projectile.Center, Items.Weapons.Raiden.focusRadius);

                if(allTargets.Count <= 0)
                {
                    projectile.ai[0] = 1f;
                    return;
                }
            }
            else
            {
                int currentTarget = (int)((FrameCheck - 1) / dashTime);
                player.immuneNoBlink = false;
                if (currentTarget > allTargets.Count)
                {
                    projectile.Kill();
                    return;
                }


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
                projectile.Kill();
            }
        }

        public static List<NPC> GetTargettableNPCs(Vector2 center, float radius)
        {
            Dictionary<NPC, float> targets = new Dictionary<NPC, float>();
            foreach(NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float distance = (center - npc.Center).Length();
                    if (distance <= radius)
                    {
                        targets.Add(npc, distance);
                    }
                }
            }

            // Sort the list by distance
            List<NPC> targetList = new List<NPC>(targets.Count);
            var ie = targets.OrderBy(pair => pair.Value).Take(targets.Count);
            
            foreach(KeyValuePair<NPC, float> kvp in ie)
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
                    vector1 + difference * Main.rand.NextFloat() - dustDisplace,
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
                if(spriteEffect == SpriteEffects.FlipHorizontally)
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
