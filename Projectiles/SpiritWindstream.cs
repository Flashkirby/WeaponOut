using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Localization;

namespace WeaponOut.Projectiles
{
    public class SpiritWindstream : ModProjectile
    {
        public override bool Autoload(ref string name) { return true; }//TESTING4BREAK
        public static int buffID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chilly Slipstream");
            DisplayName.AddTranslation(GameCulture.Chinese, "寒冰气流");
			DisplayName.AddTranslation(GameCulture.Russian, "Холодный След");
            buffID = ModContent.BuffType<Buffs.Frostbite>();
        }
        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;
            projectile.alpha = 255;

            projectile.timeLeft = 180;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.friendly = true;

            projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = -1;
            projectile.melee = true;
        }

        public override void AI()
        {
            Dust d = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 113, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 0.1f);
            d.fadeIn = 0.6f;
            d.velocity *= 0.2f;
            d.velocity += projectile.velocity;

            projectile.velocity *= 0.99f;
        }

        public override bool? CanCutTiles() { return false; }
        

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        { crit = false; }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        { crit = false; }


        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 480, false);
            target.AddBuff(buffID, 600, false);
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 480, false);
            target.AddBuff(buffID, 600, false);
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 480, false);
            target.AddBuff(buffID, 600, false);
        }
    }
}
