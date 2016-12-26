using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class Fist : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableFists;
        }

        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Bare Fist";
            item.toolTip = "Damage scales with chest defense";
            item.useStyle = FistStyle.useStyle;
            item.useAnimation = 19;//actually treated as -2
            item.width = 28;
            item.height = 28;
            item.damage = 2;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item7;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override bool UseItemFrame(Player player)
        {
            if (!increaseDamage)
            {
                //Main.NewText((player.itemAnimation - 2) + "/" + player.itemAnimationMax);
                item.damage += player.armor[1].defense; //defence increase attack
                increaseDamage = true;
            }
            FistStyle.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = FistStyle.UseItemHitbox(player, ref hitbox, 4);
        }
    }
}
