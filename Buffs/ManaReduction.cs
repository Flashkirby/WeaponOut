using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Buffs
{
    public class ManaReduction : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Mana Cost Reduced");
            DisplayName.AddTranslation(GameCulture.Chinese, "魔力消耗降低");

            Description.SetDefault("Next magic attack has mana cost reduced by 80%");
            Description.AddTranslation(GameCulture.Chinese, "下一次的魔法攻击将减少80%的魔力消耗");
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.manaCost -= 0.8f;
            if (player.itemAnimation == player.itemAnimationMax - 1
                && player.inventory[player.selectedItem].magic)
            {
                player.DelBuff(buffIndex);
            }
        }
    }
}
