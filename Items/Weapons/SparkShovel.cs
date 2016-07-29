using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class SparkShovel : ModItem
    {
        //the main transformation stats, for convenience
        const int damage = 5;
        const int damageM = 8;
        const float knockBack = 3f;
        const float knockBackM = 1f;
        const int sound = 1;
        const int soundM = 8;
        const int useAnimation = 16;
        const int useAnimationM = 28;
        const int useTime = 16;
        const int useTimeM = 28;
        const int mana = 2;
        const int shoot = ProjectileID.Spark;
        const float shootSpeed = 8f;

        private bool reset = false;
        public int damageMod;
        public float knockBackMod;
        public int useAnimationMod;
        public int manaMod;
        public float shootSpeedMod;

        public override void SetDefaults()
        {
            item.name = "Spark Shovel";
            item.toolTip = "Right click to shoot a small spark";
            item.width = 32;
            item.height = 32;

            item.autoReuse = true;
            item.pick = 35;
            //generate a default style, where melee stats take precedent
            magicDefaults();
            Item.staff[item.type] = true; //rotate weapon, as it is a staff
            meleeDefaults(true);

            item.rare = 1;
            item.value = 5400;
        }
        public override void AddRecipes()
        {
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.CopperPickaxe, 1);
                }
                else
                {
                    recipe.AddIngredient(ItemID.TinPickaxe, 1);
                }
                recipe.AddIngredient(ItemID.WandofSparking, 1);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public override void UseStyle(Player player)
        {
            if (player.altFunctionUse > 0) PlayerFX.modifyPlayerItemLocation(player, -4, 0);
        }










        #region (type 1 + useTurn) Crazy Value Swapping Transform Stuff (Should probably make this its own class at some point)

        /// <summary>
        /// Here are some standard settings for switching between melee and magic
        /// </summary>
        /// <param name="keepMagicValues"></param>
        private void meleeDefaults(bool keepMagicValues = false)
        {
            item.useStyle = 1; //swing
            item.noMelee = false;
            item.useTurn = true;

            item.melee = true; //melee damage
            item.magic = false;
            item.damage = damage + damageMod;
            item.knockBack = knockBack + knockBackMod;

            item.useSound = sound;
            item.useAnimation = useAnimation + useAnimationMod;
            item.useTime = useTime + useAnimationMod;

            if (!keepMagicValues)
            {
                item.mana = 0;
                item.shoot = 0;
                item.shootSpeed = 0;
            }
        }
        /// <summary>
        /// The general design has the magic aspect being more powerful but slower/less frequent
        /// </summary>
        private void magicDefaults()
        {
            item.useStyle = 5; //aim
            item.noMelee = true;
            item.useTurn = false;

            item.magic = true; //magic damage
            item.melee = false;
            item.damage = damageM + damageMod;
            item.knockBack = knockBackM + knockBackMod;

            item.useSound = soundM;
            item.useAnimation = useAnimationM + useAnimationMod;
            item.useTime = useTimeM + useAnimationMod;

            item.mana = mana + manaMod;
            item.shoot = shoot;
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
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                magicDefaults();
            }
            else
            {
                meleeDefaults();
            }
            return base.CanUseItem(player);
        }
        public override void HoldItem(Player player)
        {
            if (player.itemAnimation == 0)
            {
                magicDefaults();
                meleeDefaults(true);
                reset = false;
            }
        }
        #endregion

    }
}
