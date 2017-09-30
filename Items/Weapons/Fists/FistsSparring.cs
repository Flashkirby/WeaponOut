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
    public class FistsSparring : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sparring Mitt");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Consumes combo to increase melee damage by 100%");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 42;
            item.useAnimation = 28; // 30%-50% reduction
            item.knockBack = 5f;
            item.tileBoost = 4; // Combo Power

            item.value = Item.sellPrice(0, 0, 50, 0);
            item.rare = 4;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 18;
        const float fistDashSpeed = 8f;
        const float fistDashThresh = 6f;
        const float fistJumpVelo = 15.2f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().IsComboActive; }
        const float altDashSpeed = 20f;
        const float altDashThresh = 6f;
        const float altJumpVelo = 17.5f;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 4);
            recipe.AddIngredient(ItemID.SoulofLight, 6);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0)
            {
                Gore g;
                if (player.velocity.Y == 0f)
                {
                    g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f)];
                }
                else
                {
                    g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 14f), default(Vector2), Main.rand.Next(61, 64), 1f)];
                }
                g.velocity.X = (float)Main.rand.Next(-50, 51) * 0.01f;
                g.velocity.Y = (float)Main.rand.Next(-50, 51) * 0.01f;
                g.velocity *= 0.4f;
            }
            else
            {
                for (int j = 0; j < 3; j++)
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
                    d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                }
            }
        }

        // Combo
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                mpf.ConsumeCombo(player);
                damage *= 2;
                knockBack *= 1.2f;
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
                    SetDash(altDashSpeed, altDashThresh, 0.96f, 0.96f, false, altEffect);
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
