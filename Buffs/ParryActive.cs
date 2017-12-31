using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class ParryActive : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Parrying Moment");
            DisplayName.AddTranslation(GameCulture.Chinese, "闪避之时");

            Description.SetDefault("Your next parried punch is empowered!");
            Description.AddTranslation(GameCulture.Chinese, "你下一次的闪避拳击增强！");
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ModPlayerFists>().parryBuff = true;
            player.meleeCrit += 50;
        }
    }
}
