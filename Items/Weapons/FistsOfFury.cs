using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Items.Weapons
{
    public class FistsOfFury : ModItem
    {
        private const int punchComboMax = 5;
        private static Color comboColour = Color.Cyan;

        private bool isDramatic { get { return punchCombo >= punchComboMax; } }

        private int punchCombo;
        private int punchCount;

        public override void SetDefaults()
        {
            item.name = "Fists of Fury";
            item.toolTip = punchComboMax + @" combo potential
Dodge attacks during combos";
            item.useStyle = 6;//6+ for custom styles
            item.useTurn = false;
            item.autoReuse = true;
            item.useAnimation = 30;
            item.useTime = 20;

            item.width = 28;
            item.height = 28;
            item.damage = 15;
            item.shoot = mod.ProjectileType("FistFlameBlast");
            item.shootSpeed = 6;
            item.knockBack = 4f;
            item.useSound = 32;

            item.value = Item.sellPrice(0, 0, 24, 0);
            item.rare = 2;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation == 0)
            {
                if (punchCombo != 0) comboReset(player);
            }
            else
            {
                if (player.itemAnimation == player.itemAnimationMax - 1) punchCount++;
                if (player.itemAnimation == 1 && punchCount > punchCombo)
                {
                    comboReset(player);
                }
            }
            //Main.NewText(punchCombo + " / " + punchCount);
        }

        private void comboReset(Player player)
        {
            if (punchCombo > 2)
            {
                CombatText.NewText(player.getRect(),
                    comboColour, punchCombo + " hit combo", isDramatic, false);
            }
            //Main.NewText("punch reset");
            punchCombo = 0;
            punchCount = 0;
        }
        public override bool UseItemFrame(Player player)
        {
            UseStyles.FistStyle.UseItemFrame(player);

            if (player.itemTime < item.useTime - 1) //119
            {
                player.itemTime = item.useTime; //freeze at 119 until player stops attacking
            }
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = UseStyles.FistStyle.UseItemHitbox(player, ref hitbox, 12);
            if (!noHitbox)
            {
                Vector2 velo = UseStyles.FistStyle.GetFistVelocity(player) * -2f + player.velocity * 0.5f;
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 174, velo.X, velo.Y);
                Main.dust[d].noGravity = true;
                for (int i = 0; i < 10; i++)
                {
                    d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 174, velo.X * 1.2f, velo.Y * 1.2f);
                    Main.dust[d].noGravity = true;
                }
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            //C-C-Combo!
            punchCombo++;
            CombatText.NewText(player.getRect(),
                comboColour, string.Concat(punchCombo), isDramatic, false);
            if (punchCombo < punchComboMax)
            {
                player.itemAnimation = 2 * player.itemAnimation / 3;
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax);

                if (target.life > 0)
                {
                    //follow target
                    player.velocity = target.velocity;
                    if (target.noGravity) player.velocity.Y -= player.gravDir * 4f;
                }
            }
            if(punchCount == punchComboMax)
            {
                UseStyles.FistStyle.provideImmunity(player, player.itemAnimationMax);
                //disengage
                player.velocity += new Vector2(target.velocity.X * -2, target.velocity.Y * 2);
                //set on fire
                target.AddBuff(BuffID.OnFire, 300);
            }
        }
    }
}
