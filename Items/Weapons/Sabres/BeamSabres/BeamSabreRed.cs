using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres.BeamSabres
{
    public class BeamSabreRed : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Red Beam Saber");
        }

        public override Color SabreColour() { return new Color(1f, 0.1f, 0f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabreRedSlash>(); }
    }
    public class BeamSabreRedSlash : BeamSabreSlash
    { }
}
