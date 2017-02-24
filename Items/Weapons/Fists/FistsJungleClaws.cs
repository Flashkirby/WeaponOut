using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using WeaponOut.Items.Weapons.UseStyles;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons.Fists
{
    public class FistsJungleClaws : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            return ModConf.enableFists;
        }

        private FistStyle fist;
        public FistStyle Fist
        {
            get
            {
                if (fist == null)
                {
                    fist = new FistStyle(item, 6);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Spiked Gauntlets";
            item.toolTip = "<right> to dash";
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 15; //Half speed whilst combo-ing

            item.width = 20;
            item.height = 20;
            item.damage = 20;
            item.knockBack = 1f;
            item.UseSound = SoundID.Item7;

            item.value = Item.sellPrice(0, 1, 0, 0);
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
            recipe.AddIngredient(ItemID.FeralClaws, 1);
            recipe.AddIngredient(ItemID.Stinger, 3);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        public override bool AltFunctionUse(Player player)
        {
            if (player.dashDelay == 0) player.GetModPlayer<PlayerFX>(mod).weaponDash = 4;
            return player.dashDelay == 0;
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 6 blocks high!
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 22, 9f, 7f, 8f);

            Rectangle graphic = FistStyle.UseItemGraphicboxWithHitBox(player, 12, 22);
            Vector2 velo = FistStyle.GetFistVelocity(player) * -2f + player.velocity * 0.5f;
            for (int i = 0; i < 5; i++)
            {
                // Light spore
                int d = Dust.NewDust(graphic.TopLeft(), graphic.Width, graphic.Height, 44, velo.X * 3, velo.Y * 3, 0, default(Color), 0.6f);
                Main.dust[d].noGravity = true;
                // Grass
                d = Dust.NewDust(graphic.TopLeft(), graphic.Width, graphic.Height, 39, velo.X * 1.2f, velo.Y * 1.2f);
                Main.dust[d].noGravity = true;
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
            if (combo != -1)
            {
                if (combo % Fist.punchComboMax == 0)
                {
                    //apply poison for 7s
                    target.AddBuff(BuffID.Poisoned, 420);
                }
            }
        }
    }
}
