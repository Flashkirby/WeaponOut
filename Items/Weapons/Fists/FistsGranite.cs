using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsGranite : ModItem
    {
        public static int altEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Granite Smasher");
			DisplayName.AddTranslation(GameCulture.Russian, "Гранитный Крушитель");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 10% increased melee damage\n" +
                "Combo grants 8 defense");
				Tooltip.AddTranslation(GameCulture.Russian,
				"<right>, чтобы прорваться через врагов\n" +
				"Рывок: +10% урон\n" +
				"Комбо: +8 защиты");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 22;
            item.useAnimation = 25; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 5f;
            item.tileBoost = 5; // Combo Power

            item.value = Item.sellPrice(0, 0, 15, 0);
            item.rare = 1;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 20;
        const float fistDashSpeed = 5f;
        const float fistDashThresh = 3f;
        const float fistJumpVelo = 10.5f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 11f;
        const float altDashThresh = 7.5f;

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            Dust d;
            if (player.velocity.Y == 0f)
            {
                float height = player.height - 4f;
                if (player.gravDir < 0) height = 4f;
                d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + height), player.width, 8, 111, 0f, 0f, 0, default(Color), 1f)];
                d.noGravity = true;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    d = Main.dust[Dust.NewDust(player.position, player.width, player.height, 111, 0f, 0f, 0, default(Color), 0.8f)];
                    d.noGravity = true;
                    d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                }
            }
        }

        // Dash
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            if (AltStats(player))
            {
                damage = (int)(damage * 1.1f);
            }
        }

        // Combo
        public override void UpdateInventory(Player player) {            if (player.HeldItem != item) return;
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.statDefense += 8;
            }
        }
        public override void HoldItem(Player player) 
        {
            #region Hold Glow effect
            // Dust effect when Idle
            Rectangle r = ModPlayerFists.GetPlayerOnHandRectangle(player, 4);

            Dust d = Dust.NewDustDirect
                (r.TopLeft(), r.Width, r.Height, 111, 0f, 0f,
                0, default(Color), 0.5f);
            d.velocity *= 0.1f;
            d.position += player.position - player.oldPosition;
            d.noGravity = true;
            d.fadeIn = 0.6f;
            d.shader = GameShaders.Armor.GetSecondaryShader(player.cHandOn, player);
            #endregion
        }

        #region Prehard Dash Base
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
            if (player.dashDelay == 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                    SetDash(altDashSpeed, altDashThresh, 0.992f, 0.96f, false, altEffect);
                return true;
            }
            return false;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 0.5f, 9f);
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
