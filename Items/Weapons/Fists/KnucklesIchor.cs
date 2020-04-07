using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesIchor : ModItem
    {
        public static int altEffect = 0;
        public static int buffID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blood Baghnakh");
            DisplayName.AddTranslation(GameCulture.Chinese, "血腥虎爪");
            DisplayName.AddTranslation(GameCulture.Russian, "Кровавый Багнак");

            Tooltip.SetDefault(
                "<right> consumes combo and life to greatly increase melee damage\n" +
                "Combo inflicts ichor and steals life from enemies");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键消耗连击能量和生命值以大幅增加近战伤害\n连击将窃取生命值并使敌人被脓血感染");
            Tooltip.AddTranslation(GameCulture.Russian, 
				"<right>, чтобы использовать комбо и здоровье для усиления ближнего боя\n" +
				"Комбо: насылает ихор и крадёт здоровье врагов");


            ModTranslation text;

            text = mod.CreateTranslation("KnucklesIchorSuicide1");
            text.SetDefault("$NAME ripped out their mortal coil.");
            text.AddTranslation(GameCulture.Chinese, "$NAME 的肉体被绞成一团");
			text.AddTranslation(GameCulture.Russian, "$NAME вырвало внутренности.");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("KnucklesIchorSuicide2");
            text.SetDefault("$NAME was disemboweled by the $ITEM.");
            text.AddTranslation(GameCulture.Chinese, "$NAME被$ITEM开膛破肚");
			text.AddTranslation(GameCulture.Russian, "$NAME был распотрашён $ITEM");
            mod.AddTranslation(text);

            text = mod.CreateTranslation("KnucklesIchorSuicide3");
            text.SetDefault("$NAME tore themself apart.");
            text.AddTranslation(GameCulture.Chinese, "$NAME 被大卸八块");
			text.AddTranslation(GameCulture.Russian, "$NAME разорвался пополам");
            mod.AddTranslation(text);
            
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            buffID = ModContent.BuffType<Buffs.BloodLust>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 70;
            item.useAnimation = 18; // 30%-50% reduction
            item.knockBack = 3.5f;
            item.tileBoost = 6; // Combo Power

            item.value = Item.sellPrice(0, 0, 92, 0);
            item.rare = 5;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 16;
        const float fistDashSpeed = 10f;
        const float fistDashThresh = 12f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = 16f;
        const float altDashThresh = 14f;
        const float altJumpVelo = 17.5f;
        public override void AddRecipes() {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Ichor, 10);
            recipe.AddIngredient(ItemID.SoulofNight, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 18;
                player.AddBuff(buffID, 60 * 6);
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
            }

            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 10, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charging
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, DustID.Blood, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 20f;
                d.velocity *= 1.5f;
                d.velocity += player.position - player.oldPosition;
                d.noGravity = true;
            }
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Lower pitch
                Main.PlaySound(4, (int)player.position.X, (int)player.position.Y, 12, 1f, -0.2f);
                if (player.whoAmI == Main.myPlayer)
                {
                    PlayerDeathReason pdr = PlayerDeathReason.LegacyDefault();
                    switch (Main.rand.Next(3))
                    {
                        case 1:
                            pdr.SourceCustomReason = WeaponOut.GetTranslationTextValue("KnucklesIchorSuicide1")
                                .Replace("$NAME", player.name);
                            break;
                        case 2:
                            pdr.SourceCustomReason = WeaponOut.GetTranslationTextValue("KnucklesIchorSuicide2")
                                .Replace("$NAME", player.name).Replace("$ITEM", item.Name);
                            break;
                        default:
                            pdr.SourceCustomReason = WeaponOut.GetTranslationTextValue("KnucklesIchorSuicide3")
                                .Replace("$NAME", player.name);
                            break;
                    }
                    player.immune = false;
                    player.Hurt(pdr, 100 + player.statDefense / 2, player.direction, false, false, false, -1);
                }

                for (int i = 0; i < 30; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, DustID.Blood, 0, -0.5f, 0, default(Color), 2f)];
                    d.velocity.X *= 2f;
                }

                // Allow dash
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                // Punch effect
            }
        }
        
        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                target.AddBuff(BuffID.Ichor, 60 * 10);

                if (!target.immortal)
                {
                    int divider = 60;
                    if (player.FindBuffIndex(buffID) >= 0) { divider = 30; }
                    PlayerFX.LifeStealPlayer(player, damage, target.lifeMax, 1f / divider);
                }
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 1, 8);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 perpendicular = velocity.RotatedBy(Math.PI / 2);
            Vector2 pVelo = (player.position - player.oldPosition);
            // Claw like effect
            for (int y = -1; y < 2; y++)
            {
                Dust d = Dust.NewDustPerfect(r.TopLeft() + perpendicular * y * 7, 170, null, 0, default(Color), 0.6f);
                d.velocity = new Vector2(velocity.X * -2, velocity.Y * -2);
                d.position -= d.velocity * 8;
                d.velocity += pVelo;
                d.fadeIn = 0.7f;
                d.noGravity = true;
            }
        }

        #region Hardmode Combo Base
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect);
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 14f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
