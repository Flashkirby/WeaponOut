using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsJungleClaws : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }
        public static int dustEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spiked Gauntlets");
            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Dash increases armor penetration by 12\n" +
                "Combo has a chance to poison enemies");
            dustEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 26;
            item.useAnimation = 20; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 1.5f;
            item.tileBoost = 7; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 3;

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
            recipe.AddIngredient(ItemID.FeralClaws, 1);
            recipe.AddIngredient(ItemID.Stinger, 3);
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
                SetDashOnMovement(4f, 12f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if(player.dashDelay == 0)
            {   
                player.GetModPlayer<ModPlayerFists>().
                SetDash(13f, 10f, 0.985f, 0.95f, false, dustEffect);
                return true;
            }
            return false;
        }
        /// <summary> The method called during a dash. Use for ongoing dust and gore effects. </summary>
        public static void DashEffects(Player player)
        {
            if (player.dashDelay == 0) { }
            for (int i = 0; i < 2; i++)
            {
                Dust d;
                if (player.velocity.Y == 0f)
                {
                    d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)player.height - 8f), player.width, 16, 39, player.velocity.X, 0f, 0, default(Color), 1.4f)];
                }
                else
                {
                    d = Main.dust[Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(player.height / 2) - 10f), player.width, 20, 40, player.velocity.X, 0f, 0, default(Color), 1.4f)];
                }
                d.velocity *= 0.1f;
                d.scale *= 1f + (float)Main.rand.Next(20) * 0.01f;
                d.noGravity = true;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 10 blocks high
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 22, 11.7f, 0.5f, 9f);
        }

        // Dash
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.dashEffect == dustEffect)
            {
                player.armorPenetration += 12;
            }
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if(mpf.IsComboActiveItemOnHit)
            {
                //apply poison for 7s
                target.AddBuff(BuffID.Poisoned, 420);
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 4, 20);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 pVelo = (player.position - player.oldPosition) * -2f + velocity * 0.5f;
            Dust d;
            for (int i = 0; i < 5; i++)
            {
                // Light spore
                d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 44, velocity.X * 3, velocity.Y * 3, 0, default(Color), 0.6f)];
                d.noGravity = true;
                // Grass
                d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 39, velocity.X * 1.2f, velocity.Y * 1.2f)];
                d.noGravity = true;
            }
        }
    }
}
