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
    public class KnucklesDuke : ModItem
    {
        public static int altEffect = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breakwater");
			DisplayName.AddTranslation(GameCulture.Russian, "Водорез");
            Tooltip.SetDefault(
                "<right> consumes combo to teleport\n" +
                "Combo drenches you in water");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы использовать комбо и телепортироваться\n" +
				"Комбо: заливает вас водой");
            altEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 325;
            item.useAnimation = 27; // 30%-50% reduction
            item.knockBack = 7f;
            item.tileBoost = 7; // Combo Power

            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = 8;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item19;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22;
        const float fistDashSpeed = 12f;
        const float fistDashThresh = 10f;
        const float fistJumpVelo = 16.85f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().ComboEffectAbs == altEffect; }
        const int altHitboxSize = (int)(fistHitboxSize * 2.5f);
        const float altDashSpeed = 20f;
        const float altDashThresh = 16f;
        const float altJumpVelo = 18.3f;

        // Combo
        public override void UpdateInventory(Player player) {            if (player.HeldItem != item) return;
            // Fishron effect
            if (player.MountFishronSpecial)
            {
                player.meleeDamage += 0.15f;
            }

            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.dripping = true;
            }
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.immuneTime += 2;
                player.velocity /= 2;
                player.itemAnimation = player.itemAnimationMax + 8;
                player.GetModPlayer<ModPlayerFists>().jumpAgainUppercut = true;
                Main.PlaySound(SoundID.Item21, player.position);
            }

            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 10, altHitboxSize);
            Dust d;
            if (player.itemAnimation > player.itemAnimationMax)
            {
                player.immune = true;
                player.immuneTime++;

                // Charging
                d = Main.dust[Dust.NewDust(r.TopLeft(), 10, 10, 217, 0, 0, 100, default(Color), 1.2f)];
                d.position -= d.velocity * 40f;
                d.velocity *= 2f;
                d.velocity += player.position - player.oldPosition;
                d.noGravity = true;
            }
            else if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                // Bubble Noise
                Main.PlaySound(SoundID.Item86, player.position);

                // Allow dash
                if (player.controlLeft && player.direction > 0) player.direction = -1;
                else if (player.controlRight && player.direction < 0) player.direction = 1;
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
                
                if (player.whoAmI == Main.myPlayer)
                {
                    if (Main.myPlayer == player.whoAmI)
                    {
                        Vector2 telePos;
                        telePos.X = (float)Main.mouseX + Main.screenPosition.X;
                        if (player.gravDir == 1f)
                        { telePos.Y = (float)Main.mouseY + Main.screenPosition.Y - (float)player.height; }
                        else
                        { telePos.Y = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY; }
                        telePos.X -= (float)(player.width / 2);

                        if (telePos.X > 50f && telePos.X < (float)(Main.maxTilesX * 16 - 50) && 
                            telePos.Y > 50f && telePos.Y < (float)(Main.maxTilesY * 16 - 50))
                        {
                            int num246 = (int)(telePos.X / 16f);
                            int num247 = (int)(telePos.Y / 16f);
                            if ((Main.tile[num246, num247].wall != 87 || (double)num247 <= Main.worldSurface || NPC.downedPlantBoss) && !Collision.SolidCollision(telePos, player.width, player.height))
                            {
                                player.Teleport(telePos, -1, 0);
                                NetMessage.SendData(MessageID.Teleport, -1, -1, null, -1, (float)player.whoAmI, telePos.X, telePos.Y, 0, 0, 0);
                            }

                            float num2 = Vector2.Distance(player.position, telePos);
                            if (num2 < new Vector2((float)Main.screenWidth, (float)Main.screenHeight).Length() / 2f + 100f)
                            {
                                Main.SetCameraLerp(0.1f, 0);
                            }
                            else
                            {
                                Main.BlackFadeIn = 255;
                                Lighting.BlackOut();
                                Main.screenLastPosition = Main.screenPosition;
                                Main.screenPosition.X = player.position.X + (float)(player.width / 2) - (float)(Main.screenWidth / 2);
                                Main.screenPosition.Y = player.position.Y + (float)(player.height / 2) - (float)(Main.screenHeight / 2);
                                Main.quickBG = 10;
                            }
                        }
                    }
                }
                if(ModPlayerFists.Get(player).specialMove == 0)
                {
                    player.velocity.Y = -player.gravDir;
                }
            }
            else
            {
                // Teledust
                d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 217, 0, 0, 100, default(Color), 1.2f)];
                d.velocity *= 2f;
                d.velocity += player.position - player.oldPosition;
                d.noGravity = true;
                for (int i = 0; i < 10; i++)
                {
                    d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 185, 0, 0, 100, default(Color), 1.2f)];
                    d.velocity *= 3f;
                    d.velocity += player.position - player.oldPosition;
                    d.noGravity = true;
                }
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
            if (AltStats(player))
            {
                damage += player.HeldItem.damage;
                crit = true;
            }
        }

        // Hit Impact Effect
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            // Impact
            int dustID = 33; if (player.wet) dustID = 34;
            Dust d;
            for (int i = 0; i < 1 + damage / 20; i++)
            {
                d = Main.dust[Dust.NewDust((player.Center + target.Center) / 2, 0, 0, 33, -target.velocity.X, target.velocity.Y, 100, default(Color), 0.5f)];
                d.velocity = (3f * d.velocity) + (3f * ModPlayerFists.GetFistVelocity(player));
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 12, 26);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            int dustID = 33; if (player.wet) dustID = 34;
            Vector2 pos = r.TopLeft();
            for (int i = 0; i < 3; i++)
            {
                d = Main.dust[Dust.NewDust(pos, r.Width, r.Height, dustID,
                    velocity.X, 0f, 0, default(Color), 1f + player.itemAnimation * 0.05f)];
                d.velocity.Y += velocity.Y - 1f;
                d.velocity *= 2f;
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
