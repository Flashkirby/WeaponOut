using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsForbidden : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Forbidden Gauntlet");
            Tooltip.SetDefault(
                "<right> to transform into a raging sandstorm\n" +
                "Dash grants invincibility and steals life\n" +
                "Combo causes attacks to wear away at enemies\n" +
                "'Forbidden techniques in the palm of your hand'");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 101;
            item.useAnimation = 30; // 30%-50% reduction
            item.knockBack = 6f;
            item.tileBoost = 7; // Combo Power

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 5;

            item.UseSound = SoundID.Item18;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 12f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 15f;
        const float altDashThresh = 10f;
        const float altJumpVelo = 16.85f;
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.AncientBattleArmorMaterial, 2);
                if (i == 0)
                { recipe.AddIngredient(ItemID.AdamantiteBar, 5); }
                else
                { recipe.AddIngredient(ItemID.TitaniumBar, 5); }
                recipe.AddTile(TileID.MythrilAnvil);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            if (player.velocity.Y != 0)
            {
                player.velocity.Y -= (player.gravity * player.gravDir) / 2;
            }

            if (player.dashDelay == 0)
            {
                Main.PlaySound(SoundID.DD2_FlameburstTowerShot, player.position);
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 1);
                player.immuneAlpha = 255 + 50 * 20;
            }
            else
            {

                Vector2 bigPos = player.position - new Vector2(8, 8);
                for (int i = 0; i < 5; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(bigPos, player.width + 16, player.height + 16, 32, 0f, 0f, 140)];
                    d.velocity *= -1f;
                    d.velocity.X += player.velocity.X * 4f;
                    d.velocity.Y += player.velocity.Y * 2f;
                    d.position -= d.velocity * 4f;
                    d.scale = 1f;
                    d.fadeIn = 1.6f;
                    d.noGravity = true;
                    d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);

                    d = Main.dust[Dust.NewDust(bigPos, player.width + 16, player.height + 16, 32, player.velocity.X, player.velocity.Y, 50, default(Color), 2f)];
                    d.position += d.velocity;
                    d.noGravity = true;
                    d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);

                    if (i < 2)
                    {
                        d = Main.dust[Dust.NewDust(bigPos, player.width + 16, player.height + 16, DustID.Sandnado, 0f, 0f, 120)];
                        d.velocity += player.velocity / 10;
                        d.position += d.velocity * 5f;
                        d.fadeIn = 0.7f;
                        d.scale = 0.2f;
                        d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                    }
                }
            }
            player.immuneTime++;
            player.immuneNoBlink = false;

        }

        // Dash & Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (AltStats(player) && !target.immortal)
            {
                PlayerFX.LifeStealPlayer(player, damage, target.lifeMax, 1f / 40f);
            }
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                target.AddBuff(mod.BuffType<Buffs.ErodingWind>(), 600, false);
            }
        }
        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                target.AddBuff(mod.BuffType<Buffs.ErodingWind>(), 600, false);
            }
        }
        
        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, 26);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            Vector2 pos = r.TopLeft();
            for (int i = 0; i < 2; i++)
            {
                d = Main.dust[Dust.NewDust(pos, r.Width, r.Height, DustID.Sandnado,
                    velocity.X * -1.5f, velocity.Y * -1.5f, 100, default(Color), 0.2f)];
                d.noGravity = true;
                d.fadeIn = 0.4f;
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
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 1f, 16f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, altJumpVelo, 1f, 16f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
