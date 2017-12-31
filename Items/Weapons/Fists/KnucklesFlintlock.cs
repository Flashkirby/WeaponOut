using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Fists
{
    [AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
    public class KnucklesFlintlock : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flintlock Duster");
            DisplayName.AddTranslation(GameCulture.Chinese, "指虎燧发枪");

            Tooltip.SetDefault(
                "<right> consumes combo instead of ammo\n" +
                "Combo grants 50% increased bullet damage");
            Tooltip.AddTranslation(GameCulture.Chinese, "鼠标右键消耗连击能量发射子弹，而不是使用弹药\n连击将增加50%弹药伤害");
        }
        public override void SetDefaults()
        {
            item.ranged = true;
            item.damage = 10;
            item.useAnimation = 18; // Combos can increase speed by 30-50% since it halves remaining attack time
            item.useTime = 18;
            item.knockBack = 4f;
            item.tileBoost = 3; // For fists, we read this as the combo power

            item.useAmmo = AmmoID.None;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 5f;

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 1;

            item.UseSound = SoundID.Item11;
            item.useStyle = ModPlayerFists.useStyle;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
        }
        public override void AddRecipes() {            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FlintlockPistol, 1);
            recipe.AddIngredient(ItemID.IronBar, 3);
            recipe.anyIronBar = true;
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        { ModPlayerFists.ModifyTooltips(tooltips, item); }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {   // Short dash brings up to max default speed.
                item.useAmmo = AmmoID.None;

                player.GetModPlayer<ModPlayerFists>().
                SetDashOnMovement(5f, 4f, 0.992f, 0.96f, true, 0);
            }
            else
            {
                item.useAmmo = AmmoID.Bullet;
            }
            return true;
        }
        public override bool AltFunctionUse(Player player)
        {
            if (player.HasAmmo(item, true))
            {
                comboFreeConsume = player.GetModPlayer<ModPlayerFists>().
                    AltFunctionCombo(player, 0);
            }
            return true;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // jump exactly 6 blocks high!
            ModPlayerFists.UseItemHitbox(player, ref hitbox, 18, 9f);
        }

        //Combo
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();
            if (mpf.IsComboActive)
            {
                damage += damage / 2;
                knockBack -= item.knockBack;
            }
            return player.altFunctionUse > 0;
        }

        bool comboFreeConsume = false;
        public override bool ConsumeAmmo(Player player)
        {
            return !comboFreeConsume;
        }
    }
}
