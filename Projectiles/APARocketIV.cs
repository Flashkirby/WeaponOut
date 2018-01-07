using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Projectiles
{
    public class APARocketIV : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Roooocket"); //134, 137, 140, 143
            DisplayName.AddTranslation(GameCulture.Chinese, "火箭IV");
            DisplayName.AddTranslation(GameCulture.Russian, "Рааaакета");
        }
        public override void SetDefaults()
        {
            APARocketI.setDefaults(projectile);
        }
        public override void AI()
        {
            APARocketI.rocketAI(projectile, 87);
        }
        public override void Kill(int timeLeft)
        {
            APARocketI.aiKill(projectile, true, 4);
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
