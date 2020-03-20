using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesMithril : ModItem
    {
        public static int altEffect = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mythril Knuckleduster");
            DisplayName.AddTranslation(GameCulture.Chinese, "秘银指虎");
            DisplayName.AddTranslation(GameCulture.Russian, "Мифриловый Кастет");

            Tooltip.SetDefault(
                "<right> consumes combo to unleash spirit energy\n" +
                "Combo grant 50% increased melee damage");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键消耗连击能量以释放精神能量\n连击将增加50%近战伤害");
            Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы использовать комбо и выпустить духовную энергию\n" +
				"Комбо: +50% урон");

            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 55; //130dps v 20def (up to 217)
            item.useAnimation = 20; // 30%-50% reduction
            item.knockBack = 3f;
            item.tileBoost = 10; // Combo Power

            item.value = Item.sellPrice(0, 1, 3, 50); // half sword cost
            item.rare = 4;
            item.useTime = item.useAnimation * 2;
            item.shoot = ModContent.ProjectileType<Projectiles.SpiritBlast>();
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22;
        const float fistDashSpeed = 8f;
        const float fistDashThresh = 7f;
        const float fistJumpVelo = 14f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = 10f;
        const float altDashThresh = 8f;
        const float altJumpVelo = 16f;
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MythrilBar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 30;
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
            }
            // Charging
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 20, 45);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                for (int i = 0; i < 2; i++)
                {
                    // Charge effect
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, DustID.t_Martian, 0, 0, 100, default(Color), 1.4f)];
                    d.position -= d.velocity * 10f;
                    d.velocity /= 2;
                    d.noGravity = true;
                }
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                player.itemTime = 0;
            }
            else
            {
                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, DustID.t_Martian, 3, 3, 100, default(Color), 1f)];
                d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);
                d.noGravity = true;
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect &&
                player.itemAnimation < player.itemAnimationMax)
            {
                Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY) * 0.95f,
                    type, damage * 2, knockBack, player.whoAmI, 0f, 1f);
                Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY) * 0.95f,
                    type, damage * 2, knockBack, player.whoAmI, 0f, -1f);
                return true;
            }
            return false;
        }

        //Combo
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                damage *= 2;
            }
        }

        // Hit Impact Effect
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            // Impact
            Dust d;
            for (int i = 0; i < 1 + damage / 30; i++)
            {
                d = Main.dust[Dust.NewDust((player.Center + target.Center) / 2, 0, 0, 43, -target.velocity.X, target.velocity.Y, 0, default(Color), 0.1f)];
                d.fadeIn = 0.6f;
                d.velocity = (5f * d.velocity) + (5f * ModPlayerFists.GetFistVelocity(player));
            }
        }
        
        #region Hardmode Combo Base
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect);
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 14f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
