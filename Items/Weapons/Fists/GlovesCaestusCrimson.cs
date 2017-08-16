using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesCaestusCrimson : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Tenderizer");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Parry heals 5 life on next hit\n" +
                "Combo grants increased life regeneration");
        }
        public override void SetDefaults()
        {
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.useAnimation = 30; // Combos can increase speed by 30-50% since it halves remaining attack time

            item.width = 20;
            item.height = 20;
            item.damage = 24;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item7;

            item.tileBoost = 6; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 1, 0); // actually twice material value
            item.rare = 1;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Leather, 3);
            recipe.AddIngredient(ItemID.WormTooth, 1);
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
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                float dashSpeed = 3.5f;
                if (mpf.parryBuff)
                {
                    dashSpeed = 7f;
                }
                mpf.SetDashOnMovement(dashSpeed, 12f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParry(player, 18, 22);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 20, 9f, 2f);
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                player.lifeRegenCount += 2; // Regen 1 health a second
            }
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
                if (!player.moonLeech)
                {
                    player.HealEffect(5, false);
                    player.statLife += 5;
                    player.statLife = System.Math.Min(player.statLife, player.statLifeMax2);
                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, player.whoAmI, 5f);
                }
            }
        }
    }
}