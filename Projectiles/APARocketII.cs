using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;


namespace WeaponOut.Projectiles
{
    public class APARocketII : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Roocket"); //134, 137, 140, 143
            DisplayName.AddTranslation(GameCulture.Chinese, "火箭II");
			DisplayName.AddTranslation(GameCulture.Russian, "Раакета");
        }
        public override void SetDefaults()
        {
            APARocketI.setDefaults(projectile);
        }
        public override void AI()
        {
            APARocketI.rocketAI(projectile, 89);
        }
        public override void Kill(int timeLeft)
        {
            APARocketI.aiKill(projectile, false, 2);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            APARocketI.OnHitCollide(projectile);
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            APARocketI.OnHitCollide(projectile);
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            APARocketI.OnHitCollide(projectile);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            APARocketI.OnHitCollide(projectile, true); return false;
        }
    }
}
