using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesOrich : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orichalcum Glove");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike delivers a flowery finish\n" +
                "Combo grants a protective barrier");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 84; //170dps v 20def
            item.useAnimation = 26; // 30%-50% reduction
            item.knockBack = 6.5f;
            item.tileBoost = 8; // Combo Power

            item.value = Item.sellPrice(0, 1, 26, 50);
            item.rare = 4;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item18;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 24;
        const float fistDashSpeed = 9f;
        const float fistDashThresh = 6f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 28;
        const float altDashSpeed = 13f;
        const float altDashThresh = 10f;
        const float altJumpVelo = 17f;
        const int parryActive = 25;
        const int parryCooldown = 20;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CobaltBar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        // Parry
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                damage += 252;
            }
        }
        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        { OnHit(player, target, damage, 5f, crit); }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        { OnHit(player, target, damage, knockBack, crit); }
        private void OnHit(Player player, Entity target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                for(int i = 0; i < 4; i++)
                {
                    float arg_1B2_0 = player.position.X + (float)(player.width / 2);
                    int direction = player.direction;
                    float num = Main.screenPosition.X;
                    if (direction < 0)
                    {
                        num += (float)Main.screenWidth;
                    }
                    float num2 = Main.screenPosition.Y;
                    num2 += (float)Main.rand.Next(Main.screenHeight);
                    Vector2 vector2 = new Vector2(num, num2);
                    float num3 = target.position.X - vector2.X;
                    float num4 = target.position.Y - vector2.Y;
                    num3 += (float)Main.rand.Next(-50, 51) * 0.1f;
                    num4 += (float)Main.rand.Next(-50, 51) * 0.1f;
                    int num5 = 24;
                    float num6 = (float)Math.Sqrt((double)(num3 * num3 + num4 * num4));
                    num6 = (float)num5 / num6;
                    num6 *= 0.4f + 0.15f * i; // Reduce speed
                    num3 *= num6;
                    num4 *= num6;
                    Projectile.NewProjectile(num, num2, num3, num4, 221, 36, 0f, player.whoAmI, 0f, 0f);

                }
            }
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.AddBuff(mod.BuffType<Buffs.PetalShield>(), 30);
            }
        }

        #region Hardmode Parry Base
        public override bool CanUseItem(Player player)
        {
            if (!AltStats(player))
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParryDash(player, parryActive, parryCooldown, fistJumpVelo - fistJumpVelo / 4, fistDashSpeed / 2f, fistDashThresh / 2);
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 4f, 14f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 4f, 14f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
