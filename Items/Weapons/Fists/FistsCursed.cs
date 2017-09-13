using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn)]
    public class FistsCursed : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cursed Gadling");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 50% increased melee damage and knockback\n" +
                "Combo causes enemies to take more damage from fire");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 94;
            item.useAnimation = 26; // 30%-50% reduction
            item.knockBack = 6f;
            item.tileBoost = 9; // Combo Power

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 5;

            item.UseSound = SoundID.Item20;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 26;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 12f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 18f;
        const float altDashThresh = 16f;
        const float altJumpVelo = 16.85f;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CursedFlame, 10);
            recipe.AddIngredient(ItemID.RottenChunk, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 5; i++)
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 75, -player.velocity.X, 0, 100, default(Color), 2 + i * 0.15f)];
                d.noGravity = true;
                d.velocity.Y = player.velocity.Y * -0.5f;
                d.velocity *= 0.7f;
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
                damage = (int)(damage * 1.5);
                knockBack *= 1.5f;
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (Main.rand.Next(3) == 0)
            {
                target.AddBuff(BuffID.CursedInferno, 420);
            }
            if (mpf.IsComboActiveItemOnHit)
            {
                target.AddBuff(BuffID.Oiled, 420);
            }
        }


        // Idle Effect
        public override void HoldItem(Player player)
        {
            
            if (Main.time % 3 == 0)
            {
                Rectangle r = ModPlayerFists.GetPlayerOnHandRectangle(player, 10);
                
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 75,
                    (float)(player.direction * 2), 0f, 100, default(Color), 0.5f)];
                d.position += player.position - player.oldPosition;
                d.velocity = Utils.RandomVector2(Main.rand, -0.5f, 0.5f) + player.velocity * 0.5f - new Vector2(0, player.gravDir);
                d.noGravity = true;
                d.fadeIn = 0.85f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cHandOn, player);
            }
        }
        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 12, 26);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            Vector2 pos = r.TopLeft();
            for (int i = 0; i < 4; i++)
            {
                d = Main.dust[Dust.NewDust(pos, r.Width, r.Height, 75,
                    velocity.X, velocity.Y, 100, default(Color), 1f+ player.itemAnimation * 0.05f)];
                d.velocity *= 2f;
                d.noGravity = true;
                pos -= pVelo / 5; // trail better at high speeds
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
