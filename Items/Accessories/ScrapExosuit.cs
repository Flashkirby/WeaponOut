using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class ScrapExosuit : ModItem
    {
        public override bool Autoload(ref string name) { return ModConf.enableFists; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Exosuit");
            Tooltip.SetDefault(
                "Greatly reduces cooldown between dashes\n" +
                "Consuming combo will restore life\n" + 
                "Parrying with fists will steal life\n" + 
                "Greatly increases life regen when moving\n" + 
                "'Harness science!'");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.defense = 8;
            item.rare = 6;
            item.accessory = true;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.expert = true;
        }

        private int localCounter = 0;
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>();

            mpf.parryLifesteal += 0.1f;

            if (player.dashDelay > 2) player.dashDelay -= 2;

            if (Math.Abs(player.velocity.X) > 4.5f)
            { player.lifeRegenCount += 8; } // healing per 2 seconds
            else if (Math.Abs(player.velocity.X) > 3f)
            { player.lifeRegenCount += 6; } // healing per 2 seconds
            else if (Math.Abs(player.velocity.X) > 1.5f)
            { player.lifeRegenCount += 4; } // healing per 2 seconds
            
            int diff = localCounter - mpf.ComboCounter;
            if (diff > 0 && mpf.comboTimer > 0)
            {
                PlayerFX.HealPlayer(player, 2 * diff, true);
            }
            localCounter = mpf.ComboCounter;

            if (!hideVisual)
            {
                Lighting.AddLight(player.Center, 1f, 0.2f, 0f);
            }
        }
    }
}
