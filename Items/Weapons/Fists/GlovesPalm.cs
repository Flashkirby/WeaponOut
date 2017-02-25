using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons.Fists
{
    public class GlovesPalm : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            equips.Add(EquipType.HandsOn);
            equips.Add(EquipType.HandsOff);
            return ModConf.enableFists;
        }

        private FistStyle fist;
        public FistStyle Fist
        {
            get
            {
                if (fist == null)
                {
                    fist = new FistStyle(item, 4);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Palm Striker";
            item.toolTip = "<right> to parry attacks";
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.useAnimation = 25;//actually treated as -2
            item.useTime = 25;

            item.width = 20;
            item.height = 20;
            item.damage = 9;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item7;

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
            recipe.AddIngredient(ItemID.PalmWood, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            Fist.HoldItemOnParryFrame(player, mod, false, "20 bonus damage for next landed punch");
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        { ModifyHitAny(player, ref damage, ref crit); }
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        { ModifyHitAny(player, ref damage, ref crit); }
        public void ModifyHitAny(Player player, ref int damage, ref bool crit)
        {
            int buffIndex = Fist.HoldItemOnParryFrame(player, mod, false);
            if (buffIndex >= 0)
            {
                player.ClearBuff(player.buffType[buffIndex]);
                damage += 20;
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return Fist.AtlFunctionParry(player, mod, 15, 30);
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 20, 9f, 8f, 8f);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
        }
    }
}
