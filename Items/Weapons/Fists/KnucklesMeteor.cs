using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesMeteor : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }
        public static int comboEffect = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Fu");
            Tooltip.SetDefault(
                "<right> consumes combo to meteor strike!\n" +
                "Combo grants a protective barrier\n" +
                "'Space CQC is explicitly stated to be whatever you claim it to be'"); //これは宇宙CQC! sorry MPT no kanas are supported
            comboEffect = ModPlayerFists.RegisterComboEffectID(ComboEffects);
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 26;
            item.useAnimation = 19; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 2.5f;
            item.tileBoost = 5; // For fists, this is the combo power

            item.useTime = item.useAnimation * 2;
            item.shoot = mod.ProjectileType<Projectiles.SpiritComet>();
            item.shootSpeed = 6f;

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
            {   // Short dash brings up to max default speed.
                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(6.5f, 4f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionCombo(player, comboEffect);
        }
        /// <summary> The method called during a combo. Use for ongoing dust and gore effects. </summary>
        public static void ComboEffects(Player player, Item item, bool initial)
        {
            if (initial)
            {
                player.itemAnimation = player.itemAnimationMax + 12;
                Main.PlaySound(SoundID.DD2_SkyDragonsFurySwing, player.position);
            }
            // Charging
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, 20);
            if (player.itemAnimation > player.itemAnimationMax)
            {
                // Charge effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 6, 0, 0, 100, default(Color), 2f)];
                d.velocity /= 2;
                d.noGravity = true;
            }
            // Initial throw
            else if (player.itemAnimation == player.itemAnimationMax)
            {
                // Higher pitch
                Main.PlaySound(SoundID.Item88, (int)player.position.X, (int)player.position.Y);
                player.itemTime = 0;
            }
            else
            {
                // Punch effect
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), 16, 16, 6, 0, 0, 100, default(Color), 1.3f)];
                d.velocity = 1.4f * ModPlayerFists.GetFistVelocity(player);
                d.noGravity = true;
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.GetModPlayer<ModPlayerFists>().ComboEffectAbs == comboEffect &&
                player.itemAnimation < player.itemAnimationMax)
            {
                damage *= 5;
                return true;
            }
            return false;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 20, 10f, 0.5f);
        }

        //Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                player.AddBuff(mod.BuffType<Buffs.MeteorShield>(), 30);
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 4, 20);
            Vector2 velocity = ModPlayerFists.GetFistVelocity(player);
            Vector2 pVelo = (player.position - player.oldPosition);
            for (int i = 0; i < 2; i++)
            {
                Dust d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 174, 
                    velocity.X * -4f + pVelo.X, velocity.Y * -4f + pVelo.Y)];
                d.noGravity = true;
                d.velocity /= 2;
            }
        }
    }
}
