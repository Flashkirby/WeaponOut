using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn)]
    public class KnucklesShotty : ModItem
    {
        public static int altEffect = 0; // ID for when this fist is using the alt effect

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Shotgun Knuckleduster");
			DisplayName.AddTranslation(GameCulture.Russian, "Кастет-Дробовик");
            Tooltip.SetDefault(
                "<right> to shoot, or consume combo to blast away\n" +
                "Combo grants ammo restoration per strike");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы выстрелить или использовать комбо и выстрелить мощнее\n" +
				"Комбо: восстанавливает боеприпасы ударами");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults() {
            item.ranged = true;
            item.damage = 22;
            item.useAnimation = 35; // Reduced by 30-50% on hit, increasing DPS
            item.knockBack = 6f;
            item.tileBoost = 6; // Combo Power

            item.value = Item.sellPrice(0, 10, 0, 0);
            item.rare = 5;
            item.useTime = item.useAnimation;
            item.useAmmo = AmmoID.None;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 7f; // Default shoot speed in case it needs to fire projectiles

            item.UseSound = SoundID.Item36;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20; // Actual punching hitbox
        const float fistDashSpeed = 8f; // Speed at start of dash
        const float fistDashThresh = 7f; // Minimum speed of dash (before ending)
        const float fistJumpVelo = 13f; // http://rextester.com/OIY60171 for jump height in tiles
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = fistDashSpeed * 1.5f; // Dash speed using combo
        const float altDashThresh = fistDashThresh;
        const float altJumpVelo = 15.5f;
        const int comboDelay = 10;
        const int parryActive = 15;
        const int parryCooldown = 10;
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Shotgun, 1);
            recipe.AddIngredient(ItemID.SoulofMight, 20);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial) {
            if (initial) {
                item.useAmmo = AmmoID.None;
                player.itemAnimation = player.itemAnimationMax + comboDelay; // Set initial combo animation delay
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true; // Hardmode combos reset uppercut
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position); // Combo activation sound
            }

            // Charging (Hardmode)
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special

            if (player.itemAnimation > player.itemAnimationMax) {

                // Charge effect
                for (int i = 0; i < 2; i++) {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 174, 0, 0, 100, default(Color), 1.4f)];
                    d.position -= d.velocity * 10f;
                    d.velocity /= 2;
                    d.noGravity = true;
                }
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax) {

                // Higher pitch
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                item.useAmmo = AmmoID.Bullet;
                player.itemTime = 0; // Fire projectile
            }
            else {

                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 174, 3, 3, 100, default(Color), 1f)];
                d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);
                d.noGravity = true;
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if (player.itemAnimation >= player.itemAnimationMax) return false;

            // Normall shots
            if (player.altFunctionUse > 0) {
                for (int shot = 0; shot < 4; shot++) {
                    Projectile.NewProjectile(position, new Vector2(
                            speedX + 3f * Main.rand.NextFloatDirection(),
                            speedY + 3f * Main.rand.NextFloatDirection()),
                        type, damage, knockBack, player.whoAmI);
                }
            }

            // Cannonball
            if (player.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect) {

                damage *= 2;
                for (int shot = 0; shot < 6; shot++) {
                    Projectile.NewProjectile(position, new Vector2(
                            speedX + 1.5f * Main.rand.NextFloatDirection(),
                            speedY + 1.5f * Main.rand.NextFloatDirection()),
                        type, damage, knockBack, player.whoAmI);
                }

                player.velocity = new Vector2(-speedX, -speedY);
                NetMessage.SendData(MessageID.SyncPlayer, -1, player.whoAmI, null, player.whoAmI);

                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                type = ProjectileID.CannonballFriendly;
                damage *= 4;
                speedX *= 2f;
                speedY *= 2f;
                knockBack *= 2f;
                return true;
            }
            return false;
        }

        // "Melee" strikes have x6 damage -> 132 = 209dps v 20def
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
            damage *= 6; // Higher damage because it's a shotgun normally
        }
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit) {
            base.ModifyHitPvp(player, target, ref damage, ref crit);
            damage *= 6; // Higher damage because it's a shotgun normally
        }

        // Restore ammo on combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
            if (target.immortal) return;
            if (player.GetModPlayer<ModPlayerFists>().IsComboActiveItemOnHit) {

                foreach (Item i in player.inventory) {
                    if (i.stack <= 0) continue;
                    if (i.ammo == AmmoID.Bullet) {
                        if (i.stack < i.maxStack) {
                            i.stack++;
                            Main.PlaySound(SoundID.Grab);
                        }else {
                            player.QuickSpawnItem(i.type, 1);
                        }
                        return;
                    }
                }
                // Otherwise spawn a musket ball
                player.QuickSpawnItem(ItemID.MusketBall, 1);
            }
        }

        #region Hardmode Combo Base + Bullets
        //
        // ITEM CODE CHANGES - DON'T MESS UNLESS YOU KNOW WHAT YOU'RE DOIN
        //

        // Apply dash on punch
        public override bool CanUseItem(Player player) {
            if (player.altFunctionUse == 0) {
                item.useAmmo = AmmoID.None;
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }else {
                player.itemTime = 0; // Allow fire projectile
                item.useAmmo = AmmoID.Bullet;
            }
            return true;
        }
        // Combo activate
        public override bool AltFunctionUse(Player player) {
            player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect);
            return true;
        }
        // Hitbox and special attack movement
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
            if (!AltStats(player)) { ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f); }
            else { ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 14f); }
        }
        // Modify tooltip to replace tileboost with combo power
        public override void ModifyTooltips(List<TooltipLine> tooltips) { ModPlayerFists.ModifyTooltips(tooltips, item); }
        #endregion
    }
}
