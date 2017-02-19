using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using WeaponOut.Items.Weapons.UseStyles;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons
{
    public class FistsOfFury : ModItem
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
                    fist = new FistStyle(item, 5);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Fists of Fury";
            item.toolTip = "<right> to dash attack";
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 30; //Half speed whilst combo-ing

            item.width = 28;
            item.height = 28;
            item.damage = 25;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item32;

            item.value = Item.sellPrice(0, 0, 24, 0);
            item.rare = 2;
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
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        public override bool AltFunctionUse(Player player)
        {
            if(player.dashDelay == 0) player.GetModPlayer<PlayerFX>(mod).weaponDash = 1;
            return player.dashDelay == 0;
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 24, 8f, 7f, 12f);

            Rectangle graphic = FistStyle.UseItemGraphicbox(player, 12);
            Vector2 velo = FistStyle.GetFistVelocity(player) * -2f + player.velocity * 0.5f;
            int d = Dust.NewDust(graphic.TopLeft(), graphic.Width, graphic.Height, 174, velo.X, velo.Y);
            Main.dust[d].noGravity = true;
            for (int i = 0; i < 10; i++)
            {
                d = Dust.NewDust(graphic.TopLeft(), graphic.Width, graphic.Height, 174, velo.X * 1.2f, velo.Y * 1.2f);
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
                    //set on fire
                    target.AddBuff(BuffID.OnFire, 300);
                }
            }
        }
    }
}
