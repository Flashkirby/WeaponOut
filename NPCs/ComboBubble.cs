using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.NPCs
{
    public class ComboBubble : ModNPC
    {
        public override void SetDefaults()
        {
            npc.width = 36;
            npc.height = 36;
            npc.aiStyle = -1;
            npc.damage = 0;
            npc.defense = 0;
            npc.lifeMax = 1;
            npc.HitSound = SoundID.NPCHit3;
            npc.DeathSound = SoundID.NPCDeath3;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.knockBackResist = 1.5f;
            npc.alpha = 255;
            for (int b = 0; b < npc.buffImmune.Length; b++) { npc.buffImmune[b] = true; }
            Main.npcFrameCount[npc.type] = 2;
        }

        public const float maxDist = 100;
        public override void AI()
        {
            npc.alpha = Math.Max(0, npc.alpha - 9); // Fadein
            if (!npc.HasPlayerTarget)
            {
                npc.StrikeNPCNoInteraction(9999, 0f, 0, false, false, false);
                return;
            }
            Player player = Main.player[npc.target];
            Vector2 dist = player.Center - npc.Center;
            bool tooFar, tooClose;
            tooFar = (Math.Abs(dist.X) > maxDist || Math.Abs(dist.Y) > maxDist);
            tooClose = (Math.Abs(dist.X) < maxDist / 2 || Math.Abs(dist.Y) < maxDist / 2);

            npc.velocity *= 0.985f;
            Vector2 direction = default(Vector2);
            if (tooFar)
            {
                direction = player.Center - npc.Center;
                direction.SafeNormalize(default(Vector2));
                direction *= npc.Distance(player.Center) / (maxDist);
            }
            else if (tooClose)
            {
                direction = npc.Center - player.Center;
                direction.SafeNormalize(default(Vector2));
            }
            direction /= 50;
            npc.velocity = (npc.velocity * 59 + direction) / 60;
        }
        public override void FindFrame(int frameHeight)
        {
            npc.frame.Y = npc.whoAmI % 2 * frameHeight;
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            Main.PlaySound(4, (int)npc.position.X, (int)npc.position.Y, 3, 1f, 0f);
            if (npc.life <= 0)
            {
                // Do this here because in multiplayer client (which spawns these) doesn't run NPCLoot)
                Player player = Main.player[npc.target];
                ModPlayerFists.Get(player).ModifyComboCounter(1);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 veloRand = Main.rand.NextVector2Square(-3f, 3f);
                    Projectile.NewProjectile(npc.Center + player.velocity * 2, player.velocity + veloRand, ProjectileID.FlaironBubble, player.HeldItem.damage / 5, 5f, npc.target, -10f, 0);
                }

                // Actual hit effect
                for (int i = 0; i < 60; i++)
                {
                    int num209 = 25;
                    Vector2 vector10 = ((float)Main.rand.NextDouble() * 6.28318548f).ToRotationVector2() * (float)Main.rand.Next(24, 41) / 8f;
                    int num210 = Dust.NewDust(npc.Center - Vector2.One * (float)num209, num209 * 2, num209 * 2, 212, 0f, 0f, 0, default(Color), 1f);
                    Dust dust48 = Main.dust[num210];
                    Vector2 vector11 = Vector2.Normalize(dust48.position - npc.Center);
                    dust48.position = npc.Center + vector11 * 25f * npc.scale;
                    if (i < 30)
                    {
                        dust48.velocity = vector11 * dust48.velocity.Length();
                    }
                    else
                    {
                        dust48.velocity = vector11 * (float)Main.rand.Next(45, 91) / 10f;
                    }
                    dust48.color = Main.hslToRgb((float)(0.40000000596046448 + Main.rand.NextDouble() * 0.20000000298023224), 0.9f, 0.5f);
                    dust48.color = Color.Lerp(dust48.color, Color.White, 0.3f);
                    dust48.noGravity = true;
                    dust48.scale = 0.7f;
                }
            }
        }
    }
}
