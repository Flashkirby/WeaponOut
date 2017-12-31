using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class FistsBoxing : ModItem
    {
        public static int dustEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boxing Gloves");
            DisplayName.AddTranslation(GameCulture.Chinese, "拳击手套");

            Tooltip.SetDefault(
                "<right> to dash through enemies\n" +
                "Combo may confuse enemies");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键向敌人冲刺\n连击可能会使敌人混乱");

            dustEffect = ModPlayerFists.RegisterDashEffectID(DashEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 15;
            item.useAnimation = 28; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 4.5f;
            item.tileBoost = 8; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 15, 0);

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
            item.shootSpeed = 10 + item.rare / 2;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Silk, 6);
            recipe.AddTile(TileID.Loom);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(5f, 2f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if(player.dashDelay == 0)
            {   // Burst of speed with major slow down
                player.GetModPlayer<ModPlayerFists>().
                    SetDash(12f, 4f, 0.96f, 0.96f, false, dustEffect);
                return true;
            }
            return false;
        }
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

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 6 blocks high!
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 20, 9f, 0.5f, 12f);
        }

        // Combo
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if(mpf.IsComboActiveItemOnHit)
            {
                target.AddBuff(BuffID.Confused, 15 * Main.rand.Next(1, 5), false);
            }
        }
    }
}
