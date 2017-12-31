using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

using WeaponOut;
using System;

namespace WeaponOut.Items.Weapons.Fists
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class AdvancedDash : ModItem
    {
        public override bool Autoload(ref string name) { return false; }
        public static int dashEffect = 0; // ID for when this fist is dashing
        public static int altEffect = 0; // ID for when this fist is using combo power

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Advanced Dash Fist");
            Tooltip.SetDefault(
                "<right> to dash, or consume combo to bulldoze enemies\n" +
                "Dash grants 50% increased melee damage\n" +
                "'Roses are red, violets are blue, I am going, to punch you'");
            dashEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 80; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 25; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 7f;
            item.tileBoost = 14; // Combo Power

            item.value = Item.sellPrice(0, 0, 1, 0);
            item.rare = 8; // >= 4, can use second uppercut
            item.shootSpeed = 10 + item.rare / 2; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item20;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22; // Actual punching hitbox
        const float fistDashSpeed = 10f; // Speed at start of dash
        const float fistDashThresh = 7f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 14f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool DashStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == dashEffect; }
        public bool ComboStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = 17f; // Dash speed when dashing through enemies
        const float altDashThresh = 13f;
        const float altJumpVelo = 18f;
        const int comboDelay = 10;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a dash. Use for ongoing effects like dust. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0)
            {
                // =================== BEHAVIOURS =================== //

                // Make some smoke cloud gores
                Gore g;
                if (player.velocity.Y == 0f)
                { g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f)]; }
                else
                { g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 14f), default(Vector2), Main.rand.Next(61, 64), 1f)]; }
                g.velocity.X = (float)Main.rand.Next(-50, 51) * 0.01f;
                g.velocity.Y = (float)Main.rand.Next(-50, 51) * 0.01f;
                g.velocity *= 0.4f;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            // =================== BEHAVIOURS =================== //

            for (int i = 0; i < 3; i++) // Fire!
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 90,
                    -player.velocity.X, 0, 100, default(Color), 2 + i * 0.15f)];
                d.noGravity = true;
                d.velocity.Y = player.velocity.Y * -0.5f;
                d.velocity *= 0.7f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);

                d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 92,
                   -player.velocity.X, 0, 100, default(Color), 2 + i * 0.15f)];
                d.noGravity = true;
                d.velocity.Y = player.velocity.Y * -0.5f;
                d.velocity *= 0.7f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }

            // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + comboDelay; // Set initial combo animation delay
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true; // Hardmode combos reset uppercut
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position); // Combo activation sound
            }
            // Charging (Hardmode)
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // =================== BEHAVIOURS =================== //

                // Charge effect
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 162, 0, 0, 0, default(Color), 1.5f)];
                    d.position -= d.velocity * 10f;
                    d.velocity /= 2;
                    d.noGravity = true;
                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                player.GetModPlayer<ModPlayerFists>().SetDash(
                    altDashSpeed * 1.1f, altDashThresh * 1.1f, 0.992f, 0.96f, true,
                    dashEffect); // set to 0 to use default attackCD (or just not to hit all enemies)
                // =================== BEHAVIOURS =================== //
                
                // Swag dust ring
                for (int i = 0; i < 64; i++)
                {
                    double angle = Main.time + i / 10.0;
                    Dust d = Dust.NewDustPerfect(player.Center, i % 2 == 0 ? 92 : 90,
                        new Vector2((float)(5.0 * Math.Sin(angle)), (float)(5.0 * Math.Cos(angle))));
                    d.noGravity = true;
                    d.fadeIn = 1.3f;
                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            else
            {
                // =================== BEHAVIOURS =================== //

                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 173, 3, 3, 0, default(Color), 1f)];
                d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);
                d.noGravity = true;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }


        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            // Dashing Bonus
            if (DashStats(player)) // If dashing
            {
                // =================== BEHAVIOURS =================== //

                damage = (int)(damage * 1.5f); // increases damage by 50%
                knockBack = 0f; // No knockback

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }

            // Combo Bonus
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (ComboStats(player))
            {
                // =================== BEHAVIOURS =================== //

                damage = (int)(damage * 2); // increases damage by a total of 300%
                knockBack *= 4; // also knockback why not

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }




        #region Advanced Dash-Combo Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
        //
        // ITEM CODE CHANGES - DON'T MESS UNLESS YOU KNOW WHAT YOU'RE DOIN
        //

        // Apply dash on punch
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        // Apply better dash on alternate punch
        public override bool AltFunctionUse(Player player)
        {
            if (!player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect))
            {
                return player.dashDelay == 0 && player.GetModPlayer<ModPlayerFists>().
                        SetDash(altDashSpeed, altDashThresh, 0.992f, 0.96f, false, dashEffect); ;
            }
            return true;
        }
        // Hitbox and special attack movement
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!DashStats(player))
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 16f); }
            else
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, altDashSpeed); }
        }
        // Modify tooltip to replace tileboost with combo power
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
        #endregion
    }
}
