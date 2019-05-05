using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    /// <summary>
    /// This may or may not be a joke item
    /// </summary>
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)] // Uncomment if you have hand sprites
    public class FistsHokuto : ModItem
    {
        public static int dashEffect = 0; // ID for when this fist is dashing
        public static int altEffect = 0; // ID for when this fist is using combo power
        public static int buffID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Polaris Punch");
            DisplayName.AddTranslation(GameCulture.Russian, "Полярный Удар");

            Tooltip.SetDefault(
                "<right> to dash, or consume combo to unleash a deadly combo\n" +
                "Dash for a projectile deflecting punch\n" +
                "Increases length of combo by 5 second whilst held\n" + 
                "'Channel the power of the constellations'");
            Tooltip.AddTranslation(GameCulture.Russian,
                "<right> для рывка, или использовать заряд комбо для смертельного комбо\n" +
                "Рывок отталкивает снаряды\n" +
                "Увеличивает длительность комбо на 5 секунд, когда в руке\n" +
                "'Вся мощь созвездий'");

            dashEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            buffID = mod.BuffType<Buffs.Flurry>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 290; // 795dps (vs 50 def)
            item.useAnimation = 20; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 7f;
            item.tileBoost = 100; // Combo Power

            item.value = Item.sellPrice(0, 0, 5, 0);
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
        const int altHitboxSize = (int)(fistHitboxSize * 5f);
        const float altDashSpeed = 17f; // Dash speed when dashing through enemies
        const float altDashThresh = 10f;
        const float altJumpVelo = 17f;
        const int comboDelay = 80;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;

            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddIngredient(ItemID.FragmentNebula, 10);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a dash. Use for ongoing effects like dust. </summary>
        public static void DashEffects(Player player, Item item)
        {

            if (player.dashDelay == 0) { }
            // =================== BEHAVIOURS =================== //

            for (int i = 0; i < 3; i++) // Fire!
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 173,
                    -player.velocity.X, 0, 100, default(Color), 2 + i * 0.15f)];
                d.noGravity = true;
                d.velocity.Y = player.velocity.Y * -0.5f;
                d.velocity *= 0.7f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);

                d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 180,
                   -player.velocity.X, 0, 100, default(Color), 2 + i * 0.15f)];
                d.noGravity = true;
                d.velocity.Y = player.velocity.Y * -0.5f;
                d.velocity *= 0.7f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }

            player.GetModPlayer<PlayerFX>().reflectingProjectilesForce = true;
            player.GetModPlayer<PlayerFX>().reflectingProjectilesParryStyle = true;

            // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
        }
        const int flurryDuration = 150;
        const int flurryEndDelay = 60;
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial) {

                player.AddBuff(buffID, flurryDuration + flurryEndDelay + comboDelay, false); // Flurry
                player.itemAnimation = player.itemAnimationMax + comboDelay; // Set initial combo animation delay
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true; // Hardmode combos reset uppercut
                Main.PlaySound(WeaponOut.mod.GetLegacySoundSlot
                    (SoundType.Item, "Sounds/Item/HokutoActivate").WithPitchVariance(0f), 
                    player.position);
            }
            // Charging (Hardmode)
            int bIdx = player.FindBuffIndex(buffID);
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // =================== BEHAVIOURS =================== //

                // Charge effect
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 175, 0, 0, 0, default(Color), 1.5f)];
                    d.position -= d.velocity * 10f;
                    d.velocity /= 2;
                    d.noGravity = true;
                }

                // Final punch effect
                if(bIdx >= 0 && player.buffTime[bIdx] <= flurryEndDelay) {

                    player.velocity.Y -= player.gravity;
                    // Move towards mouse
                    if (player.whoAmI == Main.myPlayer && player.buffTime[bIdx] % 3 == 0) {

                        Vector2 velo = Main.MouseWorld - player.Center;
                        velo.Normalize();
                        velo *= altDashSpeed * 2f;
                        player.velocity = (player.velocity * 3 + velo) / 4;
                        NetMessage.SendData(MessageID.SyncPlayer, -1, player.whoAmI, null, player.whoAmI);
                    }
                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                if (bIdx >= 0 && player.buffTime[bIdx] > flurryEndDelay) {

                    // Higher pitch for initiate
                    Main.PlaySound(WeaponOut.mod.GetLegacySoundSlot
                        (SoundType.Item, "Sounds/Item/HokutoFlurry").WithPitchVariance(0f));
                } else {

                    // Boost otherwise
                    Vector2 velo = Main.MouseWorld - player.Center;
                    velo.Normalize();
                    velo *= altDashSpeed;
                    player.velocity = velo;
                    NetMessage.SendData(MessageID.SyncPlayer, -1, player.whoAmI, null, player.whoAmI);
                }

                // =================== BEHAVIOURS =================== //

                // Swag dust ring
                for (int i = 0; i < 64; i++)
                {
                    double angle = Main.time + i / 10.0;
                    Dust d = Dust.NewDustPerfect(player.Center, i % 2 == 0 ? 173 : 162,
                        new Vector2((float)(5.0 * Math.Sin(angle)), (float)(5.0 * Math.Cos(angle))));
                    d.noGravity = true;
                    d.fadeIn = 1.3f;
                    d.velocity *= 2f;
                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
            else
            {
                // =================== BEHAVIOURS =================== //

                // Reset
                bool bigPunch = false;
                if (bIdx >= 0) {

                    if (player.buffTime[bIdx] > flurryEndDelay) {
                        if (player.itemAnimation <= player.itemAnimationMax / 2) {

                            // Reset in flurry
                            player.itemAnimation = player.itemAnimationMax;
                            ModPlayerFists.Get(player).specialMove = 0;

                            // Move towards mouse
                            if (player.whoAmI == Main.myPlayer) {

                                Vector2 velo = Main.MouseWorld - player.Center;
                                velo.Normalize();
                                velo *= altDashSpeed;
                                player.velocity = (player.velocity * 2 + velo) / 3;
                                NetMessage.SendData(MessageID.SyncPlayer, -1, player.whoAmI, null, player.whoAmI);
                            }
                        }
                    }else {

                        if(player.buffTime[bIdx] == flurryEndDelay) {
                            // Higher pitch for initiate
                            Main.PlaySound(WeaponOut.mod.GetLegacySoundSlot
                                (SoundType.Item, "Sounds/Item/HokutoEnd").WithPitchVariance(0f));
                        }

                        // Last punch, double hitbox
                        player.itemAnimation = player.buffTime[bIdx] + 1;
                        r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize * 2);
                        bigPunch = true;
                    }

                }
                
                Vector2 pVelo = (player.position - player.oldPosition);
                Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
                Dust d;

                for (int i = 0; i < 8; i++) {
                    d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 173,
                        velocity.X, velocity.Y)];
                    d.velocity *= bigPunch ? 3f : 2f;
                    d.noGravity = true;
                    d.scale *= bigPunch ? 4f : 1.5f;
                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }


        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit, target.statLife, target.Center, target.velocity); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit, target.life, target.Center, target.velocity); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit, int targetLife, Vector2 targetCentre, Vector2 targetVelocity)
        {
            // Dashing Bonus
            if (DashStats(player)) // If dashing
            {
                // =================== BEHAVIOURS =================== //
                
                knockBack = 0f; // No knockback

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }

            // Combo Bonus
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (ComboStats(player))
            {
                // =================== BEHAVIOURS =================== //

                int bIdx = player.FindBuffIndex(buffID);
                if (bIdx >= 0) {

                    // And back
                    Vector2 velo = player.Center - targetCentre;
                    velo.Normalize();
                    velo *= altDashSpeed;
                    velo += targetVelocity;

                    if (player.buffTime[bIdx] > flurryEndDelay) {

                        damage = (int)(damage * 0.9f); // flurry attack
                        knockBack = 0f; // no knockback
                        ModPlayerFists.Get(player).ModifyComboCounter(0, true);

                        player.velocity = (player.velocity * 2 + velo) / 3;
                    }
                    else {

                        damage = 1; // little damage
                        knockBack = 0f; // no knockback
                        ModPlayerFists.Get(player).ModifyComboCounter(1, true);

                        player.velocity = velo;
                    }

                    // And back
                    NetMessage.SendData(MessageID.SyncPlayer, -1, player.whoAmI, null, player.whoAmI);

                }

                // ^^^^^^^^^^^^^^^^^^^ BEHAVIOURS ^^^^^^^^^^^^^^^^^^^ //
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {

            if (ComboStats(player)) {

                int bIdx = player.FindBuffIndex(buffID);
                if (bIdx >= 0) {

                    if (player.buffTime[bIdx] > flurryEndDelay) {

                        // Slowdown target
                        if (target.velocity.Equals(default(Vector2))) return;
                        target.velocity /= 2;
                        target.netUpdate = true;
                    }
                    else {

                        // Final hit
                        PlayerFX pfx = player.GetModPlayer<PlayerFX>();
                        if (pfx.omHaMoShin == null) {
                            pfx.omHaMoShin = target;
                            pfx.omHaMoShindearuTimer = PlayerFX.omHaMoShindearuTimerMax;
                        }
                        else if (pfx.omHaMoShin.life < target.life) {
                            pfx.omHaMoShin = target;
                            pfx.omHaMoShindearuTimer = PlayerFX.omHaMoShindearuTimerMax;
                        }
                    }
                }
            }
        }


        // Combo duration extender
        public override void UpdateInventory(Player player) {            if (player.HeldItem != item) return;

            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.comboResetTimeBonus += 300;
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox) 
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 4, fistHitboxSize);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            for (int i = 0; i < 6; i++) {
                d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 173,
                    velocity.X, velocity.Y)];
                d.velocity *= 1f;
                d.noGravity = true;
                d.scale *= 1.2f;
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
            if (!ComboStats(player)) {
                if (DashStats(player)) {
                    ModPlayerFists.UseItemHitbox(player, ref hitbox, 
                        fistHitboxSize, altJumpVelo, 0.5f, 16f);
                }
                else {
                    ModPlayerFists.UseItemHitbox(player, ref hitbox, 
                        fistHitboxSize, fistJumpVelo, 0.5f, 16f);
                }
            }
            else {
                int bIdx = player.FindBuffIndex(buffID);

                if (bIdx >= 0 && player.buffTime[bIdx] > flurryEndDelay) {
                    ModPlayerFists.UseItemHitbox(player, ref hitbox, 
                        altHitboxSize, altJumpVelo, 0.5f, altDashSpeed);
                }
                else {
                    ModPlayerFists.UseItemHitbox(player, ref hitbox, 
                        altHitboxSize * 2, altJumpVelo, 0.5f, altDashSpeed);
                }
            }
        }
        // Modify tooltip to replace tileboost with combo power
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
        #endregion
    }
}
