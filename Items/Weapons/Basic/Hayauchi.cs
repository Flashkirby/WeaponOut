using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace WeaponOut.Items.Weapons.Basic
{
    /// <summary>
    /// Stand (mostly) still to charge a slash, messes with hitboxes etc.
    /// drawstrike does quad damage, with added crit for a total of x8
    /// 35 * 8 == 280
    /// Draw Strike speed = 80 + 20 + 15 == 115
    /// Draw Strike DPS = 146
    /// hey, its me, jetstream sammy
    /// </summary>
    public class Hayauchi : ModItem
    {
        public const int waitTime = 80; //charge for special attack, due to coding must be >60 since that's the charge time
        private const int extraSwingTime = 15; //additional special attack time
        
        private bool drawStrike;
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hayauchi");
            DisplayName.AddTranslation(GameCulture.Chinese, "快打");
			DisplayName.AddTranslation(GameCulture.Russian, "Хаяуси");
            
            Tooltip.SetDefault(
                "'Focus, steel thyself'\n" +
                "'Wait for the perfect moment'\n" +
                "'A decisive blow'");
            Tooltip.AddTranslation(GameCulture.Chinese, "“聚集力量”\n“等待时机”\n“一击定音”");
			Tooltip.AddTranslation(GameCulture.Russian,
				"'Сосредоточься'\n" +
                "'Выбери момент'\n" +
                "'Точный удар'");
        }
        public override void SetDefaults()
        {
            item.width = 46;
            item.height = 46;

            item.melee = true;
            item.damage = 35; //DPS 105
            item.knockBack = 3;
            item.autoReuse = true;
            item.useTurn = true;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.useTime = 20;
            item.useAnimation = 20;

            item.rare = 5;
            item.value = 25000;

            drawStrike = false; 
        }
        public override void AddRecipes() {
            if (!ModConf.EnableBasicContent) return;
            for (int i = 0; i < 2; i++)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.Katana, 1);
                if (i == 0)
                {
                    recipe.AddIngredient(ItemID.CobaltBar, 6);
                }
                else
                {
                    recipe.AddIngredient(ItemID.PalladiumBar, 6);
                }
                recipe.AddTile(TileID.MythrilAnvil);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        public int getPatience(Player player)
        {
            return player.GetModPlayer<PlayerFX>(mod).itemSkillDelay;
        }
        public void updatePatience(Player player, int valueEquals)
        {
            player.GetModPlayer<PlayerFX>(mod).itemSkillDelay = valueEquals;
        }

        public override void HoldItem(Player player) //called when holding but not swinging
        {
            int patience = getPatience(player);
            int sheenTime = 60;

            //standing still and not grappling, to lower wait
            if (System.Math.Abs(player.velocity.X) < 1f && player.velocity.Y == 0f
                && player.grapCount == 0
                && !player.pulley
                && !(player.frozen || player.webbed || player.stoned || player.noItems)
                && player.itemAnimation == 0)
            {
                if (patience < waitTime + 1)
                {
                    patience++;
                    updatePatience(player, patience);
                }
            }
            else
            {
                patience = 0;
                updatePatience(player, patience);
            }

            if (player.itemAnimation == 0)//at the end of a strike
            {
                drawStrike = false; //reset strike at end
                item.useStyle = 1; //set back to swing
            }

            if (patience >= waitTime) //blade glow
            {
                int random = Main.rand.Next(60);
                Vector2 dustPos = player.Center;
                if (player.stealth > 0)
                {
                    dustPos.X += (
                        ((sheenTime / 2) - random) * 0.55f - 12)
                        * player.direction - 4;
                    dustPos.Y -= (
                        ((sheenTime / 2) - random) * 0.29f - 12)
                        * player.gravDir;

                    if (player.gravDir < 0) dustPos.Y -= 6;
                    int d = Dust.NewDust(dustPos, 1, 1, 90, 0f, 0f, 0, Color.White, 0.2f);
                    Main.dust[d].velocity = Vector2.Zero;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].fadeIn = 0.8f;
                }
                if (patience == waitTime)//burst
                {
                    dustPos = player.Center;
                    dustPos.X += (
                        (sheenTime / 2) * 0.55f - 12)
                        * player.direction - 4;
                    dustPos.Y -= (
                        (sheenTime / 2) * 0.29f - 12)
                        * player.gravDir;

                    if (player.gravDir < 0) dustPos.Y -= 6;
                    Main.PlaySound(25, player.position);
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDust(dustPos, player.direction, 1, 130, player.direction, 0f, 0, Color.White, 0.6f);
                    }
                }
            }
            else if (patience > waitTime - sheenTime) //blade sheen
            {
                Vector2 dustPos = player.Center;
                dustPos.X += (
                    (patience + (sheenTime / 2) - waitTime) * 0.55f - 12)
                    * player.direction - 4;
                dustPos.Y -= (
                    (patience + (sheenTime / 2) - waitTime) * 0.29f - 12)
                    * player.gravDir;

                if (player.gravDir < 0) dustPos.Y -= 6;

                int d = Dust.NewDust(dustPos, 1, 1, 71, 0f, 0f, 0, Color.White, 0.5f);
                Main.dust[d].velocity *= 5f / (patience + 6);
            }

        }
        public override bool HoldItemFrame(Player player) //called on player holding but not swinging
        {
            if (getPatience(player) >= waitTime) //ready to slash
            {
                player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                return true;
            }
            return false;
        }

        //Allows you to modify the location and rotation of this item in its use animation. Does get called in multiplayer
        public override void UseStyle(Player player)
        {
            if (getPatience(player) >= waitTime) //on the frame when first swung
            {
                drawStrike = true; //set up super strike
                player.itemAnimation += extraSwingTime; //a bit longer
                item.useStyle = 0; //allow for overwriting style (UseItemFrame)
                if (player.whoAmI == Main.myPlayer) //visual effect as a projectile (which consequently also makes light)
                {
                    Projectile.NewProjectile(
                        player.position.X,
                        player.position.Y, 0, 0,
                        mod.ProjectileType<Projectiles.Hayauchi>(),
                        player.direction, 0, player.whoAmI);
                }
                Main.PlaySound(2, player.position, 71); //SHWING!
            }
        }
        //Allows you to modify the player's animation when this item is being used. Return true if you modify the player's animation. Returns false by default.
        public override bool UseItemFrame(Player player)
        {
            if (drawStrike)
            {
                //counts down from 1 to 0, animation is basically sword swipe in reverse
                float anim = player.itemAnimation / (float)(player.itemAnimationMax + extraSwingTime);
                if (anim > 0.9f)
                {
                    player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                }
                else if (anim > 0.8f)
                {
                    player.bodyFrame.Y = 3 * player.bodyFrame.Height;
                }
                else if (anim > 0.7f)
                {
                    //dust effect at tip of strike
                    Vector2 dustPos = player.Center;
                    dustPos.X += -12 * player.direction - 4;
                    dustPos.Y -= 16 * player.gravDir;
                    if (player.gravDir < 0) dustPos.Y -= 6;
                    for (int i = 0; i < 2; i++)
                    {
                        Dust.NewDust(dustPos, player.direction, 1, 130, -player.direction, -player.gravDir * 2, 0, Color.White, 0.6f);
                    }
                    player.bodyFrame.Y = 2 * player.bodyFrame.Height;
                }
                else if (anim > 0.4f)
                {
                    // put hand down
                    player.bodyFrame.Y = 1 * player.bodyFrame.Height;
                }
                else
                {
                    // hand on sheath
                    player.bodyFrame.Y = 4 * player.bodyFrame.Height;
                }
            }
            return true;
            //return false;
        }
        //Changes the hitbox of this melee weapon when it is used.
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (drawStrike)
            {
                float anim = player.itemAnimation / (float)(player.itemAnimationMax + extraSwingTime);
                //hitbox only occurs between 0.8 - 0.5
                if (anim <= 0.9f && anim > 0.7f)
                {
                    hitbox.Width = 228;
                    hitbox.Height = 142;
                    hitbox.X = (int)player.Center.X - 50 * player.direction;
                    if (player.gravDir > 0) { hitbox.Y = (int)player.Bottom.Y + 16 - hitbox.Height; }
                    else { hitbox.Y = (int)player.Top.Y - 16; }
                    if (player.direction < 0) hitbox.X -= hitbox.Width;

                    player.attackCD = 0;
                }
                else
                {
                    noHitbox = true;
                }
            }
        }

        //x6 damage + crit to make up for terrible (but cool) usage
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (!drawStrike) return;
            damage *= 6;
            knockBack *= 2;
            crit = true;
        }
        public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
        {
            if (!drawStrike) return;
            damage *= 6;
            crit = true;
        }

    }
}
