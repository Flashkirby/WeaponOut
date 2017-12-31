using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOut.Items.Weapons.Fists
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class AdvancedParry : ModItem
    {
        public override bool Autoload(ref string name) { return false; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Advanced Parry Glove");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike grants bonus damage based on enemy power\n" +
                "Combo grants lifestealing parries\n" +
                "'Hold back to bounce off enemies, or forward to dash through'");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 80; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 22; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 5f;
            item.tileBoost = 15; // Combo Power

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 8; // >= 4, can use second uppercut
            item.shootSpeed = 10 + item.rare / 2; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item18;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20; // Actual punching hitbox
        const float fistDashSpeed = 9f; // Speed at start of dash
        const float fistDashThresh = 6f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 30;
        const float altDashSpeed = 19f; // Dash speed when counterstriking
        const float altDashThresh = 14f;
        const float altJumpVelo = 17f;
        const int parryActive = 25;
        const int parryCooldown = 15;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit, target.statDefense); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit, target.defDefense); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit, int EnemyPower)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.parryBuff)
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();

                damage += 120 + EnemyPower * 2;
            }
        }

        public override void UpdateInventory(Player player)
        {
            if (player.HeldItem == item)
            {
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                if (mpf.IsComboActive)
                {
                    mpf.parryLifesteal += 0.2f;
                }
            }
        }




        #region Hardmode Parry-Dash Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
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
        // Parry-dash
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParryDash(player, parryActive, parryCooldown, fistJumpVelo, fistDashSpeed, fistDashThresh);
        }
        // Hitbox and special attack movement
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f); }
            else
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 15f,
                    ModPlayerFists.MovingInDash()); }
        }
        // Modify tooltip to replace tileboost with combo power
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
        #endregion
    }
}
