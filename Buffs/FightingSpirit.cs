using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WeaponOut.Buffs
{
    public class FightingSpirit : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableFists;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Puglist's Resolve");
            Description.SetDefault("Heals LIFE% life at the end of a combo");
        }

        public int lifeStored = 0;
        public int lastLife = 0;
        public bool startRecording = false;

        public int LifeRestorable(Player player)
        { return Math.Min((int)(player.statLifeMax2 * 0.25f), lifeStored); }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.dead)
            {
                lifeStored = 0;
                lastLife = 0;
                startRecording = false;
                return;
            }

            ModPlayerFists mpf = player.GetModPlayer<ModPlayerFists>(mod);
            if (mpf.ComboCounter > 0)
            {
                // Set time to be visible
                player.buffTime[buffIndex] = mpf.comboTimerMax - mpf.comboTimer;

                if (!startRecording)
                {
                    lastLife = player.statLife;
                    startRecording = true;
                }

                // Lost health? Records lost amount
                if (player.statLife < lastLife)
                { lifeStored += lastLife - player.statLife; }

                lastLife = player.statLife;

                lifeStored = LifeRestorable(player);
            }
            else
            {
                if (mpf.ComboFinishedFrame > 0)
                {
                    if (!player.moonLeech && lifeStored > 0)
                    {
                        Main.PlaySound(2, -1, -1, 4, 0.3f, 0.2f); // mini heal effect
                        player.HealEffect(lifeStored, false);
                        player.statLife += lifeStored;
                        player.statLife = Math.Min(player.statLife, player.statLifeMax2);
                        NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, player.whoAmI, lifeStored);
                    }
                }

                startRecording = false;
                lifeStored = 0;
            }
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            tip = tip.Replace("LIFE%", "" + lifeStored);
        }
    }
}
