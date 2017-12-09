using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesObsidian : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Obsidian Vambrace");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike grants 50 bonus damage and increased knockback\n" +
                "Combo grants greatly increased damage whilst on fire");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 30;
            item.useAnimation = 28; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 4.5f;
            item.tileBoost = 6; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 10, 0);
            item.rare = 1;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 26;
            item.height = 26;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Obsidian, 20);
            recipe.AddTile(TileID.Furnaces);
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
                AltFunctionParry(player, 15, 25);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 20, 9f, 0.5f);
        }

        // Parry && Combo
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
                damage += 50;
                knockBack += 5.5f;
            }
        }
    }
}