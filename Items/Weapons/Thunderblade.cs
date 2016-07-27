using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponOut.Items.Weapons
{
    public class Thunderblade : ModItem
    {
        bool reset = false;
        public const int damage = 15;
        public const float knockBack = 5f;
        public const int useAnimation = 15;
        public const int mana = 5;
        public const float shootSpeed = 5f;

        public int damageMod;
        public float knockBackMod;
        public int useAnimationMod;
        public int manaMod;
        public float shootSpeedMod;

        public override void SetDefaults()
        {
            item.name = "Thunderblade";
            item.toolTip = "Right click to cast a magic bolt";
            item.width = 40;
            item.height = 40;
            item.scale = 1.15f;

            item.autoReuse = true;
            //generate a default style
            magicDefaults();
            meleeDefaults(true);

            item.rare = 2;
            item.value = 57500;
        }
        #region Crazy Value Swapping
        private void meleeDefaults(bool keepMagicValues = false)
        {
            item.useStyle = 1; //swing
            item.noMelee = false;

            item.melee = true; //melee damage
            item.damage = damage + damageMod;
            item.knockBack = knockBack + knockBackMod;

            item.useSound = 1;
            item.useAnimation = useAnimation + useAnimationMod;
            item.useTime = item.useAnimation;

            if (!keepMagicValues)
            {
                item.mana = 0;
                item.shoot = 0;
                item.shootSpeed = 0;
            }
        }
        private void magicDefaults()
        {
            item.useStyle = 5; //aim
            Item.staff[item.type] = true; //rotate weapon, as it is a staff
            item.noMelee = true;

            item.magic = true; //magic damage
            item.damage = damage + damageMod + 25;
            item.knockBack = knockBack + knockBack - 3;

            item.useSound = 94;
            item.useAnimation = useAnimation + useAnimationMod + 15;
            item.useTime = item.useAnimation;

            item.mana = mana + manaMod;
            item.shoot = ProjectileID.JestersArrow;
            item.shootSpeed = shootSpeed + shootSpeedMod;
        }
        public override void PostReforge()
        {
            damageMod = item.damage - damage;
            knockBackMod = item.knockBack - knockBack;
            useAnimationMod = item.useAnimation - useAnimation;
            manaMod = item.mana - mana;
            shootSpeedMod = item.shootSpeed - shootSpeed;
        }
        #endregion
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            //polarise the stats to use either component
            if (player.altFunctionUse == 2)
            {
                magicDefaults();
            }
            else
            {
                meleeDefaults();
            }
            reset = true;
            return true;
        }
        public override void HoldItem(Player player)
        {
            //fix time on the frame AFTER firing
            if (player.itemAnimation == player.itemAnimationMax - 2)
            {
                player.itemTime = player.itemAnimation + 1; //itemTime for some reason doesn't take into account melee speed increases
            }

            //set values back to "default style" when not in use
            if (player.itemAnimation == 0 && reset)
            {
                magicDefaults();
                meleeDefaults(true);
            }
        }
        
        public override void UseStyle(Player player)
        {
            Main.NewText("time " + player.itemTime + "| anim " + player.itemAnimation);
            PlayerFX.modifyPlayerItemLocation(player, -6, 0);
        }
    }
}
