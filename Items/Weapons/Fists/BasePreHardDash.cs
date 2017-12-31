using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOutExtension.FistWeapons
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class PreHardDash : ModItem
    {
        public override bool Autoload(ref string name) { return false; }
        public static int altEffect = 0; // ID for when this fist is using the alt effect

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Basic Dash Fist");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 10% increased melee damage\n" +
                "Combo restores some life after striking an enemy\n" +
                "'The simplest, yet most versatile fist type'");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 10; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 25; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 7f;
            item.tileBoost = 10; // Combo Power

            item.value = Item.sellPrice(0, 0, 1, 0);
            item.rare = 1; // < 4, cannot perform second uppercut
            item.shootSpeed = 10 + item.rare / 2; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22; // Actual punching hitbox
        const float fistDashSpeed = 6.5f; // Speed at start of dash
        const float fistDashThresh = 4f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 12.5f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const int altHitboxSize = fistHitboxSize;
        const float altDashSpeed = 13f; // Dash speed when dashing through enemies
        const float altDashThresh = 9f;
        const float altJumpVelo = fistJumpVelo;
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
            // Start of dash
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
            else
            {
                // =================== BEHAVIOURS =================== //

                // Make some smoke (feet when grounded, centre when in the air)
                for (int j = 0; j < 2; j++)
                {
                    Dust d;
                    if (player.velocity.Y == 0f)
                    {
                        float height = player.height - 4f;
                        if (player.gravDir < 0) height = 4f;
                        d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + height), player.width, 8, 31, 0f, 0f, 100, default(Color), 1.4f)];
                    }
                    else
                    {
                        d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 8f), player.width, 16, 31, 0f, 0f, 100, default(Color), 1.4f)];
                    }
                    d.velocity *= 0.1f;
                    d.scale *= 1f + (float)Main.rand.Next(20) * 0.01f;
                }

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

                damage = (int)(damage * 1.1f); // increases damage by 10%

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }


        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit) // If combo would be active when this hits
            {
                // =================== BEHAVIOURS =================== //

                if (!target.immortal)
                {
                    PlayerFX.LifeStealPlayer(player, 3, target.lifeMax, 1f); // WeaponOut inbuilt healing code
                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }




        #region Pre-Hardmode Dash Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
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