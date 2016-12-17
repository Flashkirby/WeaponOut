using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Vs Vortex Beater
    /// Higher accuracy, similiar DPS at range due to vortex spray
    /// 
    /// </summary>
    public class AllPorpoiseAssaultRifle : ModItem
    {
        public static int proji;
        public static int projii;
        public static int projiii;
        public static int projiv;

        private const int rocketCooldownMax = 90;
        private int rocketCooldown;

        public static short customGlowMask = 0;

        /// <summary>
        /// Generate a completely legit glowmask ;)
        /// </summary>
        public override bool Autoload(ref string name, ref string texture, System.Collections.Generic.IList<EquipType> equips)
        {
            if (Main.netMode != 2)
            {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++)
                {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = mod.GetTexture("Glow/" + this.GetType().Name + "_Glow");
                customGlowMask = (short)(glowMasks.Length - 1);
                Main.glowMaskTexture = glowMasks;
            }
            return base.Autoload(ref name, ref texture, equips);
        }
        HelperDual dual;
        public override void SetDefaults()
        {
            item.name = "All-Porpoise Assault Rifle";
            item.toolTip = "Right click to fire a powerful underbarrel rocket\n50% chance to not consume ammo";
            item.toolTip2 = "'Perfect for target rich environments'";
            item.width = 60;
            item.height = 20;
            item.scale = 1.1f;

            item.UseSound = SoundID.Item99;
            item.useStyle = 5;
            item.useAnimation = 7; //just above insanely fast, because its not really that fast
            item.useTime = 7;
            item.autoReuse = true;

            item.ranged = true;
            item.noMelee = true;
            item.damage = 52;
            item.knockBack = 1f;

            item.useAmmo = AmmoID.Bullet;
            item.shoot = 10;
            item.shootSpeed = 10f;

            item.glowMask = customGlowMask;
            item.rare = 9;
            item.value = Item.sellPrice(0, 1, 0, 0);

            dual = new HelperDual(item, true);
            dual.UseAnimation = 16;
            dual.UseTime = 16;

            dual.Damage = 130; //+base 40
            dual.KnockBack = 4f;

            dual.UseAmmo = AmmoID.Rocket;
            dual.Shoot = 134;
            dual.ShootSpeed = 5.5f;

            proji = mod.GetProjectile("APARocketI").projectile.type;
            projii = mod.GetProjectile("APARocketII").projectile.type;
            projiii = mod.GetProjectile("APARocketIII").projectile.type;
            projiv = mod.GetProjectile("APARocketIV").projectile.type;

            dual.FinishDefaults();
            //end by setting default values

            rocketCooldown = 0;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentVortex, 18);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void OnCraft(Recipe recipe) { HelperDual.OnCraft(item); }

        public override bool AltFunctionUse(Player player) { return rocketCooldown <= 0; }
        public override void UseStyle(Player player)
        {
            dual.UseStyleMultiplayer(player);
            PlayerFX.modifyPlayerItemLocation(player, -22, 0);
        }
        public override bool CanUseItem(Player player)
        {
            dual.CanUseItem(player);
            return base.CanUseItem(player);
        }
        public override void HoldStyle(Player player) { dual.HoldStyle(player); }

        public override void HoldItem(Player player)
        {
            if (rocketCooldown >= 0) rocketCooldown--;
            if (rocketCooldown == 0 && player.whoAmI == Main.myPlayer)
            {
                Main.PlaySound(25, player.Center); //alert player has rocket up
            }
        }

        public override bool ConsumeAmmo(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                if (Main.rand.Next(2) == 0) return false;
            }
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 0)
            {
                speedX += 0.3f * (Main.rand.NextFloat() - 0.5f);
                speedY += 0.3f * (Main.rand.NextFloat() - 0.5f);
            }
            else
            {
                if (rocketCooldown > 0) return false;
                rocketCooldown = rocketCooldownMax;
                damage *= 2; //douvle for direct hits, this halves on explosion in rocket code
                switch (type)
                {
                    case 134: //Rocket I
                        type = proji;
                        break;
                    case 137: //Rocket II
                        type = projii;
                        break;
                    case 140: //Rocket III
                        type = projiii;
                        break;
                    case 143: //Rocket IV
                        type = projiv;
                        break;
                }
            }
            return true;
        }
    }
}
