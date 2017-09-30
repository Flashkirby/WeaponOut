using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsOfFury : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }
        public static int dustEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fists of Fury");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash grants 15% increased melee damage and knockback\n" +
                "Combo may ignite enemies");
            dustEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 41;
            item.useAnimation = 25; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 4f;
            item.tileBoost = 5; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 24, 0);
            item.rare = 2;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.noUseGraphic = true;
            item.width = 20;
            item.height = 20;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
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
                SetDashOnMovement(5f, 3f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if(player.dashDelay == 0)
            {   
                player.GetModPlayer<ModPlayerFists>().
                    SetDash(10f, 8f, 0.98f, 0.94f, false, dustEffect);
                return true;
            }
            return false;
        }
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player, Item item)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 3; i++)
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height,
                    DustID.Fire, 0, 0, 100, default(Color), 1.8f)];
                d.velocity = d.velocity * 0.5f + player.velocity * -0.4f;
                d.noGravity = true;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 6 blocks high!
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 24, 12.5f, 1f, 9f);
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
                damage = (int)(damage * 1.15f);
                knockBack *= 1.15f;
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if(mpf.IsComboActiveItemOnHit)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 4, 20);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 pVelo = (player.position - player.oldPosition);
            Dust d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 174,
                    velocity.X + pVelo.X, velocity.Y + pVelo.Y)];
            d.noGravity = true;

            for (int i = 0; i < 3; i++)
            {
                d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 174,
                    velocity.X * 1.2f + pVelo.X, velocity.Y * 1.2f + pVelo.Y)];
                d.noGravity = true;
            }
        }
    }
}
