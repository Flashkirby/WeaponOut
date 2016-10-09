using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class Capacitor : ModItem
    {
        HelperDual dual;
        public override void SetDefaults()
        {
            item.name = "Capacitor";
            item.toolTip = "Right click to cast a frost bolt\nMelee attacks grant 80% reduced mana cost";
            item.width = 40;
            item.height = 40;
            item.scale = 1.15f;

            item.autoReuse = true;

            item.useSound = 1;
            item.useStyle = 1; //swing
            item.useAnimation = 16;
            item.useTime = 15;

            item.melee = true; //melee damage
            item.damage = 17;
            item.knockBack = 4f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 2;
            item.value = 20000;

            dual = new HelperDual(item, true); //prioritise magic defaults
            dual.UseSound = 0;
            dual.UseStyle = 5;
            dual.UseAnimation = 21;
            dual.UseTime = 21;

            dual.Magic = true;
            dual.NoMelee = true;
            dual.Damage = 30;
            dual.KnockBack = 0f;

            dual.Mana = 16;
            dual.Shoot = ProjectileID.FrostBoltStaff; //staff one is magic, sword one is melee
            dual.ShootSpeed = 14f;

            dual.setValues(false, true);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IceBlade, 1);
            recipe.AddIngredient(ItemID.FallenStar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player) { return true; }
        public override void UseStyle(Player player)
        {
            dual.UseStyleMultiplayer(player);
            if (player.altFunctionUse > 0) PlayerFX.modifyPlayerItemLocation(player, -6, 0);
        }
        public override bool CanUseItem(Player player)
        {
            dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player)
        {
            dual.HoldStyle(player);
            base.HoldStyle(player);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            player.AddBuff(WeaponOut.BuffIDManaReduction, 180);//3 second buff
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            if (player.HasBuff(WeaponOut.BuffIDManaReduction) != -1)
            {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 15, (player.velocity.X * 0.2f) + (player.direction * 3), player.velocity.Y * 0.2f, 100, Color.White, 1.3f);
                Main.dust[d].noGravity = true;
            }
            else
            {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 15, (player.velocity.X * 0.2f) + (player.direction * 3), player.velocity.Y * 0.2f, 100, Color.White, 0.6f);
                Main.dust[d].noGravity = true;
            }
        }

    }
}
