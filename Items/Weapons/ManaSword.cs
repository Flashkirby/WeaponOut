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
            item.toolTip = @"Casts a mana restoring star
Right click to cast a powerful mana bolt
Mana bolt damage increases with mana";
            item.width = 58;
            item.height = 28;
            item.scale = 0.9f;

            item.autoReuse = true;

            item.useSound = 28;
            item.useStyle = 1; //swing
            item.useAnimation = 16;
            item.useTime = 15;

            item.magic = true;
            item.damage = 32;
            item.knockBack = 5f;

            item.mana = 10;
            item.shoot = mod.ProjectileType("ManaBlast");
            item.shootSpeed = 11f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 5;
            item.value = Item.sellPrice(0, 5, 0, 0);

            dual = new HelperDual(item, true); //prioritise magic defaults
            dual.UseSound = 68;
            dual.UseStyle = 5;
            dual.UseAnimation = 40;
            dual.UseTime = 40;

            dual.NoMelee = true;
            dual.Damage = 100;
            dual.KnockBack = 10f;

            dual.Mana = 30;
            dual.Shoot = mod.ProjectileType("ManaBolt");
            item.shootSpeed = 30f;

            dual.setValues(false, true);
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.AdamantiteSword, 1);
                }
                else
                {
                    recipe.AddIngredient(ItemID.TitaniumSword, 1);
                }
                recipe.AddIngredient(mod.GetItem("ManaBlast").item.type, 1);
                recipe.AddTile(TileID.MythrilAnvil);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }
        public override void OnCraft(Recipe recipe)
        {
            HelperDual.OnCraft(item);
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

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse > 0)
            {
                //Main.NewText("Bolt buffed by " + (player.statMana / 4));
                damage += player.statMana / 4; //up to 100 with max mana, though will be around 25
            }
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
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
