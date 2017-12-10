using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn)]
    public class KnucklesDemon : ModItem
    {
        public static int altEffect = 0;
        public static int customDashEffect = 0;
        public static int buffID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Hand");
            Tooltip.SetDefault(
                "<right> to dash, or consume combo to steal life from enemies\n" +
                "Combo grants increased melee damage at the cost of defense\n" + 
                "'Might makes right'");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            customDashEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            buffID = mod.BuffType<Buffs.DemonFrenzy>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 55;
            item.useAnimation = 22; // 30%-50% reduction
            item.knockBack = 4f;
            item.tileBoost = 13; // Combo Power

            item.value = Item.sellPrice(0, 0, 15, 0);
            item.rare = 3;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 30;
        const float fistDashSpeed = 8f;
        const float fistDashThresh = 12f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = 64    ;
        const float altDashSpeed = 16f;
        const float altDashThresh = 12f;
        const float altJumpVelo = 16.85f;
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(mod.ItemType<FistsMolten>(), 1);
                recipe.AddIngredient(mod.ItemType<KnucklesDungeon>(), 1);
                recipe.AddIngredient(mod.ItemType<FistsJungleClaws>(), 1);
                if (i == 0)
                { recipe.AddIngredient(mod.ItemType<GlovesCaestus>(), 1); }
                else
                { recipe.AddIngredient(mod.ItemType<GlovesCaestusCrimson>(), 1); }
                recipe.AddTile(TileID.DemonAltar);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }
        
        //Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.AddBuff(buffID, 2);
            }
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 10;
                Main.PlaySound(SoundID.Item73);
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
            }

            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 8, altHitboxSize);
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charging
                Dust.NewDust(r.TopLeft(), r.Width, r.Height, 21, 0f, 0f, 0, default(Color), 0.5f);
            }
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                Main.PlaySound(SoundID.Item71, player.position);
                // Force dash
                player.GetModPlayer<ModPlayerFists>().
                SetDash(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);

                for (int i = 0; i < 64; i++)
                {
                    double angle = Main.time + i / 10.0;
                    Dust d = Dust.NewDustPerfect(player.Center, 21,
                        new Vector2((float)(5.0 * Math.Sin(angle)), (float)(5.0 * Math.Cos(angle))));
                }
            }
            else
            {
                player.yoraiz0rEye = Math.Max(2, player.yoraiz0rEye);
                if(player.attackCD > 2) player.attackCD = 2; // Attack more things
                for (int i = 0; i < 5; i++)
                {
                    int d = Dust.NewDust(r.TopLeft(), r.Width, r.Height, 27, player.velocity.X * -0.5f, player.velocity.Y * -0.5f, 180);
                    Main.dust[d].noGravity = true;
                }
            }
        }
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(player.position, player.width, player.height, 27, player.velocity.X * -0.5f, player.velocity.Y * -0.5f, 180);
                Main.dust[d].noGravity = true;
                Main.dust[d].shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (AltStats(player) && !target.immortal)
            {
                player.lifeSteal += Math.Min(target.lifeMax, damage) / 4f; // Each hit restores half lifesteal potential
                PlayerFX.LifeStealPlayer(player, damage, target.lifeMax, 1f / 2f);
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
                    Dust d = Main.dust[Dust.NewDust(r.TopLeft() + perpendicular * y * 7, r.Width, r.Height, 27,
                        0, 0, 0, default(Color), 0.7f)];
                    d.velocity /= 4;
                    d.velocity += new Vector2(velocity.X * -2, velocity.Y * -2);
                    d.position -= d.velocity * 8;
                    d.velocity += pVelo;
                    d.noGravity = true;
                }
            }
        }


        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if (player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, altEffect)
                )
            {
                return true;
            }
            else
            {
                // Since auto calls at 1, don't want this dash to happen before
                // the combo attack, which only happens with itemAnimation == 0
                if (player.dashDelay == 0 && player.itemAnimation == 0)
                {
                    player.GetModPlayer<ModPlayerFists>().
                        SetDash(altDashSpeed, altDashThresh, 0.992f, 0.96f, false, customDashEffect);
                    return true;
                }
            }
            return false;
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
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
