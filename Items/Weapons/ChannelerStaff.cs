using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{

    /// <summary>
    /// A team support item that doubles your mana to an ally
    /// </summary>
    public class ChannelerStaff : ModItem
    {
        const bool devTest = true;
        public static Vector2 mouse
        {
            get
            {
                return new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
            }
        }

        public static short customGlowMask = 0;

        //minimum transform stats, usually the melee one
        const int damage = 17;
        const int damageM = 4;
        const float knockBack = 4f;
        const float knockBackM = 4f;
        const int sound = 1;
        const int soundM = 9;
        const int useAnimation = 24;
        const int useAnimationM = 14;
        const int useTime = 24;
        const int useTimeM = 4;
        const int mana = 8;
        const float shootSpeed = 2f;

        public int damageMod;
        public float knockBackMod;
        public int useAnimationMod;
        public int manaMod;
        public float shootSpeedMod;

        /// <summary>
        /// Generate a completely legit glowmask ;)
        /// </summary>
        public override bool Autoload(ref string name, ref string texture, System.Collections.Generic.IList<EquipType> equips)
        {
            if (Main.netMode != 2)
            {
                Texture2D[] glowMasks = new Texture2D[Main.glowMaskTexture.Length + 1];
                for (int i = 0; i < Main.glowMaskTexture.Length; i++)
                {
                    glowMasks[i] = Main.glowMaskTexture[i];
                }
                glowMasks[glowMasks.Length - 1] = mod.GetTexture("Glow/" + this.GetType().Name + "_Glow");
                customGlowMask = (short)(glowMasks.Length - 1);
                Main.glowMaskTexture = glowMasks;
            }
            return base.Autoload(ref name, ref texture, equips);
        }
        public override void SetDefaults()
        {
            item.name = "Channeler Staff";
            item.toolTip = @"Greatly increases mana regen when held
Right click to cast a mana restoring ray to players on your team
Ray also increases magic damage";
            item.width = 42;
            item.height = 42;

            //generate a default style, where melee stats take precedent
            magicDefaults();
            Item.staff[item.type] = true; //rotate weapon, as it is a staff
            meleeDefaults(true);

            item.glowMask = customGlowMask;
            item.rare = 3;
            item.value = Item.sellPrice(0, 1, 0, 0);
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation == 0)
            {
                magicDefaults();
                meleeDefaults(true);
            }
            //basic mana regen behaviour addon (increases at count>=120)
            if (player.manaRegenDelay <= 0)
            {
                player.manaRegenCount += 50;
            }

            Lighting.AddLight(player.Center, 0.1f, 0.3f, 0.3f);
            //Show link to team mate from cursor
            Player p = getPlayerTeamAtCursor(player);
            if (p != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dist = p.Center - mouse;
                    int d = Dust.NewDust(
                        mouse + dist * Main.rand.NextFloat(),
                        1, 1, 175, dist.X * 0.4f, dist.Y * 0.4f, 255, Main.teamColor[player.team], 0.75f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.1f;
                }
            }
        }

        public override void UseStyle(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                PlayerFX.modifyPlayerItemLocation(player, -14, 0);
                Lighting.AddLight(player.Center, 0.2f, 0.6f, 0.6f);
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Player p = getPlayerTeamAtCursor(player);
            if (p != null)
            {
                Projectile.NewProjectile(position.X + speedX * 15, position.Y + speedY * 15,
                    speedX, speedY, type, damage, knockBack, Main.myPlayer, p.whoAmI);
            }
            return false;
        }
        private Player getPlayerTeamAtCursor(Player player)
        {
            if (player.whoAmI == Main.myPlayer && (player.team != 0 || devTest))
            {
                foreach (Player p in Main.player)
                {
                    if (!p.active || p.dead) continue;
                    if (p.team == player.team && (p.whoAmI != Main.myPlayer || devTest))
                    {
                        if (Math.Abs(mouse.X - p.Center.X) < 80 &&
                            Math.Abs(mouse.Y - p.Center.Y) < 80)
                        {
                            return p;
                        }
                    }
                }
            }
            return null;
        }












        #region (type 1 - custom defaults) Crazy Value Swapping Transform Stuff (Should probably make this its own class at some point)

        /// <summary>
        /// Here are some standard settings for switching between melee and magic
        /// </summary>
        /// <param name="keepMagicValues"></param>
        private void meleeDefaults(bool keepMagicValues = false)
        {
            item.noMelee = false;
            item.knockBack = knockBack + knockBackMod;

            item.useSound = sound;
            item.useAnimation = useAnimation + useAnimationMod;
            item.useTime = useTime + useAnimationMod;

            if (!keepMagicValues)
            {
                item.useStyle = 1; //swing

                item.melee = true; //melee damage
                item.magic = false;
                item.damage = damage + damageMod;

                item.mana = 0;
                item.shoot = 0;
                item.shootSpeed = 0;
            }
        }
        /// <summary>
        /// The general design has the magic aspect being more powerful but slower/less frequent
        /// </summary>
        private void magicDefaults()
        {
            item.useStyle = 5; //aim
            item.noMelee = true;

            item.magic = true; //magic damage
            item.melee = false;
            item.damage = damageM + damageMod;
            item.knockBack = knockBackM + knockBackMod;

            item.useSound = soundM;
            item.useAnimation = useAnimationM + useAnimationMod;
            item.useTime = useTimeM;

            item.mana = mana + manaMod;
            item.shoot = mod.ProjectileType("ManaRestoreBeam");
            item.shootSpeed = shootSpeed + shootSpeedMod;
        }
        public override void PostReforge()
        {
            damageMod = item.damage - damageM;
            knockBackMod = item.knockBack - knockBack;
            useAnimationMod = item.useAnimation - useAnimation;
            manaMod = item.mana - mana;
            shootSpeedMod = item.shootSpeed - shootSpeed;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse > 0)
            {
                if (getPlayerTeamAtCursor(player) == null) return false;
                magicDefaults();
            }
            else
            {
                meleeDefaults();
            }
            return base.CanUseItem(player);
        }
        #endregion

    }
}
