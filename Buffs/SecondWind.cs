using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class SecondWind : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Second Wind");
            DisplayName.AddTranslation(GameCulture.Chinese, "重振精力");

            Description.SetDefault("'Perhaps you should see a nurse...'");
            Description.AddTranslation(GameCulture.Chinese, "“也许你应该去看看护士...”");
            Main.debuff[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            PlayerFX pfx = player.GetModPlayer<PlayerFX>();

            player.statLifeMax2 -= pfx.secondWindLifeTax;

            if (player.statLife > 1)
            {
                pfx.secondWindLifeTax -= player.statLife - 1;
                if (pfx.secondWindLifeTax < 0) pfx.secondWindLifeTax = 0;

                if (player.statLife > 5) // Must've healed I guess
                {
                    if (Main.netMode == 1 && player.whoAmI == Main.myPlayer)
                    {
                        NetMessage.SendData(MessageID.PlayerHealth, -1, -1, null, player.whoAmI);
                    }
                }
            }
            player.statLife = 1;
            if (pfx.secondWindLifeTax == 0) player.ClearBuff(Type);
        }
    }
}
