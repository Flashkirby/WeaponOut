using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Acts a bit like the solar eruption
    /// ai0 = time out?
    /// local ai0 = projectile rotation
    /// </summary>
    public class NotchedWhip : ModItem
    {
        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Notched Whip";
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 18;
            item.useTime = 18;
            item.useSound = 19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.channel = true;
            item.damage = 11;
            item.crit = 21;
            item.knockBack = 2f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length


            item.rare = 0;
            item.value = Item.sellPrice(0,0,1,0);
        }
    }
}