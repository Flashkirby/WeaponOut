using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class MirrorBarrier : ModBuff
    {
        public int manaCost { get { return mod.GetItem("MirrorBadge").item.mana; } }
        public const float shieldDist = 40f;

        public override void SetDefaults()
        {
            Main.buffName[Type] = "Magic Barrier";
            Main.buffTip[Type] = "Hostile projectiles may be reflected";
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ReflectProjectiles(player);
        }

        public void ReflectProjectiles(Player player)
        {
            Projectile projectile;
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                projectile = Main.projectile[i];
                //ignore these
                if (!projectile.active || (projectile.friendly && projectile.owner == player.whoAmI) || projectile.melee ||
                    projectile.damage <= 0 || projectile.aiStyle == 7 || projectile.aiStyle == 13 || projectile.aiStyle == 15 ||
                    projectile.aiStyle == 19 || projectile.aiStyle == 20 || projectile.aiStyle == 26) continue;

                Player projOwner = Main.player[projectile.owner];
                Vector2 projCentre = projectile.Center;
                Vector2 playerCentre = player.Center;
                //close enough
                if (Vector2.Distance(projCentre, playerCentre) < shieldDist + Vector2.Distance(default(Vector2), projectile.velocity * 2.5f))
                {
                    //		NPC					concerns shielded player		OR		player proj	   concerns the projectile owner due to PVP	  both shooter and target are PVP		shooter on no team OR   shooter in different team to target
                    if ((projectile.hostile && Main.myPlayer == player.whoAmI) || (projectile.friendly && Main.myPlayer == projectile.owner && projOwner.hostile && player.hostile && (projOwner.team == 0 || projOwner.team != player.team) && !projectile.minion))
                    {
                        //check if there are projectiles in close proximity with the player
                        //Main.NewText("Have send!");
                        //bool selfHandle = true;
                        //if (projectile.friendly) selfHandle = false;
                        if(projectile.damage < 100 * Main.damageMultiplier) ReflectProjectile(i, player);
                        //Codable.RunGlobalMethod("ModWorld", "ReflectProjectile", player.whoAmI, projectile.owner, projectile.whoAmI, projectile.position.X, projectile.position.Y, projectile.velocity.X, projectile.velocity.Y, projectile.damage, projectile.knockBack, selfHandle);
                    }
                }
            }
        }
        public void ReflectProjectile(int i, Player player)
        {
            //spend mana
            if (player.CheckMana(manaCost, true, false))
            {
                //add delay
                if (player.manaRegenDelay < 60) player.manaRegenDelay += 60;

                //vanilla reflect code
                Projectile projectile = Main.projectile[i];

                //swap projectile around
                projectile.hostile = false;
                projectile.friendly = true;
                projectile.velocity *= -1f;
                projectile.penetrate = 1;

                //extra code
                if (projectile.Center.X > player.Center.X * 0.5f)
                {
                    projectile.direction = 1;
                    projectile.spriteDirection = 1;
                }
                else
                {
                    projectile.direction = -1;
                    projectile.spriteDirection = -1;
                }
                projectile.owner = player.whoAmI;
                projectile.timeLeft = projectile.timeLeft / 2;
                //Main.NewText("Reflected projectile: "+projectile.name+" | "+projectile.damage+" | "+projectile.knockBack+" | "+Main.player[projectile.owner].name);

                //shield visual
                Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 28);
                Vector2 spawnPosition;
                for (int j = 0; j < 30; j++)
                {
                    int d = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 15, j * projectile.velocity.X * 0.03f, j * projectile.velocity.Y * 0.03f, 50, Color.Transparent, 1f);
                    Main.dust[d].velocity *= 0.5f;

                    //circle effect
                    float rotToTarget = Main.rand.Next((int)(-Math.PI * 10000), (int)(Math.PI * 10000)) / 10000f;
                    spawnPosition = player.position
                        + new Vector2(player.width / 2 - 2, player.height / 2)
                        + new Vector2((float)(shieldDist * Math.Cos(rotToTarget)), (float)(shieldDist * Math.Sin(rotToTarget)));
                    d = Dust.NewDust(spawnPosition, 0, 0, 43, 2f * (float)Math.Cos(rotToTarget), 2f * (float)Math.Sin(rotToTarget), 0, Color.White, 0.4f);
                    Main.dust[d].fadeIn = 1f;
                    Main.dust[d].velocity *= 0.25f;
                }
            }
        }
    }
}
