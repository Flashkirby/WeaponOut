using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    /// <summary>
    /// Spins around a player, re-orients when player is attacking
    /// </summary>
    public class Skelebro : ModProjectile
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Backup Skeleton");
            Main.projFrames[projectile.type] = 20;
        }
        public override void SetDefaults()
        {
            projectile.width = 40;
            projectile.height = 62;
            projectile.alpha = 255; // Start invisible

            projectile.penetrate = -1;
            projectile.timeLeft = 60;

            projectile.friendly = false;
            projectile.melee = true;
            projectile.tileCollide = false;

            // So it doesn't conflict with fist NPC cooldown
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 12;
        }
        public override bool? CanCutTiles() { return false; }

        private Queue<float> followX;
        private Queue<float> followY;
        private Queue<float> followXVel;
        private Queue<float> followYVel;
        private Queue<int> playerFrame;
        private Queue<byte> playerAttack;
        private Queue<bool> playerDirection;

        public override void AI()
        {
            if (projectile.ai[0] == 0f)
            {
                projectile.ai[0]++;
                followX = new Queue<float>();
                followY = new Queue<float>();
                followXVel = new Queue<float>();
                followYVel = new Queue<float>();
                playerFrame = new Queue<int>();
                playerAttack = new Queue<byte>();
                playerDirection = new Queue<bool>();
            }

            projectile.friendly = false;
            projectile.timeLeft++;
            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active || owner.dead) { projectile.timeLeft = 0; return; }
            if (owner.whoAmI == Main.myPlayer)
            {
                // Kill if no combo
                if (!owner.GetModPlayer<ModPlayerFists>().IsComboActive)
                {
                    if(projectile.penetrate < 0) projectile.penetrate = 1;
                    if (Main.netMode != 0 && projectile.owner == owner.whoAmI)
                    {
                        projectile.netUpdate = true;
                    }
                }
            }

            #region Enqueue

            followX.Enqueue(owner.Center.X);
            followY.Enqueue(owner.Center.Y);
            followXVel.Enqueue(owner.velocity.X);
            followYVel.Enqueue(owner.velocity.Y);
            if (owner.itemAnimation > 0)
            {
                playerFrame.Enqueue(owner.bodyFrame.Y / owner.bodyFrame.Height);
            }
            else
            {
                playerFrame.Enqueue(owner.legFrame.Y / owner.legFrame.Height);
            }
            if (owner.itemAnimation > 1)
            {
                playerAttack.Enqueue((byte)(1 + 250f * Math.Min(1f,
                    (owner.itemAnimation - 1f) / (owner.itemAnimationMax - 2f)
                    )));
            }
            else
            { playerAttack.Enqueue(0); }
            playerDirection.Enqueue(owner.direction > 0);

            #endregion

            #region Spawn Escape Code

            if (playerFrame.Count < 60)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 26, 0f, 0f, 150, default(Color), 1f);
                projectile.alpha = 255 - (playerFrame.Count * 5);
                return;
            }
            else
            {
                projectile.alpha = 0;
            }

            #endregion

            #region Dequeue

            projectile.Center = new Vector2(followX.Dequeue(), followY.Dequeue());
            projectile.velocity = new Vector2(followXVel.Dequeue(), followYVel.Dequeue());
            projectile.frame = playerFrame.Dequeue();
            byte attackFrame = playerAttack.Dequeue();
            projectile.friendly = attackFrame > 0;

            projectile.direction = playerDirection.Dequeue() ? 1 : -1;

            #endregion

            #region Snap to Enemies

            if (attackFrame > 0)
            {
                attackFrame -= 125; //251 - 1;
                float reach = Math.Abs(125 - attackFrame) * 0.008f;

                if (projectile.ai[1] == 0f)
                {
                    float distance = 128;
                    float newDistance = 0;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.dontTakeDamage && !npc.friendly)
                        {
                            newDistance = Vector2.Distance(projectile.Center, npc.Center);
                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                projectile.ai[1] = 1;
                                projectile.localAI[0] = npc.Center.X;
                                projectile.localAI[1] = npc.Center.Y;
                            }
                        }
                    }
                }

                if (projectile.ai[1] == 1f && reach > 0f)
                {
                    projectile.Center = new Vector2(
                        MathHelper.Lerp(projectile.Center.X, projectile.localAI[0], reach),
                        MathHelper.Lerp(projectile.Center.Y, projectile.localAI[1], reach));
                    if (projectile.Center.X < projectile.localAI[0]) projectile.direction = 1;
                    else projectile.direction = -1;
                }
            }
            else
            {
                projectile.ai[1] = 0;
                projectile.localAI[0] = projectile.Center.X;
                projectile.localAI[1] = projectile.Center.Y;
            }

            #endregion

            projectile.spriteDirection = projectile.direction;
            if (projectile.penetrate >= 0) projectile.friendly = true;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCHit2, projectile.position);

            for(int i = 0; i < 20; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 26, 0f, 0f, 150, default(Color), 1f);
            }
            Gore.NewGore(projectile.position, projectile.velocity, mod.GetGoreSlot("Gores/Skull"), projectile.scale);
            Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y + 20f), projectile.velocity, 43, projectile.scale);
            Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y + 20f), projectile.velocity, 43, projectile.scale);
            Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y + 34f), projectile.velocity, 44, projectile.scale);
            Gore.NewGore(new Vector2(projectile.position.X, projectile.position.Y + 34f), projectile.velocity, 44, projectile.scale);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if(projectile.owner == Main.myPlayer) damage = (int)(damage * Main.player[projectile.owner].minionDamage);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.penetrate == -1)
            {
                if (projectile.owner == Main.myPlayer && Main.player[projectile.owner].active)
                {
                    Main.player[projectile.owner].GetModPlayer<ModPlayerFists>().ModifyComboCounter(1);
                }
            }
        }
    }
}
