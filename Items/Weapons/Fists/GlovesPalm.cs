﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesPalm : ModItem
    {
		
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palm Striker");
			DisplayName.AddTranslation(GameCulture.Russian, "Пальмовая Перчатка");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike grants 15 bonus damage\n" +
                "Combo grants 2 bonus damage");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы парировать удар\n" +
				"Контратака: \n" +
				"Комбо: ");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 12;
            item.useAnimation = 25; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 4f;
            item.tileBoost = 6; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 0, 60);

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
            item.shootSpeed = 10 + item.rare / 2;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PalmWood, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(5f, 3f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParry(player, 20, 30);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 26, 9f, 0.5f);
        }

        // Parry & Combo
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                damage += 2;
            }
            if (mpf.parryBuff)
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                damage += 15;
            }
        }
    }
}
