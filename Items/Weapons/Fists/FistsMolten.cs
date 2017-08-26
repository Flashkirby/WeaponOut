using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn)]
    public class FistsMolten : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }
        public static int dustEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phoenix Mark");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 50% increased melee damage and knockback\n" +
                "Combo grants 10 defense");
            dustEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 55;
            item.useAnimation = 26; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 5.5f;
            item.tileBoost = 5; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 28, 0);
            item.rare = 3;

            item.UseSound = SoundID.Item20;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(5f, 12f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if(player.dashDelay == 0)
            {   
                player.GetModPlayer<ModPlayerFists>().
                    SetDash(13f, 7f, 0.99f, 0.8f, false, dustEffect);
                return true;
            }
            return false;
        }
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                    DustID.Fire, 0, 0, 100, default(Color), 2f)];
                d.velocity = d.velocity * 0.5f + player.velocity * -0.4f;
                d.noGravity = true;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
                d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                    DustID.Smoke, 0, 0, 100, default(Color), 0.4f)];
                d.fadeIn = 0.7f;
                d.velocity = d.velocity * 0.1f + player.velocity * -0.2f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 16 blocks high!
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 26, 14.6f, 3f, 14f);
        }

        // Dash
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.dashEffect == dustEffect)
            {
                damage = (int)(damage * 1.5);
                knockBack *= 1.5f;
            }
        }
        
        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.statDefense += 10;
            }

            #region Hold Fire effect

            // Dust effect when Idle
            if (Main.time % 3 == 0)
            {
                Rectangle r = ModPlayerFists.GetPlayerOnHandRectangle(player, 10);

                Dust d = Main.dust[Dust.NewDust
                    (r.TopLeft(), r.Width, r.Height, 174, (float)(player.direction * 2), 0f,
                    100, default(Color), 0.5f)
                    ];
                d.position += player.position - player.oldPosition;
                d.velocity = Utils.RandomVector2(Main.rand, -0.5f, 0.5f) + player.velocity * 0.5f - new Vector2(0, player.gravDir);
                d.noGravity = true;
                d.fadeIn = 0.7f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cHandOn, player);
            }
            #endregion
        }

        // Hit Effect
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if(Main.rand.Next(3) == 0)
            {
                target.AddBuff(BuffID.OnFire, 180);
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 4, 26);
            Vector2 pVelo = (player.position - player.oldPosition);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player) * -3f + pVelo * 0.5f;
            Dust d;

            for (int i = 0; i < 6; i++)
            {
                d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 174,
                    velocity.X, velocity.Y)];
                d.velocity *= 1.2f;
                d.noGravity = true;
            }
        }
    }
}
