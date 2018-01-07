using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesIron : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.EnableFists;
        }
        public static int comboEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Iron Knuckleduster");
			DisplayName.AddTranslation(GameCulture.Russian, "Железный Кастет");
            Tooltip.SetDefault(
                "<right> consumes combo for for an empowered strike\n" +
                "Combo grants 4 bonus damage");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы использовать комбо и нанести мощный удар\n" +
				"Комбо: +4 урон");
            comboEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 14;
            item.useAnimation = 24; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 2.5f;
            item.tileBoost = 6; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 3, 0);

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
            item.shootSpeed = 10 + item.rare / 2;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IronBar, 2);
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
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(6f, 4f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, comboEffect);
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 20;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
            }
            // Charging
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, 45);
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charge effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 31, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 10f;
                d.velocity /= 2;
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                Main.PlaySound(42, (int)player.position.X, (int)player.position.Y, 184, 1f, 0.5f);
                // Allow dash
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(3f, 12f, 0.992f, 0.96f, true, 0);
            }
            else
            {
                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 31, 3, 3, 100, default(Color), 1f)];
                d.velocity *= 0.6f * ModPlayerFists.GetFistVelocity(player);
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            int size = 18;
            float jump = 9f;
            if (mpf.ComboEffectAbs == comboEffect)
            {
                size = (int)(size * 2.5f);
                jump = 11f;
            }
            // jump exactly 6 blocks high!
            ModPlayerFists.UseItemHitbox(player, ref hitbox, size, jump, 0.5f, 8f);
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
                damage += 4;
            }
            if(mpf.ComboEffectAbs == comboEffect)
            {
                damage += player.HeldItem.damage * 4;
                knockBack *= 2f;
            }
        }
    }
}
