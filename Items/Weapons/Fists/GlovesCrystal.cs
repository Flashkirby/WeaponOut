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
    public class GlovesCrystal : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Gauntlet");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔晶手套");

            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike unleashes crystal beams that build combo\n" +
                "Combo causes damage to refract towards nearby enemies");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键躲避到来的伤害\n反击将释放可以造成连击的魔晶射线\n连击将向附近敌人释放折射攻击");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 71;
            item.useAnimation = 26; // 30%-50% reduction
            item.knockBack = 4f;
            item.tileBoost = 18; // Combo Power

            item.value = Item.sellPrice(0, 1, 50, 0);
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
        const float fistDashThresh = 6f;
        const float fistJumpVelo = 13.5f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 32;
        const float altDashSpeed = 14f;
        const float altDashThresh = 12f;
        const float altJumpVelo = 17.6f;
        const int parryActive = 25;
        const int parryCooldown = 15;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UnicornHorn, 2);
            recipe.AddIngredient(ItemID.CrystalShard, 30);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        // Parry & Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();

                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == target.whoAmI) continue;
                    if (npc.Distance(player.Center) < 256)
                    {
                        if(player.CanHit(npc))
                        {
                            Projectile.NewProjectile(npc.Center, new Vector2(-player.direction),
                                mod.ProjectileType<Projectiles.SpiritBeam>(), (int)(damage * 0.33f), 0f, player.whoAmI);
                        }
                    }
                }
            }
            if (mpf.IsComboActiveItemOnHit)
            {
                float distance = 128;
                NPC nextTarget = null;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == target.whoAmI) continue;
                    float ndist = npc.Distance(player.Center);
                    if (ndist < distance)
                    {
                        nextTarget = npc;
                        distance = ndist;
                    }
                }
                if (nextTarget != null)
                {
                    Projectile.NewProjectile(nextTarget.Center, new Vector2(-player.direction),
                        mod.ProjectileType<Projectiles.SpiritBeam>(), damage, knockBack, player.whoAmI);
                }
            }
        }

        #region Hardmode Parry Base
        public override bool CanUseItem(Player player)
        {
            if (!AltStats(player))
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParryDash(player, parryActive, parryCooldown, fistJumpVelo - fistJumpVelo / 4, fistDashSpeed / 2f, fistDashThresh / 2);
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
