using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class KnucklesMeteor : ModItem
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
            item.name = "Comet Fu";
            item.toolTip = "<right> at full combo power to unleash meteors";
            item.toolTip2 = "'Kore wa uchuu CQC!'"; // これは宇宙CQC! but no jpn support, rip MPT
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.useAnimation = 19;//actually treated as -2
            item.useTime = 19;

            item.width = 20;
            item.height = 20;
            item.damage = 15;
            item.knockBack = 2.5f;
            item.UseSound = SoundID.Item7;

            item.shoot = mod.ProjectileType<Projectiles.SpiritBlast>();
            item.shootSpeed = 8f;

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
            recipe.AddIngredient(ItemID.LeadBar, 2);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (Fist.ExpendCombo(player, true) > 0)
            {
                if (player.itemTime > 0) player.itemTime = 0;
                HeliosphereEmblem.DustVisuals(player, 20, 0.9f);
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return Fist.ExpendCombo(player) > 0;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            damage *= 2;
            knockBack *= 0.25f;
            return player.altFunctionUse > 0;
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 20, 8f, 8f, 8f);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
            if (combo != -1)
            {
                if (combo % Fist.punchComboMax == 0)
                {
                    // Ready flash
                    PlayerFX.ItemFlashFX(player);
                }
            }
        }
    }
}
