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
            item.toolTip = "100% chance not to consume ammo at full combo power";
            item.useStyle = FistStyle.useStyle;
            item.useTurn = false;
            item.useAnimation = 20;
            item.useTime = 20;

            item.width = 20;
            item.height = 20;
            item.damage = 10;
            item.knockBack = 3f;
            item.UseSound = SoundID.Item11;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 5f;

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
            recipe.AddIngredient(ItemID.FlintlockPistol, 1);
            recipe.AddIngredient(ItemID.IronBar, 3);
            recipe.anyIronBar = true;
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        
        public override bool ConsumeAmmo(Player player)
        {
            return !Fist.IsFullCombo(player);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            speedX += Main.rand.NextFloatDirection() * 0.5f;
            speedY += Main.rand.NextFloatDirection() * 0.5f;
            knockBack = 0f; // Flintlock pistol has no knockback
            return true;
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
