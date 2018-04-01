using Terraria;
using Terraria.ModLoader;

namespace WeaponOut.Buffs
{
    public class SpiritGuardian : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Starlight Guardian");
            Description.SetDefault("The starlight guardian will assist you");
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            int guardianID = mod.ProjectileType<Projectiles.SpiritGuardian>();
            player.GetModPlayer<PlayerFX>().starlightGuardian = player.ownedProjectileCounts[guardianID] > 0;
        }
    }
}
