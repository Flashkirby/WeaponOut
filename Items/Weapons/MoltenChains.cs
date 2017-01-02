using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Hellstone tier, about sun fury DPS
	/// Resprite by Eli10293
    /// </summary>
    public class MoltenChains : ModItem
    {
        public static short customGlowMask = 0;

        /// <summary>
        /// Generate a completely legit glowmask ;)
        /// </summary>
        public override bool Autoload(ref string name, ref string texture, System.Collections.Generic.IList<EquipType> equips)
        {
            if (Main.netMode != 2 && ModConf.enableWhips)
            {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++)
                {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = mod.GetTexture("Glow/" + this.GetType().Name + "_Glow");
                customGlowMask = (short)(glowMasks.Length - 1);
                Main.glowMaskTexture = glowMasks;
                return true;
            }
            return false;
        }

        private bool increaseDamage;
        public override void SetDefaults()
        {
            item.name = "Molten Chains";
			item.width = 34;
			item.height = 34;

            item.useStyle = 5;
            item.useAnimation = 18;
            item.useTime = 18;
            item.UseSound = SoundID.Item19;
            item.noUseGraphic = true;
            item.noMelee = true;
            
            item.melee = true;
            item.channel = true;
            item.damage = 19;
            item.crit = 16; //crit chance on whips increase crit damage instead
            item.knockBack = 4f;
            item.shoot = mod.ProjectileType(this.GetType().Name);
            item.shootSpeed = 1f; //projectile length

            item.glowMask = customGlowMask;
            item.rare = 3;
            item.value = Item.sellPrice(0,0,54,0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.AddIngredient(ItemID.Chain, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {

            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0, 
                Main.rand.Next(-100, 100) * 0.001f * player.gravDir); //whip swinging
            return false;
        }
    }
}