using System;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class ManaSword : ModItem
    {
        HelperDual dual;
        public override void SetDefaults()
        {
            item.name = "Mana Sword";
            item.toolTip = "";
            item.width = 58;
            item.height = 28;
            item.scale = 0.9f;

            item.autoReuse = true;

            item.useSound = 28;
            item.useStyle = 1; //swing
            item.useAnimation = 16;
            item.useTime = 15;

            item.magic = true;
            item.damage = 10;
            item.knockBack = 5f;

            item.mana = 10;
            item.shoot = mod.ProjectileType("ManaBlast");
            item.shootSpeed = 11f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 4;
            item.value = 10;

            dual = new HelperDual(item, true); //prioritise magic defaults
            dual.UseSound = 60;
            dual.UseStyle = 5;
            dual.UseAnimation = 40;
            dual.UseTime = 40;

            dual.NoMelee = true;
            dual.Damage = 30;
            dual.KnockBack = 10f;

            dual.Mana = 10;
            dual.Shoot = ProjectileID.ChargedBlasterOrb; //staff one is magic, sword one is melee
            dual.ShootSpeed = 14f;

            dual.setValues(false, true);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.EnchantedSword, 1);
            recipe.AddIngredient(mod.GetItem("ManaBlast").item.type, 5);
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

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 dustPosition;
            Vector2 dustVelocity = CalculateDustVelocityNormal(player, -1.57f); //send dust flying backwards of blade vlocity
            for (int i = 0; i < 10; i++)
            {
                float dist = Main.rand.NextFloat();
                dustPosition = CalculateDustPosition(player, item, 4, dist, 4f);
                int d = Dust.NewDust(dustPosition, 0, 0,
                    WeaponOut.DustIDManaDust, player.velocity.X, player.velocity.Y, 100, Color.White, 1.3f - 0.8f * dist);
                Main.dust[d].velocity += dustVelocity * 5 * dist;
            }
        }

        private static Vector2 CalculateDustPosition(Player player, Item item, int handleLength, float distNormal, float scaleDisplace)
        {
            handleLength += 16;
            int length = (int)(item.width * 1.414f * item.scale) - handleLength;
            float cosRot = (float)Math.Cos(player.itemRotation - 0.78f * player.direction * player.gravDir);
            float sinRot = (float)Math.Sin(player.itemRotation - 0.78f * player.direction * player.gravDir);
            if (length < 1) length = 1;
            return new Vector2(
                        (float)(player.itemLocation.X + (handleLength + length * distNormal) * cosRot * player.direction) - scaleDisplace,
                        (float)(player.itemLocation.Y + (handleLength + length * distNormal) * sinRot * player.direction) - scaleDisplace);
        }
        private static Vector2 CalculateDustVelocityNormal(Player player, float angleOffset)
        {
            float cosRot = (float)Math.Cos(player.itemRotation - (0.78f - angleOffset) * player.direction * player.gravDir);
            float sinRot = (float)Math.Sin(player.itemRotation - (0.78f - angleOffset) * player.direction * player.gravDir);
            return new Vector2(
                        (float)(cosRot * player.direction),
                        (float)(sinRot * player.direction));
        }

    }
}
