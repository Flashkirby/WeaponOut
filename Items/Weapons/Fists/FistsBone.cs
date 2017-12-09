using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsBone : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }
        public static int altEffect = 0;
        public static int skeleBroID = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phalanx");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 30% increased melee damage\n" + 
                "Combo summons a backup skeleton\n" +
				"'Need a hand?'");
            altEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
            skeleBroID = mod.ProjectileType<Projectiles.Skelebro>();
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 30;
            item.useAnimation = 23; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 4f;
            item.tileBoost = 12; // Combo Power

            item.value = Item.sellPrice(0, 0, 60, 0);
            item.rare = 2;
            item.expert = true;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        const int fistHitboxSize = 22;
        const float fistDashSpeed = 6.5f;
        const float fistDashThresh = 4f;
        const float fistJumpVelo = 11.7f; // http://rextester.com/OIY60171
        public bool AltStats(Player p) { return p.GetModPlayer<ModPlayerFists>().dashEffect == altEffect; }
        const float altDashSpeed = 13f;
        const float altDashThresh = 9f;

        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0)
            {
                Gore g;
                if (player.velocity.Y == 0f)
                {
                    g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 4f), default(Vector2), Main.rand.Next(61, 64), 1f)];
                }
                else
                {
                    g = Main.gore[Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 24f, player.position.Y + (float)(player.height / 2) - 14f), default(Vector2), Main.rand.Next(61, 64), 1f)];
                }
                g.velocity.X = (float)Main.rand.Next(-50, 51) * 0.01f;
                g.velocity.Y = (float)Main.rand.Next(-50, 51) * 0.01f;
                g.velocity *= 0.4f;
            }
            else
            {
                for (int j = 0; j < 2; j++)
                {
                    Dust d;
                    if (player.velocity.Y == 0f)
                    {
                        float height = player.height - 4f;
                        if (player.gravDir < 0) height = 4f;
                        d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + height), player.width, 8, 31, 0f, 0f, 100, default(Color), 1.4f)];
                    }
                    else
                    {
                        d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 8f), player.width, 16, 31, 0f, 0f, 100, default(Color), 1.4f)];
                    }
                    d.velocity *= 0.1f;
                    d.scale *= 1f + (float)Main.rand.Next(20) * 0.01f;
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
                damage = (int)(damage * 1.3f);
            }
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                bool noBro = player.whoAmI == Main.myPlayer;
                foreach (Projectile p in Main.projectile)
                {
                    if (p.active && p.owner == Main.myPlayer && p.type == skeleBroID)
                    { noBro = false; break; }
                }
                if (noBro)
                {
                    Projectile.NewProjectile(player.Center, new Vector2(), skeleBroID, item.damage, item.knockBack, Main.myPlayer);
                }
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
