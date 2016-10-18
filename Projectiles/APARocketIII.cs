using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Projectiles
{
    public class APARocketIII : ModProjectile
    {
        public override void SetDefaults()
        {
            APARocketI.setDefaults(projectile);
        }
    }
}
