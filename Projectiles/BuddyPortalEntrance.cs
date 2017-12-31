using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.Localization;

namespace WeaponOut.Projectiles
{
    public class BuddyPortalEntrance : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Team Portal Entrance");
            DisplayName.AddTranslation(GameCulture.Chinese, "团队传送门入口");
            Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 68;
            projectile.tileCollide = false;
            projectile.timeLeft = 1200;
            projectile.netImportant = true;
        }
        private bool instanced = false;
        public override void AI()
        {
            if(!instanced)
            {
                for(int i = 0; i < 50; i++)
                {
                    Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 16);
                    d.scale *= 2f;
                    d.velocity *= 0.5f;
                }
                instanced = true;
            }

            Lighting.AddLight(projectile.Center, Main.teamColor[Main.player[projectile.owner].team].ToVector3() * 0.5f);

            if (projectile.localAI[0] == 1 && CanUsePortal(Main.LocalPlayer))
            {
                projectile.localAI[0] = 2;
                Main.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy);
                
                foreach (Projectile p in Main.projectile)
                {
                    if (p.active && projectile.owner == p.owner)
                    {
                        if (p.type == mod.ProjectileType<BuddyPortalExit>())
                        {
                            Vector2 newPosition = p.Center - new Vector2(Main.LocalPlayer.width, Main.LocalPlayer.height) / 2;
                            Main.LocalPlayer.Teleport(newPosition, 3);
                        }
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
            {
                Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 15);
                d.scale *= 2f;
                d.velocity *= 0.5f;
            }
        }

        public bool CanUsePortal(Player player)
        {
            Player owner = Main.player[projectile.owner];
            return (owner.team == 0 || owner.team == player.team);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            // Draw frame
            spriteBatch.Draw(
                texture, projectile.Center - Main.screenPosition, 
                new Rectangle(0, 0, texture.Width, frameHeight), lightColor,
                0f, new Vector2(texture.Width, frameHeight) / 2, 1f, SpriteEffects.None, 0f);

            // Draw Colourised Door
            Color teamColour = Main.teamColor[Main.player[projectile.owner].team];
            spriteBatch.Draw(
                texture, projectile.Center - Main.screenPosition, 
                new Rectangle(0, frameHeight, texture.Width, frameHeight),
                teamColour, 0f, new Vector2(texture.Width, frameHeight) / 2, 1f, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if ((!Main.gamePaused || Main.gameMenu) && CanUsePortal(Main.LocalPlayer))
            {
                Vector2 vector79 = projectile.position - Main.screenPosition;
                if ((float)Main.mouseX > vector79.X && (float)Main.mouseX < vector79.X + (float)projectile.width && (float)Main.mouseY > vector79.Y && (float)Main.mouseY < vector79.Y + (float)projectile.height)
                {
                    int num351 = (int)(Main.player[Main.myPlayer].Center.X / 16f);
                    int num352 = (int)(Main.player[Main.myPlayer].Center.Y / 16f);
                    int num353 = (int)projectile.Center.X / 16;
                    int num354 = (int)projectile.Center.Y / 16;
                    int lastTileRangeX = Main.player[Main.myPlayer].lastTileRangeX;
                    int lastTileRangeY = Main.player[Main.myPlayer].lastTileRangeY;
                    if (num351 >= num353 - lastTileRangeX && num351 <= num353 + lastTileRangeX + 1 && num352 >= num354 - lastTileRangeY && num352 <= num354 + lastTileRangeY + 1)
                    {
                        Main.player[Main.myPlayer].noThrow = 2;
                        Main.player[Main.myPlayer].showItemIcon = true;
                        Main.player[Main.myPlayer].showItemIcon2 = mod.ItemType<Items.BuddyHorn>();
                        if (PlayerInput.UsingGamepad)
                        {
                            Main.player[Main.myPlayer].GamepadEnableGrappleCooldown();
                        }
                        if (Main.mouseRight && Main.mouseRightRelease)// && Player.StopMoneyTroughFromWorking == 0)
                        {
                            Main.mouseRightRelease = false;
                            projectile.localAI[0] = 1;
                        }
                    }
                }
            }
        }
    }
}
