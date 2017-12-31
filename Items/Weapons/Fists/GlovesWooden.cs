using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesWooden : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.EnableFists;
        }
		
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wooden Tekko");
            DisplayName.AddTranslation(GameCulture.Chinese, "木手甲");

            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Parry grants temporary invincibility\n" +
                "Combo grants 3 bonus damage");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键躲避到来的伤害\n躲避成功后暂时无敌\n连击将奖励增加3点伤害");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 8;
            item.useAnimation = 24; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 2.5f;
            item.tileBoost = 5; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 0, 50);

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
            item.shootSpeed = 10 + item.rare / 2;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
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
                SetDashOnMovement(4f, 3f, 0.992f, 0.96f, true, 0);
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
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 26, 9f, 0.5f);
        }

		// Parry
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.parryBuff)
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                player.immuneTime += 60;
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
