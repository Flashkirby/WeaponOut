using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons
{
    public class KnucklesFlintlock : ModItem
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
                    fist = new FistStyle(item, 4);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Flintknuckle";
            item.toolTip = "<right> at full combo power to fire forceful shots";
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.useAnimation = 20;
            item.useTime = 20;

            item.width = 28;
            item.height = 28;
            item.damage = 13;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item11;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 6f;

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 1;
            item.noUseGraphic = true;
            item.ranged = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Fist.ModifyTooltips(tooltips, mod);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (Fist.ExpendCombo(player, true) > 0)
            {
                HeliosphereEmblem.DustVisuals(player, 20, 0.9f);
            }
        }
        
        public override bool AltFunctionUse(Player player)
        {
            return Fist.ExpendCombo(player) > 0;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.altFunctionUse > 0)
            {
                damage *= 2;
                knockBack *= 2;
            }
            return true;
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
