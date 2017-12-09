using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class GlovesBee : ModItem
    {
        public override bool Autoload(ref string name)
        {
            return ModConf.enableFists;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Honeycomb Glove");
            Tooltip.SetDefault(
                "<right> to parry incoming damage\n" +
                "Counterstrike grants 60 bonus damage and releases a swarm of bees\n" +
                "Combo increases the strength of friendly bees\n" +
                "'Nice to BEEt you'");
        }
        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 42;
            item.useAnimation = 23; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.knockBack = 4.5f;
            item.tileBoost = 10; // For fists, we read this as the combo power

            item.value = Item.sellPrice(0, 0, 40, 0);
            item.rare = 3;
            item.shootSpeed = 10 + item.rare / 2;

            item.UseSound = SoundID.Item7;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 26;
            item.height = 26;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BeeWax, 8);
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
                ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
                float dashSpeed = 5f;
                if (mpf.parryBuff)
                {
                    dashSpeed = 10f;
                }
                mpf.SetDashOnMovement(dashSpeed, dashSpeed - 0.5f, 0.992f, 0.96f, true, 0);
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<ModPlayerFists>().
                AltFunctionParry(player, 20, 10);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 24, 11.7f, 0.5f, 8f); //10 blocks
        }

        // Parry
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { float knockBack = 5f; ModifyHit(player, ref damage, ref knockBack, ref crit); }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHit(player, ref damage, ref knockBack, ref crit); }
        private void ModifyHit(Player player, ref int damage, ref float knockBack, ref bool crit)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();

            // Just periodically release more bees
            if(mpf.ComboCounter % 2 == 0)
            {
                float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
                float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
                Projectile.NewProjectile(player.position.X, player.position.Y, speedX, speedY, player.beeType(), player.beeDamage(7), player.beeKB(0f), Main.myPlayer, 0f, 0f);
            }

            // Parry for bees
            if (mpf.parryBuff)
            {
                if (mpf.GetParryBuff() >= 0) mpf.ClearParryBuff();
                damage += 60;

                // THE BEES NOT THE BEES
                int numberOfBees = 2;
                if(mpf.ComboCounter >= mpf.ComboCounterMaxReal * 2)
                { numberOfBees++; }
                if (mpf.ComboCounter >= mpf.ComboCounterMaxReal * 3)
                { numberOfBees++; }
                if (mpf.ComboCounter >= mpf.ComboCounterMaxReal * 4)
                { numberOfBees++; }
                if (player.strongBees && mpf.ComboCounter >= mpf.ComboCounterMaxReal * 5)
                { numberOfBees++; }
                if (Main.rand.Next(3) == 0)
                {
                    numberOfBees++;
                }
                if (Main.rand.Next(3) == 0)
                {
                    numberOfBees++;
                }
                if (player.strongBees && Main.rand.Next(3) == 0)
                {
                    numberOfBees++;
                }
                for (int i = 0; i < numberOfBees; i++)
                {
                    float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
                    float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
                    Projectile.NewProjectile(player.position.X, player.position.Y, speedX, speedY, player.beeType(), player.beeDamage(7), player.beeKB(0f), Main.myPlayer, 0f, 0f);
                }
            }
        }

        // Combo
        public override void HoldItem(Player player)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActiveItemOnHit)
            {
                player.strongBees = true; // Same effect as Hive Pack
            }
        }

        // Melee Effect
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Rectangle r = ModPlayerFists.UseItemGraphicbox(player, 16, 24);
            Vector2 pVelo = (player.position - player.oldPosition);

            Dust d = Main.dust[Dust.NewDust(r.TopLeft(), r.Width, r.Height, 152,
                pVelo.X, pVelo.Y, 120, default(Color), 1.2f)];
            d.noGravity = true;
        }
    }
}