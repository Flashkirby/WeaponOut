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
    public class GlovesButterfly : ModItem
    {
        public static int buffID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Butterfly's Grace");
			DisplayName.AddTranslation(GameCulture.Russian, "Грация Бабочки");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike grants 500% increased divekick damage\n" +
                "Combo grants increased aerial movement\n" +
                "'...Sting like a bee'");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы парировать удар\n" +
				"Контратака: +500% урон в падении\n" +
				"Комбо: улучшает управление в воздухе\n" +
				"'...Жаль, как пчела'");
            buffID = mod.BuffType<Buffs.ButterflyGrace>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 110;
            item.useAnimation = 25; // 30%-50% reduction
            item.knockBack = 4f;
            item.tileBoost = 5; // Combo Power

            item.value = Item.sellPrice(0, 1, 50, 0);
            item.rare = 7;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item18;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 28;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 7f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 28;
        const float altDashSpeed = 15f;
        const float altDashThresh = 13f;
        const float altJumpVelo = 17.6f;
        const int parryActive = 22;
        const int parryCooldown = 18;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ButterflyDust, 1);
            recipe.AddIngredient(ItemID.SoulofFright, 10);
            recipe.AddIngredient(ItemID.SoulofMight, 5);
            recipe.AddIngredient(ItemID.SoulofSight, 5);
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
            if (AltStats(player) && mpf.specialMove == 2)
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                // Increase by item damage, not buffed total damage
                damage += (int)(player.HeldItem.damage * 5f);
            }
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                if (player.velocity.Y != 0) player.AddBuff(buffID, 60);
            }
        }

        #region Hardmode Parry Base
        public override bool CanUseItem(Player player)
        {
            if (AltStats(player) || player.FindBuffIndex(buffID) >= 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
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
            if (AltStats(player) || player.FindBuffIndex(buffID) >= 0)
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 15f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
