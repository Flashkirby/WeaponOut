using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using WeaponOut.Items.Weapons.UseStyles;

namespace WeaponOut.Items.Weapons.Fists
{
    public class KnucklesMeteor : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            equips.Add(EquipType.HandsOn);
            equips.Add(EquipType.HandsOff);
            return ModConf.enableFists;
        }

        private FistStyle fist;
        public FistStyle Fist
        {
            get
            {
                if (fist == null)
                {
                    fist = new FistStyle(item, 4);
                }
                return fist;
            }
        }
        public override void SetDefaults()
        {
            item.name = "Comet Fu";
            item.toolTip = "<right> at full combo power to unleash meteors";
            item.toolTip2 = "'Space CQC is explicitly stated to be whatever you claim it to be'"; // これは宇宙CQC! but no jpn support, rip MPT
            item.useStyle = FistStyle.useStyle;
            item.autoReuse = true;
            item.useAnimation = 19;
            item.useTime = 19;

            item.width = 20;
            item.height = 20;
            item.damage = 15;
            item.knockBack = 2.5f;
            item.UseSound = SoundID.Item7;

            item.shoot = mod.ProjectileType<Projectiles.SpiritComet>();
            item.shootSpeed = 6f;

            item.value = Item.sellPrice(0, 0, 24, 0);
            item.rare = 2;
            item.noUseGraphic = true;
            item.melee = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Fist.ModifyTooltips(tooltips, mod);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void HoldItem(Player player)
        {
            if (Fist.ExpendCombo(player, true) > 0)
            {
                if (player.itemTime > 0) player.itemTime = 0;
                HeliosphereEmblem.DustVisuals(player, 174, 0.9f);
            }
        }

        int charges;
        public override bool AltFunctionUse(Player player)
        {
            if (player.itemTime == 0)
            {
                charges = Fist.ExpendCombo(player);
            }
            return charges > 0;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if(player.altFunctionUse > 0)
            {
                damage *= 2;
                knockBack *= 2f;
                
                if(charges > 0)
                {
                    for(int i = 0; i < System.Math.Min(3, charges); i++)
                    {
                        Projectile.NewProjectile(position, 
                            new Vector2(
                                speedX + i * Main.rand.NextFloatDirection(),
                                speedY + i * Main.rand.NextFloatDirection()),
                            type, damage, knockBack, player.whoAmI);
                    }
                    charges = 0;
                }
            }
            return false;
        }

        public override bool UseItemFrame(Player player)
        {
            Fist.UseItemFrame(player);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = Fist.UseItemHitbox(player, ref hitbox, 20, 9f, 8f, 8f);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int combo = Fist.OnHitNPC(player, target, true);
            if (combo != -1)
            {
                if (combo % Fist.punchComboMax == 0)
                {
                    // Ready flash
                    PlayerFX.ItemFlashFX(player);
                }
            }
        }
    }
}
