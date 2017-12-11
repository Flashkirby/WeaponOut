using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class GolemHeart : ModItem
    {
        private const int chargeTime = 60;
        private const int chargeTimeConBonus = 30;
        private int chargeTick = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Spark");
            Tooltip.SetDefault(
                "Reduces combo power cost by 2\n" +
                "Hold DOWN when not attacking to charge up to 10 combo power");
        }
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.rare = 8;
            item.accessory = true;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ModPlayerFists mpf = ModPlayerFists.Get(player);
            mpf.comboCounterMaxBonus -= 2;
            if (player.controlDown && player.itemAnimation == 0 &&
                mpf.ComboCounter < 10 && player.HeldItem.melee) {
                chargeTick++;
                if (chargeTick > chargeTime) {
                    chargeTick = chargeTimeConBonus;
                    mpf.ModifyComboCounter(1);
                    Main.PlaySound(SoundID.Item34.WithVolume(0.5f));
                }

                double angle = Main.rand.NextFloat() * Math.PI * 2;
                Vector2 velo = new Vector2((float)(7.0 * Math.Sin(angle)), (float)(5.0 * Math.Cos(angle)));
                Dust d = Dust.NewDustPerfect(player.Center, 88, velo);
                d.position -= d.velocity * 10;
                d.noGravity = true;
                d.scale = 0.1f;
                d.fadeIn = 1f;
                Main.playerDrawDust.Add(d.dustIndex);
            }
            else { chargeTick = 0; }
        }
    }
}
