using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesDungeon : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.EnableFists;
        }
        public static int comboEffect = 0;
        public static int buffID = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cerulean Claws");
			DisplayName.AddTranslation(GameCulture.Russian, "Лазурные Когти");
            Tooltip.SetDefault(
                "<right> consumes combo to unleash a flurry of strikes\n" +
                "Combo grants 25% increased melee attack speed");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы использовать комбо и ударить много раз\n" +
				"Комбо: +25% скорость атаки");
            comboEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            buffID = mod.BuffType<Buffs.Flurry>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 31;
            item.useAnimation = 16; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 3f;
            item.tileBoost = 10; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 15, 0);
            item.rare = 2;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.GoldenKey, 1);
            recipe.AddIngredient(ItemID.WaterCandle, 2);
            recipe.AddIngredient(ItemID.Spike, 6);
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
                float dashSpeed = 5f;
                if (player.FindBuffIndex(buffID) >= 0)
                {
                    dashSpeed = 8f;
                }
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(dashSpeed, 12f, 0.992f, 0.96f, true, 0);
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
                player.velocity.X = 0;
                player.velocity.Y = player.velocity.Y == 0f ? 0f : -5.5f;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
            }
            // Charging
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, 20);
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charge effect
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 172, 0, 0, 0, default(Color), 0.7f)];
                    d.fadeIn = 1.2f;
                    d.position -= d.velocity * 20f;
                    d.velocity *= 1.5f;
                    d.noGravity = true;
                }
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                player.AddBuff(buffID, 60, false);
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            float jump = 11.7f;
            if (player.FindBuffIndex(buffID) >= 0)
            {
                jump = 14f;
            }
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 22, jump, 0.5f, 12f);
        }

        //Combo
        public override void UpdateInventory(Player player) {            if (player.HeldItem != item) return;
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.meleeSpeed += 0.25f;
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 2, 22);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 perpendicular = velocity.RotatedBy(Math.PI / 2);
            Vector2 pVelo = (player.position - player.oldPosition);
            // Claw like effect
            for (int y = -1; y < 2; y++)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft() + perpendicular * y * 5, r.Width, r.Height, 172,
                        0, 0, 0, default(Color), 0.7f)];
                    d.velocity /= 8;
                    d.velocity += new Vector2(velocity.X, velocity.Y) + pVelo;
                    d.noGravity = true;
                }
            }
        }
    }
}
