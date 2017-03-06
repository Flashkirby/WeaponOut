using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics.Shaders;

using WeaponOut.Items.Weapons.UseStyles;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons.Fists
{
    public class FistsMolten : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            equips.Add(EquipType.HandsOn);
            return ModConf.enableFists;
        }

        private FistStyle fist;
        public FistStyle Fist
        {
            get
            {
                if (fist == null)
                {
                    fist = new FistStyle(item, 5);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Phoenix Mark";
            item.toolTip = "<right> to dash for 50% increased melee damage and knockback";
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 26;

            item.width = 20;
            item.height = 20;
            item.damage = 38;
            item.knockBack = 5.5f;
            item.UseSound = SoundID.Item20;

            item.value = Item.sellPrice(0, 0, 28, 0);
            item.rare = 3;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Fist.ModifyTooltips(tooltips, mod);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            // Dust effect when Idle
            if (Main.time % 3 == 0)
            {
                Vector2 hand = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
                if (player.direction != 1)
                {
                    hand.X = (float)player.bodyFrame.Width - hand.X;
                }
                if (player.gravDir != 1f)
                {
                    hand.Y = (float)player.bodyFrame.Height - hand.Y;
                }
                hand -= new Vector2((float)(player.bodyFrame.Width - player.width), (float)(player.bodyFrame.Height - 42)) / 2f;
                Vector2 dustPos = player.RotatedRelativePoint(player.position + hand, true) - player.velocity;

                Dust d = Main.dust[Dust.NewDust
                    (player.Center, 0, 0, 174, (float)(player.direction * 2), 0f,
                    100, default(Color), 0.5f)
                    ];
                d.position = dustPos + player.velocity;
                d.velocity = Utils.RandomVector2(Main.rand, -0.5f, 0.5f) + player.velocity * 0.5f - new Vector2(0, player.gravDir);
                d.noGravity = true;
                d.fadeIn = 0.7f;
                d.shader = GameShaders.Armor.GetSecondaryShader(player.cHandOn, player);
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            if (player.dashDelay == 0) player.GetModPlayer<PlayerFX>(mod).weaponDash = 5;
            return player.dashDelay == 0;
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if(player.GetModPlayer<PlayerFX>(mod).weaponDash == 5)
            {
                damage = (int)(damage * 1.5f);
                knockBack *= 1.5f;
            }
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 16 blocks high!
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 26, 14.6f, 3f, 14f);

            Rectangle graphic = FistStyle.UseItemGraphicboxWithHitBox(player, 16, 26);
            Vector2 velo = FistStyle.GetFistVelocity(player) * -3f + player.velocity * 0.5f;
            for (int i = 0; i < 10; i++)
            {
                Dust d = Main.dust[Dust.NewDust(graphic.TopLeft(), graphic.Width, graphic.Height, 174, velo.X * 0.8f, velo.Y * 0.8f, 50, default(Color), 1.3f)];
                d.velocity *= 1.4f;
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
            if (combo != -1)
            {
                if (combo % Fist.punchComboMax == 0)
                {
                    //set on fire
                    target.AddBuff(BuffID.OnFire, 300);
                }
            }
            else
            {
                //set on fire a bit
                target.AddBuff(BuffID.OnFire, 60);
            }
        }
    }
}
