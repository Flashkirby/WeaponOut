using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons.Fists
{
    public class GlovesFossil : ModItem
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
                    fist = new FistStyle(item, 3);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Bonefist";
            item.toolTip = "<right> to parry attacks";
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 16;
            item.useTime = 16;

            item.width = 26;
            item.height = 26;
            item.damage = 18;
            item.knockBack = 5f;
            item.UseSound = SoundID.Item7;

            item.value = Item.sellPrice(0, 0, 25, 0);
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
            recipe.AddIngredient(ItemID.FossilOre, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }


        public override void HoldItem(Player player)
        {
            Fist.HoldItemOnParryFrame(player, mod, false, "Punches have increased armor penetration and knockback");
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            int buffIndex = Fist.HoldItemOnParryFrame(player, mod, false);
            if (buffIndex >= 0)
            {
                knockBack += 2f;
                ModifyHitAny(player, ref damage, ref crit);
            }
        }
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        {
            int buffIndex = Fist.HoldItemOnParryFrame(player, mod, false);
            if (buffIndex >= 0)
            {
                ModifyHitAny(player, ref damage, ref crit);
            }
        }
        public void ModifyHitAny(Player player, ref int damage, ref bool crit)
        {
            player.armorPenetration += 20;
        }

        public override bool AltFunctionUse(Player player)
        {
            return Fist.AtlFunctionParry(player, mod, 22, 18);
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 24, 11.7f, 9f, 8f); //10 blocks
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
        }
    }
}
