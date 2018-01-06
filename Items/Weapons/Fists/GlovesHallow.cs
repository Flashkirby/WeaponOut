﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesHallow : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hallowed Vambrace");
			DisplayName.AddTranslation(GameCulture.Russian, "Святая Перчатка");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike grants 400% increased melee damage\n" +
                "Combo grants increased defensive capabilities");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы парировать удар\n" +
				"Контратака: +400% урон\n" +
				"Комбо: увеличивает защиту");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 117; //229dps v 20def
            item.useAnimation = 28; // 30%-50% reduction
            item.knockBack = 7f;
            item.tileBoost = 9; // Combo Power

            item.value = Item.sellPrice(0, 2, 20, 0);
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
        const float fistDashThresh = 12f;
        const float fistJumpVelo = 14.4f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 28;
        const float altDashSpeed = 14f;
        const float altDashThresh = 12f;
        const float altJumpVelo = 17.6f;
        const int parryActive = 25;
        const int parryCooldown = 15;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HallowedBar, 6);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        // Parry
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                // Increase by item damage, not buffed total damage
                damage += (int)(player.HeldItem.damage * 4f);
            }
        }

        // Combo
        public override void UpdateInventory(Player player) {            if (player.HeldItem != item) return;
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
			// TODO: change to somewhere else because holditem is too late in the procedure to effect damage
            if (mpf.IsComboActive)
            {
                player.endurance += 0.04f;
                player.statDefense += 7;
                player.longInvince = true;
                player.noKnockback = true;
            }
        }

        #region Hardmode Parrydash Base
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
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 14f,
                    ModPlayerFists.MovingInDash());
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
