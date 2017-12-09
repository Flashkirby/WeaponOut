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
    public class KnucklesFrost : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Iced Punch");
            Tooltip.SetDefault(
                "<right> consumes combo to release a burst of icicles\n" +
                "Combo grants increased armor penetration and icy kicks");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 84;
            item.useAnimation = 25; // 30%-50% reduction
            item.knockBack = 1.5f;
            item.tileBoost = 9; // Combo Power

            item.useTime = item.useAnimation * 2;
            item.shoot = mod.ProjectileType<Projectiles.SpiritIcicle>();
            item.shootSpeed = 10f;

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 5;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20;
        const float fistDashSpeed = 12.5f;
        const float fistDashThresh = 11f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altJumpVelo = fistJumpVelo;
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.FrostCore, 2);
                if (i == 0)
                { recipe.AddIngredient(ItemID.AdamantiteBar, 5); }
                else
                { recipe.AddIngredient(ItemID.TitaniumBar, 5); }
                recipe.AddTile(TileID.MythrilAnvil);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        // Combo
        bool hitGround;
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.armorPenetration += mpf.ComboCounter * 3;

                if (Main.time % 2 == 0)
                {
                    Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.head, 43, 0f, 0f, 100)];
                    d.scale /= 2;
                    d.fadeIn = 0.9f;
                }

                // On landing with divekicks
                if (mpf.specialMove == 2 && player.velocity.Y == 0 && !hitGround)
                {
                    Main.PlaySound(SoundID.Item27, player.position);
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Vector2 position = new Vector2(
                            player.Center.X + player.direction * player.width / 2,
                            player.Center.Y + player.gravDir * player.height / 2
                            );
                        Vector2 direction = item.shootSpeed *
                            new Vector2(player.direction, -player.gravDir);
                        Projectile.NewProjectile(position, direction, item.shoot,
                              (int)(2 * item.damage * player.meleeDamage),
                              20f * player.meleeSpeed, player.whoAmI, 10);
                    }
                }
                hitGround = player.velocity.Y == 0;
            }
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                Main.PlaySound(SoundID.Item9, player.position);
                player.itemAnimation = player.itemAnimationMax + 20;
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
            }

            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 4, altHitboxSize);
            player.statDefense += player.itemAnimation; // Bonus defence during special
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charging
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 4, 4, 88, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 10f;
                d.velocity *= 1f;
                d.velocity += player.position - player.oldPosition;
                d.noGravity = true;
            }
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                Main.PlaySound(SoundID.Shatter, player.position);
                Dust d;
                Vector2 center = player.Center - new Vector2(4, 4);
                for (int i = 0; i < 15; i++)
                {
                    d = Main.dust[Dust.NewDust(center, 8, 8, DustID.Ice, 0, -1, 100, default(Color), 1.5f)];
                    d.velocity *= 3f;
                    d.noGravity = true;
                    d = Main.dust[Dust.NewDust(center, 8, 8, 88, 0, -1, 100, default(Color), 0.5f)];
                    d.velocity *= 2f;
                    d.fadeIn = 1f;
                    d.noGravity = true;
                }

                player.itemTime = 0;
            }
            else
            {
                // Punch effect
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect &&
                player.itemAnimation < player.itemAnimationMax)
            {
                float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 direction = item.shootSpeed * new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));

                    Projectile.NewProjectile(player.Center, direction, item.shoot,
                        (int)(2 * item.damage * player.meleeDamage),
                        20f * player.meleeSpeed, player.whoAmI);

                    angle += (float)(Math.PI / 4);
                }
            }
            return false;
        }

        //Combo
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                damage += player.HeldItem.damage;
                knockBack *= 5f;
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
