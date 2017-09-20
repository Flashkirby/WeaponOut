using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsSlime : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime Slapper");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Combo restore 1 life per hit\n" + 
                "'Slip and slide!'");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 26;
            item.useAnimation = 26; // 30%-50% reduction
            item.knockBack = 5f;
            item.tileBoost = 10; // Combo Power

            item.value = Item.sellPrice(0, 0, 20, 0);
            item.rare = 1;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22;
        const float fistDashSpeed = 5.5f;
        const float fistDashThresh = 3f;
        const float fistJumpVelo = 10.5f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 9f;
        const float altDashThresh = 6f;

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            player.drippingSlime = true;
            Dust d;
            if (player.velocity.Y == 0f)
            {
                float height = player.height - 4f;
                if (player.gravDir < 0) height = 4f;
                d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + height), player.width, 8, DustID.t_Slime, 0f, 0f, 155, new Color(0, 80, 255, 100), 1f)];
            }
            else
            {
                d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 8f), 16, 16, DustID.t_Slime, 0f, 0f, 155, new Color(0, 80, 255, 100), 1f)];
            }
            d.velocity *= 0.1f;
            d.velocity.Y -= 1f;
            d.scale *= 1f + (float)Main.rand.Next(20) * 0.01f;
            d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit && !target.immortal)
            {
                PlayerFX.LifeStealPlayer(player, 1, 1, 1f);
            }
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
                    SetDash(altDashSpeed, altDashThresh, 0.998f, 0.98f, false, altEffect);
                return true;
            }
            return false;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, fistHitboxSize, fistJumpVelo, 1f, 9f);
        }
        #endregion

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }
    }
}
