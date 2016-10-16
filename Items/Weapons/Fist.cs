using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class Fist : ModItem
    {
        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Bare Fist";
            item.toolTip = "Damage scales with chest defense";
            item.useStyle = FistStyle.useStyle;
            item.useAnimation = 19;//actually treated as -2
            item.damage = 2;
            item.knockBack = 2f;
            item.useSound = 7;
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
