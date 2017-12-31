using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOut.Items.Weapons.Fists
{
    //[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class AdvancedCombo : ModItem
    {
        public override bool Autoload(ref string name) { return false; }
        public static int altEffect = 0; // ID for when this fist is using the alt effect

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Advanced Combo Knuckles");
            Tooltip.SetDefault(
                "<right> to parry, or consume combo to throw an cannonball\n" +
                "Combo grants 100% increased melee damage\n" + 
                "'Minor parry ability, coupled with projectile reflecting combo power'");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 80; // Base damage should be double of equivalent tier melee weapons
            item.useAnimation = 18; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 5f;
            item.tileBoost = 10; // Combo Power

            item.value = Item.sellPrice(0, 10, 0, 0);
            item.rare = 8; // >= 4, can use second uppercut
            item.useTime = item.useAnimation * 2;
            item.shoot = ProjectileID.CannonballFriendly;
            item.shootSpeed = 10 + item.rare / 2; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20; // Actual punching hitbox
        const float fistDashSpeed = 8f; // Speed at start of dash
        const float fistDashThresh = 7f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 15f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = fistDashSpeed * 1.5f; // Dash speed using combo
        const float altDashThresh = fistDashThresh;
        const float altJumpVelo = 17.5f;
        const int comboDelay = 20;
        const int parryActive = 15;
        const int parryCooldown = 10;
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
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true; // Hardmode combos reset uppercut
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position); // Combo activation sound
            }
            // Charging (Advanced)
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize);
            player.GetModPlayer<PlayerFX>().reflectingProjectilesForce = true; // Reflect projectiles
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // =================== BEHAVIOURS =================== //

                // Charge effect
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 174, 0, 0, 100, default(Color), 1.4f)];
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
                player.itemTime = 0; // Fire projectile
                // =================== BEHAVIOURS =================== //

                

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            else
            {
                // =================== BEHAVIOURS =================== //

                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 174, 3, 3, 100, default(Color), 1f)];
                d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);
                d.noGravity = true;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect &&
                player.itemAnimation < player.itemAnimationMax) // Only fire when combo is in use
            {
                // =================== BEHAVIOURS =================== //

                player.velocity = new Vector2(-speedX, -speedY);
                NetMessage.SendData(MessageID.SyncPlayer, -1, player.whoAmI, null, player.whoAmI);

                damage *= 4;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
                return true;
            }
            return false;
        }
        
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                // =================== BEHAVIOURS =================== //

                damage *= 8;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }

            if (mpf.IsComboActiveItemOnHit) // If combo would be active when this hits
            {
                // =================== BEHAVIOURS =================== //

                damage *= 2;

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }




        #region Advanced Combo-Parry Base: CanUseItem, AltFunctionUse, UseItemHitbox, ModifyTooltips
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
            if (!player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect))
            {
                return player.GetModPlayer<ModPlayerFists>().
                    AltFunctionParry(player, parryActive, parryCooldown);
            }
            return true;
        }
        // Hitbox and special attack movement
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f); }
            else
            { ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 14f); }
        }
        // Modify tooltip to replace tileboost with combo power
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
        #endregion
    }
}
