using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesPlantera : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;
        public static int leafID = 0;
        public static int ballID = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Barbed Knuckles");
            Tooltip.SetDefault(
                "<right> consumes combo for a temporary barrier\n" +
                "Combo grants an orbiting ball of thorns\n" +
                "'Found deep in the jungle labyrinth'");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
            leafID = mod.ProjectileType<Projectiles.SpiritLeaf>();
            ballID = mod.ProjectileType<Projectiles.SpiritThornBall>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 95;
            item.useAnimation = 18; // 30%-50% reduction
            item.knockBack = 2.5f;
            item.tileBoost = 5; // Combo Power

            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 7;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20;
        const float fistDashSpeed = 9.5f;
        const float fistDashThresh = 8f;
        const float fistJumpVelo = 16f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = 11.5f;
        const float altDashThresh = 10f;
        const float altJumpVelo = 17.5f;

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                bool noBall = player.whoAmI == Main.myPlayer;
                foreach (Projectile p in Main.projectile)
                {
                    if (p.active && p.owner == Main.myPlayer && p.type == ballID)
                    { noBall = false; break; }
                }
                if (noBall)
                {
                    Projectile.NewProjectile(player.Center, new Vector2(), ballID, 100, 1f, Main.myPlayer, player.direction);
                }
            }
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 10;
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
            }

            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charging
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, DustID.GrassBlades, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 20f;
                d.velocity *= 1.5f;
                d.velocity += player.position - player.oldPosition;
                d.noGravity = true;
            }
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Create a shield of leaves
                Main.PlaySound(SoundID.Grass, player.position);
                if (player.whoAmI == Main.myPlayer)
                {
                    foreach (Projectile p in Main.projectile)
                    {
                        if (p.active && p.owner == Main.myPlayer && p.type == leafID)
                        {
                            p.Kill();
                        }
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(player.Center, new Vector2(), leafID, 100, 1f, Main.myPlayer, player.direction, MathHelper.PiOver2 * i);
                    }
                }
            }
            else
            {
                // Punch effect
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 1, 4);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 perpendicular = velocity.RotatedBy(Math.PI / 2);
            Vector2 pVelo = (player.position - player.oldPosition);
            // Claw like effect
            for (int y = -1; y < 2; y++)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust d = Dust.NewDustPerfect(r.TopLeft() + perpendicular * y * 4, 75, null, 0, default(Color), 0.7f);
                    d.velocity = new Vector2(velocity.X * -i, velocity.Y * -i);
                    d.position += velocity * fistHitboxSize / 2;
                    d.velocity += pVelo;
                    d.noGravity = true;
                }
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
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 3f, 14f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 3f, 14f);
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
