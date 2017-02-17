using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class Knuckles : ModItem
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
            item.name = "Knuckleduster";
            item.toolTip = "<right> at full combo power to unleash spirit";
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.useAnimation = 19;//actually treated as -2
            item.useTime = 19;

            item.width = 28;
            item.height = 28;
            item.damage = 5;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item7;

            item.shoot = mod.ProjectileType<Projectiles.SpiritBlast>();
            item.shootSpeed = 10f;

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
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.anyIronBar = true;
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (Fist.ExpendCombo(player, true) > 0)
            { HeliosphereEmblem.DustVisuals(player, 20, 0.9f); }
        }

        public override bool AltFunctionUse(Player player)
        {
            return player.itemAnimation <= (item.autoReuse ? 1 : 0) && Fist.ExpendCombo(player) > 0;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            damage *= 2;
            knockBack *= 0.25f;
            return player.altFunctionUse > 0;
        }

        public override bool UseItemFrame(Player player)
        {
            FistStyle.UseItemFrame(player);
            Fist.UseItemFrameComboStop(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = FistStyle.UseItemHitbox(player, ref hitbox, 20);
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
