using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesWooden : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }
		
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wooden Tekko");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Parry grants temporary invulnerability\n" +
                "Combo grants 3 bonus damage");
        }
        public override void SetDefaults()
        {
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.useAnimation = 24; // Combos can increase speed by 30-50% since it halves remaining attack time

            item.width = 20;
            item.height = 20;
            item.damage = 5;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item7;

            item.tileBoost = 5; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 0, 50);
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 3);
            recipe.anyWood = true;
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
                SetDashOnMovement(3f, 12f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParry(player, 25, 25);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 26, 9f, 1f);
        }

		// Parry
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.parryBuff)
            {
				if(mpf.GetParryBuff() >= 0) player.buffTime[mpf.GetParryBuff()] -= 5;
                player.immuneTime++;
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
            if (mpf.IsComboActiveItemOnHit)
            {
                damage += 3;
            }
        }
    }
}
