using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using WeaponOut.Items.Weapons.UseStyles;
using System.Collections.Generic;

namespace WeaponOut.Items.Weapons.Fists
{
    public class FistsMolten : ModItem
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
            item.name = "Apocafist";
            item.toolTip = "<right> to dash";
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
        
        public override bool AltFunctionUse(Player player)
        {
            if (player.dashDelay == 0) player.GetModPlayer<PlayerFX>(mod).weaponDash = 5;
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
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 26, 9f, 7f, 12f);

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
        }
    }
}
