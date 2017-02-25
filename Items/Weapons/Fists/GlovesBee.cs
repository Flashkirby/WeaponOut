using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons.Fists
{
    public class GlovesBee : ModItem
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
                    fist = new FistStyle(item, 5);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Honeycomb Glove";
            item.toolTip = "<right> to parry attacks and release bees";
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 23;
            item.useTime = 23;

            item.width = 26;
            item.height = 26;
            item.damage = 28;
            item.knockBack = 4.5f;
            item.UseSound = SoundID.Item7;

            item.value = Item.sellPrice(0, 0, 40, 0);
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
            recipe.AddIngredient(ItemID.BeeWax, 8);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }


        public override void HoldItem(Player player)
        {
            Fist.HoldItemOnParryFrame(player, mod, false, "30 bonus damage and release bees for next landed punch");
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
                damage += 30;

                int numberOfBees = 2;
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

        public override bool AltFunctionUse(Player player)
        {
            return Fist.AtlFunctionParry(player, mod, 20, 10);
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 22, 11.7f, 9f, 8f); //10 blocks
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
        }
    }
}
