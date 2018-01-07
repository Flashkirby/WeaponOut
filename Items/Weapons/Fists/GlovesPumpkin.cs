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
    public class GlovesPumpkin : ModItem
    {
        public static int buffID = 0;
        public static int projID = 0;
        const int buffTime = 60 * 6;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Patchy Pounder");
			DisplayName.AddTranslation(GameCulture.Russian, "Тыквенный Подрывник");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Strike through enemies to mark them\n" +
                "Counterstrike to detonate marked enemies\n" + 
                "Combo grants increased explosive potential\n" +
                "'Its explosive might makes the heavens tremble'");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы парировать удар\n" +
				"Помечайте врагов атаками\n"
				"Контратака: подрывает помеченных врагов\n" +
				"Комбо: усиливает взрывы");
            buffID = mod.BuffType<Buffs.PumpkinMark>();
            projID = mod.ProjectileType<Projectiles.SpiritPumpkinsplosion>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 292; //650dps vs 20def
            item.useAnimation = 26; // 30%-50% reduction
            item.knockBack = 4f;
            item.tileBoost = 11; // Combo Power

            item.value = Item.sellPrice(0, 2, 0, 0);
            item.rare = 8;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.DD2_SonicBoomBladeSlash;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 26;
        const float fistDashSpeed = 12f;
        const float fistDashThresh = 7f;
        const float fistJumpVelo = 14.8f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().parryBuff; }
        const int altHitboxSize = 32;
        const float altDashSpeed = 19f;
        const float altDashThresh = 14f;
        const float altJumpVelo = 17.6f;
        const int parryActive = 18;
        const int parryCooldown = 12;

        // Parry
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (AltStats(player))
            {
                damage += 357;
            }
        }
        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            int buffIndex = target.FindBuffIndex(buffID);
            if (AltStats(player))
            {
                OnHit(player, target, damage, 5f, crit, buffIndex >= 0);
                if (buffIndex >= 0)
                {
                    target.DelBuff(buffIndex);
                }
                else
                {
                    target.buffImmune[buffID] = false;
                    target.AddBuff(buffID, buffTime, false);
                }
            }
            else
            {
                target.buffImmune[buffID] = false;
                target.AddBuff(buffID, buffTime, false);
            }
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            int buffIndex = target.FindBuffIndex(buffID);
            if (AltStats(player))
            {
                OnHit(player, target, damage, 5f, crit, buffIndex >= 0);
                if (buffIndex >= 0)
                {
                    target.DelBuff(buffIndex);
                }
                else
                {
                    target.buffImmune[buffID] = false;
                    target.AddBuff(buffID, buffTime, false);
                }
            }
            else
            {
                target.buffImmune[buffID] = false;
                target.AddBuff(buffID, buffTime, false);
            }
        }
        private void OnHit(Player player, Entity target, int damage, float knockBack, bool crit, bool detonate)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            int parryIndex = mpf.GetParryBuff();
            if (detonate)
            {
                if (parryIndex >= 0) mpf.ClearParryBuff();
                int dmg = 400;
                if (mpf.IsComboActiveItemOnHit)
                {
                    // Combo detonate causes delayed triple explosion
                    // This means if you can keep the target still enough you can mark the target
                    // BEFORE the last explosion triggers on it.
                    // For massive damage!!! chain reactions ftw
                    for (int i = 1; i < 3; i++)
                    {
                        Vector2 pos = new Vector2(
                            64 * Main.rand.NextFloatDirection(),
                             64 * Main.rand.NextFloatDirection()
                            );
                        Projectile.NewProjectile(target.Center + pos, new Vector2(), projID, dmg, 12f, player.whoAmI, -15f * i);
                    }
                }
                Projectile.NewProjectile(target.Center, new Vector2(), projID, dmg, 12f, player.whoAmI, 0f);
            }
            else
            {
                //quickly degrade parry buff
                if (parryIndex >= 0)
                {
                    player.buffTime[parryIndex] -= 120;
                    if (player.buffTime[parryIndex] < 1) player.buffTime[parryIndex] = 1;
                }
            }
        }

        #region Hardmode Parrydash Base
        public override bool CanUseItem(Player player)
        {
            if (AltStats(player) || player.FindBuffIndex(buffID) >= 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(altDashSpeed, altDashThresh, 0.992f, 0.96f, true, 0);
            }
            else
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(fistDashSpeed, fistDashThresh, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParryDash(player, parryActive, parryCooldown, fistJumpVelo - fistJumpVelo / 4, fistDashSpeed / 2f, fistDashThresh / 2);
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!AltStats(player))
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 14f);
            }
            else
            {
                ModPlayerFists.UseItemHitbox(player, ref hitbox, altHitboxSize, altJumpVelo, 0.5f, 15f,
                    ModPlayerFists.MovingInDash());
            }
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
