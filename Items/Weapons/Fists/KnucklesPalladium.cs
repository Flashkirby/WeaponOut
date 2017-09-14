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
    public class KnucklesPalladium : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;
        public static int buffID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palladium Katar");
            Tooltip.SetDefault(
                "<right> consumes combo for a life-empowered strike\n" +
                "Combo grants increased life regeneration");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            buffID = mod.BuffType<Buffs.DemonFrenzy>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 63;
            item.useAnimation = 24; // 30%-50% reduction
            item.knockBack = 4f;
            item.tileBoost = 6; // Combo Power

            item.value = Item.sellPrice(0, 0, 92, 0);
            item.rare = 4;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 28;
        const float fistDashSpeed = 8f;
        const float fistDashThresh = 12f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = 14f;
        const float altDashThresh = 12f;
        const float altJumpVelo = 16.85f;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PalladiumBar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                player.lifeRegenCount += 8; // Regen 4 health a second
            }
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 18;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
            }

            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 10, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charging
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 170, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 20f;
                d.velocity *= 1.5f;
                d.velocity += player.position - player.oldPosition;
                d.noGravity = true;
            }
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                // Allow dash
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                // Punch effect
                Dust d;
                for (int i = 0; i < 3; i++)
                {
                    d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 232, 3, 3, 100, default(Color), 1f)];
                    d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);
                    d.noGravity = true;
                }
            }
        }
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(player.position, player.width, player.height, 27, player.velocity.X * -0.5f, player.velocity.Y * -0.5f, 180);
                Main.dust[d].noGravity = true;
                Main.dust[d].shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }


        //Combo
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                damage += player.statLifeMax2 / 2 + player.statLife / 2;
            }
        }

        // Hit Impact Effect
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            // Impact
            Dust d;
            for (int i = 0; i < 1 + damage / 20; i++)
            {
                d = Main.dust[Dust.NewDust((player.Center + target.Center) / 2, 0, 0, 159, -target.velocity.X, target.velocity.Y, 100, default(Color), 0.5f)];
                d.velocity = (3f * d.velocity) + (3f * ModPlayerFists.GetFistVelocity(player));
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
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 3f, 14f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 3f, 14f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
