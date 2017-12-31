using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOutExtension.FistWeapons
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class HardmodeDash : ModItem
    {
        public override bool Autoload(ref string name) { return false; }
        public static int altEffect = 0; // ID for when this fist is using the alt effect

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hardmode Dash Fist");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 50% increased melee damage\n" +
                "Combo grants 20% increased uppercut damage\n" +
                "'Standard dash fist, with upgraded hardmode movement bonuses'");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 40; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 25; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 7f;
            item.tileBoost = 10; // Combo Power

            item.value = Item.sellPrice(0, 0, 1, 0);
            item.rare = 4; // >= 4, can use second uppercut
            item.shootSpeed = 10 + item.rare / 2; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item20;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22; // Actual punching hitbox
        const float fistDashSpeed = 8f; // Speed at start of dash
        const float fistDashThresh = 6f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 14f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const int altHitboxSize = fistHitboxSize;
        const float altDashSpeed = 14f; // Dash speed when dashing through enemies
        const float altDashThresh = 11f;
        const float altJumpVelo = 16f;
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
            // =================== BEHAVIOURS =================== //

            // Body fire
            for (int i = 0; i < 3; i++)
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                    DustID.Fire, 0, 0, 100, default(Color), 1.8f)];
                d.velocity = d.velocity * 0.5f + player.velocity * -0.4f;
                d.noGravity = true;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }

            // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
        }


        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            // Dashing Bonus
            if (AltStats(player)) // If dashing
            {
                // =================== BEHAVIOURS =================== //

                damage = (int)(damage * 1.5f); // increases damage by 50%

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }

            // Combo Bonus
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit && mpf.specialMove == 1)
            {
                // =================== BEHAVIOURS =================== //

                damage = (int)(damage * 1.2f); // increases damage by 20%

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }




        #region Hardmode Dash Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
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
            if (player.dashDelay == 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                    SetDash(altDashSpeed, altDashThresh, 0.992f, 0.96f, false, altEffect);
                return true;
            }
            return false;
        }
        // Hitbox and special attack movement
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
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
