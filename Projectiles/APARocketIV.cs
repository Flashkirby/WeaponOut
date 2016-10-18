using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class APARocketIV : ModProjectile
    {
        public override void SetDefaults()
        {
            APARocketI.setDefaults(projectile);
        }
    }
}
