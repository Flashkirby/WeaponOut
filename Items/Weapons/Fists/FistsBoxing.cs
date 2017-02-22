using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using WeaponOut.Items.Weapons.UseStyles;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons.Fists
{
    public class FistsBoxing : ModItem
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
                    fist = new FistStyle(item, 3);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Boxing Glove";
            item.toolTip = "<right> to dash";
            item.toolTip2 = "Has a chance to confuse at the end of combos";
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 24; //Half speed whilst combo-ing

            item.width = 20;
            item.height = 20;
            item.damage = 10;
            item.knockBack = 6f;
            item.UseSound = SoundID.Item7;

            item.value = Item.sellPrice(0, 0, 10, 0);
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
            recipe.AddIngredient(ItemID.Silk, 6);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        public override bool AltFunctionUse(Player player)
        {
            if (player.dashDelay == 0) player.GetModPlayer<PlayerFX>(mod).weaponDash = 3;
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
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 22, 9f, 7f, 12f);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
            if (combo != -1)
            {
                if (combo % Fist.punchComboMax == 0)
                {
                    //maybe confuse
                    target.AddBuff(BuffID.Confused, 30 * Main.rand.Next(1, 4), false);
                }
            }
        }
    }
}
