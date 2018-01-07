using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

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
            DisplayName.AddTranslation(GameCulture.Chinese, "太阳火花石");
            DisplayName.AddTranslation(GameCulture.Russian, "Искра Солнца");

            Tooltip.SetDefault(
                "Reduces combo power cost by 2\n" +
                "Hold UP when not attacking to charge up to 10 combo power");
            Tooltip.AddTranslation(GameCulture.Chinese, "减少2点连击能量消耗\n不攻击时按住上方向键可以积攒最高10点的连击能量");
			Tooltip.AddTranslation(GameCulture.Russian,
				"-2 стоимость заряда комбо\n" +
                "Не атакуя зажмите ВВЕРХ, чтобы набрать заряд комбо до 10");

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
            if (player.controlUp && player.itemAnimation == 0 &&
                mpf.ComboCounter < 10 && 
                (player.HeldItem.melee || player.HeldItem.useStyle == ModPlayerFists.useStyle)) {
                chargeTick++;
                if (chargeTick > chargeTime) {
                    chargeTick = chargeTimeConBonus;
                    mpf.ModifyComboCounter(1);
                    Main.PlaySound(SoundID.Item34.WithVolume(0.5f));
                }

                double angle = Main.rand.NextFloat() * Math.PI * 2;
                Vector2 velo = new Vector2((float)(10.0 * Math.Sin(angle)), (float)(10.0 * Math.Cos(angle)));
                Dust d = Dust.NewDustPerfect(player.Center, Main.rand.Next(2) == 0 ? 88 : 262, velo);
                d.position -= d.velocity * 5f;
                d.noGravity = true;
                d.scale = 0.55f;
                d.velocity += player.velocity;
            }
            else { chargeTick = 0; }
        }
    }
}
