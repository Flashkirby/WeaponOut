using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    public class Capacitor : ModItem
    {
        //minimum transform stats, usually the melee one
        const int damage = 17;
        const int damageM = 30;
        const float knockBack = 4f;
        const float knockBackM = 0f;
        const int sound = 1;
        const int soundM = 0;
        const int useAnimation = 21;
        const int useAnimationM = 15;
        const int useTime = 21;
        const int useTimeM = 15;
        const int mana = 16;
        const int shoot = ProjectileID.FrostBoltStaff;
        const float shootSpeed = 14f;

        public int damageMod;
        public float knockBackMod;
        public int useAnimationMod;
        public int manaMod;
        public float shootSpeedMod;

        public override void SetDefaults()
        {
            item.name = "Capacitor";
            item.toolTip = "Right click to cast a frost bolt\nMelee attacks grant 80% reduced mana cost";
            item.width = 40;
            item.height = 40;
            item.scale = 1.15f;

            item.autoReuse = true;
            //generate a default style, where melee stats take precedent
            magicDefaults();
            Item.staff[item.type] = true; //rotate weapon, as it is a staff
            meleeDefaults(true);

            item.rare = 2;
            item.value = 20000;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IceBlade, 1);
            recipe.AddIngredient(ItemID.FallenStar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            player.AddBuff(mod.BuffType("ManaReduction"), 180);//3 second buff
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            if (player.HasBuff(mod.BuffType("ManaReduction")) != -1)
            {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 15, (player.velocity.X * 0.2f) + (player.direction * 3), player.velocity.Y * 0.2f, 100, Color.White, 1.3f);
                Main.dust[d].noGravity = true;
            }
            else
            {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 15, (player.velocity.X * 0.2f) + (player.direction * 3), player.velocity.Y * 0.2f, 100, Color.White, 0.6f);
                Main.dust[d].noGravity = true;
            }
        }
        
        public override void UseStyle(Player player)
        {
            if(player.altFunctionUse == 2) PlayerFX.modifyPlayerItemLocation(player, -6, 0);
        }







        #region (type 1) Crazy Value Swapping Transform Stuff (Should probably make this its own class at some point)

        /// <summary>
        /// Here are some standard settings for switching between melee and magic
        /// </summary>
        /// <param name="keepMagicValues"></param>
        private void meleeDefaults(bool keepMagicValues = false)
        {
            item.useStyle = 1; //swing
            item.noMelee = false;

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
            if (player.altFunctionUse > 0)
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
            }
        }
        #endregion

    }
}
