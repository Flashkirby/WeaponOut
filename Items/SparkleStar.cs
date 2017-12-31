using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items
{
    public class SparkleStar : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star Shard");
            DisplayName.AddTranslation(GameCulture.Chinese, "星之碎片");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(6, 4));
        }
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.healLife = 10;
            item.healMana = 5;
        }
        
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            if (Main.rand.Next(20) == 0)
            {
                Dust d = Dust.NewDustDirect(item.position, item.width, item.headSlot, 57 + Main.rand.Next(2), 0f, 0f, 200);
                d.noGravity = true;
                d.velocity *= 2f;
            }

            gravity /= 3f; // 0.1f
            maxFallSpeed *= 1.5f;

            // Bouncy
            if (item.velocity.Y == 0)
            {
                Main.PlaySound(2, (int)item.position.X, (int)item.position.Y, 25, 0.1f, 0.4f); // sparkly bounce effect
                for (int i = 0; i < (item.newAndShiny ? 4 : 10); i++)
                {
                    Dust d = Dust.NewDustDirect(item.position, item.width, item.headSlot, 57 + Main.rand.Next(2), 0f, 0f, 200);
                    d.noGravity = true;
                    d.velocity.X *= 2f;
                }

                if (item.newAndShiny)
                {
                    item.velocity.Y = -2f;
                    item.newAndShiny = false;
                    Console.WriteLine(item.whoAmI + " Bounced     !  !  !");
                }
                else
                {
                    item.active = false;
                    item.type = 0;
                    item.stack = 0;
                }

                if (Main.netMode == 2)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.netID, 0f, 0f, 0f, 0, 0, 0);
                }
            }
        }
        public override bool CanPickup(Player player)
        {
            return item.velocity.Y > -1f;
        }
        public override bool OnPickup(Player player)
        {
            player.statLife += item.healLife;
            player.statMana += item.healMana;
            if (Main.myPlayer == player.whoAmI)
            {
                player.HealEffect(item.healLife, true);
                player.ManaEffect(item.healMana);
            }

            Main.PlaySound(2, -1, -1, 4, 0.3f, 0.2f); // mini heal effect
            return false;
        }
        
        public override void GrabRange(Player player, ref int grabRange)
        {
            grabRange = 0;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = Main.itemTexture[item.type];
            float colMod = 1f;
            if (item.velocity.Y <= -2f)
            { colMod = 0.2f; }
            else if (item.velocity.Y <= -1.5f)
            { colMod = 0.4f; }

            spriteBatch.Draw(texture,
                item.Center - Main.screenPosition,
                Main.itemAnimations[item.type].GetFrame(texture),
                new Color(colMod, colMod, colMod, colMod * 0.5f),
                rotation, item.Center - item.position,
                Main.cursorScale,
                SpriteEffects.None, 0f);
            return false;
        }
    }
}
