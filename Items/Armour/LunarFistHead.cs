using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace WeaponOut.Items.Armour
{
    [AutoloadEquip(EquipType.Head)]
    public class LunarFistHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starlight Circlet");

            Tooltip.SetDefault(
                "15% increased minion damage, 13% increased melee critical strike chance\n" +
                "Fighting bosses slowly empowers next melee attack, up to 2500%\n" +
                "Increases your max number of minions by 1");

            ModTranslation text;

            text = mod.CreateTranslation("LunarFistHeadBonus");
            text.SetDefault("Double tap $BUTTON to switch your guardian's stance");
            mod.AddTranslation(text);
        }
        public override void SetDefaults()
        {
            item.defense = 16;
            item.value = 0;
            item.rare = 10;

            item.width = 18;
            item.height = 18;
        }
        public override void AddRecipes()
        {
            if (!ModConf.EnableFists) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FragmentStardust, 4);
            recipe.AddIngredient(ItemID.FragmentSolar, 4);
            recipe.AddIngredient(ItemID.LunarBar, 6);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.minionDamage += 0.15f;
            player.meleeCrit += 13;
            player.maxMinions += 1;
            player.GetModPlayer<PlayerFX>().patienceDamage = 25f; // Can do up to 2500%
            player.GetModPlayer<PlayerFX>().patienceBuildUpModifier += 0.4f; // 75->105%

            Lighting.AddLight(player.Center + player.velocity, new Vector3(0.9f, 0.9f, 0.95f));
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair) { drawHair = true; }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == mod.ItemType<LunarFistHead>()
                && body.type == mod.ItemType<LunarFistBody>()
                && legs.type == mod.ItemType<LunarFistLegs>();
        }

        public override void UpdateArmorSet(Player player)
        {
            string button = Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");
            player.setBonus = WeaponOut.GetTranslationTextValue("LunarFistHeadBonus").Replace("$BUTTON", button);

            if (player.whoAmI == Main.myPlayer)
            {
                // Based on Stardust Guardian spawn
                int buffID = mod.BuffType<Buffs.SpiritGuardian>();
                int guardianID = mod.ProjectileType<Projectiles.SpiritGuardian>();
                if (player.FindBuffIndex(buffID) == -1) player.AddBuff(buffID, 1, true);
                if (player.ownedProjectileCounts[guardianID] < 1) // No guardian? spawn one
                    Projectile.NewProjectile(
                        player.Center.X,
                        player.Center.Y,
                        0.0f, -1f, guardianID, 0, 0.0f, Main.myPlayer, 0.0f, 0.0f);
            }
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
            player.armorEffectDrawOutlinesForbidden = true;
        }

        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
        {
            color = Color.White
                * (1f - shadow)
                * ((byte.MaxValue - drawPlayer.immuneAlpha) / (float)byte.MaxValue);
        }
    }
}