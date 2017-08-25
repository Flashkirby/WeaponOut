using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class ParryActive : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            return ModConf.enableFists;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Parrying Moment");
            Description.SetDefault("Your next parried punch is empowered!");
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ModPlayerFists>().parryBuff = true;
            player.meleeCrit += 50;
        }
    }
}
