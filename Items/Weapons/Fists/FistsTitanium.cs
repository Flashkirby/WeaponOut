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
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsTitanium : ModItem
    {
        public static int altEffect = 0;
        public static int shadowID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Titanium Fist");
            DisplayName.AddTranslation(GameCulture.Chinese, "钛金钢拳");

            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 30% increased melee damage and knockback\n" +
                "Combo grants shadow clones");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键向敌人冲刺\n冲刺将增加30%的近战伤害和击退\n连击将给予影子克隆体");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            shadowID = mod.ProjectileType<Projectiles.SpiritShadow>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 92; //205dps v 20def
            item.useAnimation = 24; // 30%-50% reduction
            item.knockBack = 7f;
            item.tileBoost = 15; // Combo Power

            item.value = Item.sellPrice(0, 1, 61, 0);
            item.rare = 4;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 26;
        const float fistDashSpeed = 8.5f;
        const float fistDashThresh = 7f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 15f;
        const float altDashThresh = 16f;
        const float altJumpVelo = 16.85f;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.TitaniumBar, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 3; i++)
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 111, -player.velocity.X, -player.velocity.Y, 0, default(Color), 0.8f)];
                d.velocity *= 0.01f * Main.rand.Next(-20, 21);
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
            if (AltStats(player))
            {
                damage = (int)(damage * 1.3);
                knockBack *= 1.3f;
            }
        }
        
        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                bool noShadow = player.whoAmI == Main.myPlayer;
                foreach (Projectile p in Main.projectile)
                {
                    if (p.active && p.owner == Main.myPlayer && p.type == shadowID)
                    { noShadow = false; break; }
                }
                if (noShadow)
                {
                    Projectile.NewProjectile(player.Center, new Vector2(), shadowID, item.damage, item.knockBack, Main.myPlayer, 1f);
                    Projectile.NewProjectile(player.Center, new Vector2(), shadowID, item.damage, item.knockBack, Main.myPlayer, -1f);
                }
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
