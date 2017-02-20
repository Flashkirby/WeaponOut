using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class GlovesCaestusCrimson : ModItem
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
                    fist = new FistStyle(item, 2);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "The Tenderizer";
            item.toolTip = "<right> to parry attacks";
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.useAnimation = 22;//actually treated as -2
            item.useTime = 22;

            item.width = 28;
            item.height = 28;
            item.damage = 24;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item7;

            item.value = Item.sellPrice(0, 0, 10, 0);
            item.rare = 1;
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
            recipe.AddIngredient(ItemID.CrimtaneBar, 4);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            Fist.HoldItemOnParryFrame(player, mod, false, "Heal 5 life and dash for next landed punch");
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
                if (!player.moonLeech)
                {
                    player.HealEffect(5, false);
                    player.statLife += 5;
                    player.statLife = System.Math.Min(player.statLife, player.statLifeMax2);
                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, "", player.whoAmI, 5f);
                }
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return Fist.AtlFunctionParry(player, mod, 12, 18);
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            int buffIndex = Fist.HoldItemOnParryFrame(player, mod, false);
            if (buffIndex >= 0)
            {
                if (player.dashDelay == 0) player.GetModPlayer<PlayerFX>(mod).weaponDash = 2;
            }

            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 20, 8f, 8f, 8f);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
        }
    }
}
