﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesFossil : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.EnableFists;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fossil Glove");
			DisplayName.AddTranslation(GameCulture.Russian, "Ископаемая Перчатка");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike increases armor penetration by 20\n" +
                "Combo deals bonus damage based on combo power");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы парировать удар\n" +
				"Контратака: +20 бронепробиваемости\n" +
				"Комбо: выше заряд - больше урон");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 22;
            item.useAnimation = 18; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 6f;
            item.tileBoost = 8; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 25, 0);
            item.rare = 2;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 26;
            item.height = 26;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FossilOre, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                float dashSpeed = 5f;
                if (mpf.parryBuff)
                {
                    dashSpeed = 8f;
                }
                mpf.SetDashOnMovement(dashSpeed, dashSpeed - 0.5f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParry(player, 22, 18);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 20, 9f, 0.5f);
        }

        // Parry
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.parryBuff)
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                knockBack += 2f;
                player.armorPenetration += 20;
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                // deal 0.5 true damage
                target.lifeRegenCount -= 60 * mpf.ComboCounter;
            }
        }
    }
}