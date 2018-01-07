using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Projectiles
{
    public class SpiritShadow : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadow Clone");
			DisplayName.AddTranslation(GameCulture.Russian, "Теневой Клон");
        }
        public override void SetDefaults()
        {
            projectile.width = Player.defaultWidth + 26 * 2;
            projectile.height = Player.defaultHeight + 26 * 2;
            projectile.alpha = 255;

            projectile.penetrate = -1;
            projectile.timeLeft = 60;

            projectile.friendly = false;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            // So it doesn't conflict with fist NPC cooldown
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            projectile.friendly = false;
            projectile.timeLeft++;
            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active || owner.dead) { projectile.timeLeft = 0; return; }
            if (owner.whoAmI == Main.myPlayer)
            {
                // Kill if no combo
                if (!owner.GetModPlayer<ModPlayerFists>().IsComboActive)
                {
                    projectile.timeLeft = 0;
                    if (Main.netMode != 0 && projectile.owner == owner.whoAmI)
                    {
                        projectile.netUpdate = true;
                    }
                }
            }

            projectile.velocity = owner.velocity;
            projectile.Center = owner.Center + new Vector2(30 * projectile.ai[0], owner.gfxOffY) - projectile.velocity;

            if (projectile.ai[0] == 1f)
            {
                owner.shadowDodgeCount++;
                if (!owner.shadowDodge) owner.shadowDodgeCount++;// nullify effect of countdown
                if (owner.shadowDodgeCount > 31) owner.shadowDodgeCount = 31;
            }

            projectile.friendly = owner.itemAnimation > 1 && owner.itemAnimation <= owner.itemAnimationMax - 6;
        }
    }
}
