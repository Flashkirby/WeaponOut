using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOutExtension.FistWeapons
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class PreHardCombo : ModItem
    {
        public override bool Autoload(ref string name) { return false; }
        public static int altEffect = 0; // ID for when this fist is using the alt effect

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Basic Combo Knuckles");
            Tooltip.SetDefault(
                "<right> consumes combo for for an empowered strike\n" +
                "Combo grants increased life regeneration\n" + 
                "'Build up combo for a sweet effect, and spend it to hit hard!'");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 10; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 18; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 5f;
            item.tileBoost = 7; // Combo Power

            item.value = Item.sellPrice(0, 0, 1, 0);
            item.rare = 1; // < 4, cannot perform second uppercut
            item.shootSpeed = 10 + item.rare / 2; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20; // Actual punching hitbox
        const float fistDashSpeed = 6f; // Speed at start of dash
        const float fistDashThresh = 4f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 11.8f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = fistDashSpeed * 1.5f; // Dash speed using combo
        const float altDashThresh = 4.5f;
        const float altJumpVelo = 14.9f;
        const int comboDelay = 10;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + comboDelay; // Set initial combo animation delay
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position); // Combo activation sound
            }
            // Charging (Pre-Hard)
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize);
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // =================== BEHAVIOURS =================== //

                // Charge effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 31, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 10f;
                d.velocity /= 2;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                // =================== BEHAVIOURS =================== //

                // Allow dash
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            else
            {
                // =================== BEHAVIOURS =================== //

                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 31, 3, 3, 100, default(Color), 1f)];
                d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }

        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            if (AltStats(player)) // If dashing
            {
                // =================== BEHAVIOURS =================== //

                damage = (int)(damage * 4f); // x4 Empowered damage

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }

        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                // =================== BEHAVIOURS =================== //

                player.lifeRegenCount += 2; // Regen 1 health a second

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }




        #region Pre-Hardmode Combo Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
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
        // Combo activate
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect);
        }
        // Hitbox and special attack movement
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f); }
            else
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, altDashSpeed); }
        }
        // Modify tooltip to replace tileboost with combo power
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
        #endregion
    }
}
