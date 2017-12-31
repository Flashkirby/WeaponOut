using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOutExtension.FistWeapons
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class PreHardParry : ModItem
    {
        public override bool Autoload(ref string name) { return false; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Basic Parry Glove");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike sets enemies alight\n" +
                "Combo removes the effects of On Fire!\n" +
                "'Parry to gain access to a counterstrike'");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 10; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 22; // Reduced by 30-50% on hit, increasing DPS
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
        const float fistDashSpeed = 5f; // Speed at start of dash
        const float fistDashThresh = 4f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 11.8f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 28;
        const float altDashSpeed = 7f; // Dash speed when counterstriking
        const float altDashThresh = 4.5f;
        const float altJumpVelo = fistJumpVelo;
        const int parryActive = 20;
        const int parryCooldown = 20;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player)) // If counterstrike is available
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff(); // Consume counterstrike buff

                // =================== BEHAVIOURS =================== //

                target.AddBuff(BuffID.OnFire, 600, false);

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }
        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player)) // If counterstrike is available
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff(); // Consume counterstrike buff

                // =================== BEHAVIOURS =================== //

                player.AddBuff(BuffID.OnFire, 600, false);

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }

        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive) // Combo is currently at power cost
            {
                // =================== BEHAVIOURS =================== //

                player.ClearBuff(BuffID.OnFire);

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }




        #region Pre-Hardmode Parry Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
        //
        // ITEM CODE CHANGES - DON'T MESS UNLESS YOU KNOW WHAT YOU'RE DOIN
        //

        // Apply dash on punch
        public override bool CanUseItem(Player player)
        {
            if (AltStats(player))
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                if (player.altFunctionUse == 0)
                {
                    player.GetModPlayer<ModPlayerFists>().
                    SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
                }
            }
            return true;
        }
        // Parry frames
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParry(player, parryActive, parryCooldown);
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