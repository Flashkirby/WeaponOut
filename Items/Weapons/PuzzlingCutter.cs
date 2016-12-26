using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Not the garian sword :P
    /// </summary>
    public class PuzzlingCutter : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModCfg.enableWhips;
        }

        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Puzzling Cutter";
            item.toolTip = "'No one said it was a sword'";
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 17;
            item.useTime = 17;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.autoReuse = true;
            
            item.melee = true;
            item.channel = true;
            item.damage = 20;
            item.crit = 36; //crit chance on whips increase crit damage instead
            item.knockBack = 1f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.rare = 5;
            item.value = Item.sellPrice(0,6,0,0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0,
                0.15f * player.gravDir + Main.rand.Next(-50, 50) * 0.001f * player.gravDir); //overhead swinging
            return false;
        }
    }
}