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
    public class FistsAdamant : ModItem
    {
        public static int altEffect = 0;
        public static int projID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Adamantite Fist");
            DisplayName.AddTranslation(GameCulture.Chinese, "精金拳套");
			DisplayName.AddTranslation(GameCulture.Russian, "Адамантитовый Кулак");

            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 50% increased melee damage and knockback\n" +
                "Combo grants destructive punches");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键向敌人冲刺\n冲刺将增加50%的近战伤害和击退\n连击将带来破坏性的打击");
			Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы прорваться через врагов\n" +
                "Рывок: +50% урон и отбрасывание\n" +
                "Комбо: разрушительные удары");

            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            projID = ModContent.ProjectileType<Projectiles.SpiritExplosion>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 107; //200dps v 20def
            item.useAnimation = 29; // 30%-50% reduction
            item.knockBack = 9f;
            item.tileBoost = 9; // Combo Power

            item.value = Item.sellPrice(0, 1, 38, 0);
            item.rare = 4;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item18;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 28;
        const float fistDashSpeed = 8f;
        const float fistDashThresh = 7f;
        const float fistJumpVelo = 14.4f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 16.5f;
        const float altDashThresh = 16f;
        const float altJumpVelo = 16.85f;
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.AdamantiteBar, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 5; i++)
            {
                Dust d = Main.dust[Dust.NewDust(player.Center - new Vector2(8, 16), 8, 32, 
                    31, -player.velocity.X, -player.velocity.Y, 0, new Color(1f, i * 0.2f, i * 0.2f), 0.8f)];
                d.fadeIn = 1.1f + 0.1f * i;
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }

        // Dash
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                damage += 12;
                knockBack += 2f;
            }
            if (AltStats(player))
            {
                damage = (int)(damage * 1.5);
                knockBack *= 1.5f;
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {

            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                if(!mpf.IsComboActive) Main.PlaySound(SoundID.Item21, player.Center);

                Vector2 center = player.Center;
                center += 1.5f * (target.Center - player.Center);
                Projectile.NewProjectile(center, new Vector2(player.direction, player.gravDir), projID, 75, 11f, player.whoAmI);
            }
        }

        // Idle Effect
        private float soundDelay;
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                soundDelay--;
                if (soundDelay <= 0)
                {
                    Main.PlaySound(SoundID.Item22.WithVolume(0.3f), player.Center);
                    soundDelay = 25;
                }
                Rectangle r = ModPlayerFists.GetPlayerOnHandRectangle(player, 4);

                Dust d = Dust.NewDustDirect(r.TopLeft(), r.Width, r.Height, DustID.Smoke);

                float dir = Main.time % 2 == 0 ? -1f : 1f;
                d.position.X += dir * 4;
                d.velocity = new Vector2(dir, -1f * player.gravDir);
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cHandOn, player);
            }
        }

        #region Hardmode Dash Base
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
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
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 16f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, altJumpVelo, 0.5f, 16f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
