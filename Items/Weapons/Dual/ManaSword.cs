using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Dual
{
    public class ManaSword : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mana Sword");
			DisplayName.AddTranslation(GameCulture.Russian, "Меч Маны");
            Tooltip.SetDefault(
                "Casts a mana restoring star\n" +
                "<right> to cast a powerful mana bolt\n" +
                "Mana bolt damage increases with mana");
				Tooltip.AddTranslation(GameCulture.Russian,
				"Пускает звезду, крадущую ману\n" +
                "<right>, чтобы послать мощный снаряд маны\n" +
                "Больше маны - мощнее снаряд");
        }
        public override void SetDefaults()
        {
            item.width = 58;
            item.height = 28;
            item.scale = 0.9f;

            item.autoReuse = true;

            item.UseSound = SoundID.Item28;
            item.useStyle = 5;
            item.useAnimation = 15;
            item.useTime = 15;

            item.magic = true;
            item.damage = 30;
            item.knockBack = 5f;

            item.mana = 10;
            item.shoot = mod.ProjectileType<Projectiles.ManaBlast>();
            item.shootSpeed = 11f;

            Item.staff[item.type] = true; //rotate weapon, as it is a staff

            item.rare = 5;
            item.value = Item.sellPrice(0, 5, 0, 0);
        }
        public override void AddRecipes() {
            if (!ModConf.EnableDualWeapons || !ModConf.EnableBasicContent) return;
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
                recipe.AddIngredient(mod.GetItem<Basic.ManaBlast>().item.type, 1);
                recipe.AddTile(TileID.MythrilAnvil);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public override bool AltFunctionUse(Player player) { return true; }

        // Light weight, less flexible, but much safer
        public override bool CanUseItem(Player player)
        {
            if (PlayerFX.DualItemCanUseItemAlt(player, this,
                1f, 1f,
                1f, 0.25f))
            {
                item.useStyle = 5;
                item.UseSound = SoundID.Item68;
                item.noMelee = true;
                player.manaCost *= 4f;
                item.shoot = mod.ProjectileType<Projectiles.ManaBolt>();
            }
            else
            {
                item.useStyle = 1;
                item.UseSound = SoundID.Item28;
                item.noMelee = false;
                item.shoot = mod.ProjectileType<Projectiles.ManaBlast>();
            }
            return true;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (target.immortal) return;
            player.statMana += 3;
            if(player.whoAmI == Main.myPlayer) player.ManaEffect(3); //other players will see this already
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 0)
            { }
            else
            {
                damage = (int)(damage * 3.125f);
                knockBack *= 2f;
                //speedX *= 2.7f;
                //speedY *= 2.7f;

                damage += player.statMana / 2; //up to 200 with max mana, 50 default
                //Main.NewText("Bolt buffed by \n" + (player.statMana / 2) + " to \n" + damage);
            }
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f,0);
        }

        #region Melee Effects
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
        #endregion
    }
}
