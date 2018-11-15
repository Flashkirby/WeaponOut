using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;

namespace WeaponOut.Tiles
{
    public class CampTent : ModTile
    {
        const int _FRAMEWIDTH = 5;
        const int _FRAMEHEIGHT = 3;

        public override void SetDefaults()
        {
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Camping Tent");
            name.AddTranslation(GameCulture.Chinese, "野营帐篷");
            AddMapEntry(new Color(90, 190, 20), name);
            TileID.Sets.HasOutlines[Type] = true;

            //extra info
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            dustType = 93;
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Beds };
            bed = true;

            //style is set up like a bed
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
            //strangely enough this means styles are read VERTICALLY
            TileObjectData.newTile.StyleHorizontal = true;
            //width in blocks, and define required ground anchor
            TileObjectData.newTile.Width = _FRAMEWIDTH;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

            //height, and coordinates of each row
            TileObjectData.newTile.Height = _FRAMEHEIGHT;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16,
                16,
                16
            };

            //placement centre and offset on ground
            TileObjectData.newTile.Origin = new Point16(2, 2);
            TileObjectData.newTile.DrawYOffset = 2;

            //add left and right versions
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);

        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (ModConf.EnableBasicContent)
            {
                int type = GetItemTypeFromStyle(frameY);
                if (type > 0)
                { Item.NewItem(i * 16, j * 16, 80, 48, mod.ItemType("CampTent")); }
            }
        }

        public override bool HasSmartInteract()
        { return true; }

        public override void RightClick(int i, int j)
        {
            //get middle bottom of tent
            Player player = Main.player[Main.myPlayer];
            Tile tile = Main.tile[i, j];
            int frameX = tile.frameX % (18 * _FRAMEWIDTH * 2);
            int frameY = tile.frameY % (18 * _FRAMEHEIGHT);
            int spawnX = i - frameX / 18 + 2;
            int spawnY = j - frameY / 18 + 2;
            if (frameX > 90) spawnX += 5; // mirror facing offset for alternate

            //Dust.NewDust(new Vector2((float)(spawnX * 16), (float)(spawnY * 16)), 16, 16, 6, 0f, 0f, 0, default(Color), 4f);
            PlayerFX modPlayer = player.GetModPlayer<PlayerFX>(mod);
            if (modPlayer.localTempSpawn != new Vector2(spawnX, spawnY))
            {
                Main.NewText("Temporary spawn point set!", 255, 240, 20, false);
                modPlayer.localTempSpawn = new Vector2(spawnX, spawnY);
            }
            else
            {
                if (player.SpawnX == -1 && player.SpawnY == -1)
                {
                    Main.NewText("Temporary spawn point removed!", 255, 240, 20, false);
                }
                else
                {
                    Main.NewText("Spawn point set to bed!", 255, 240, 20, false);
                }
                modPlayer.localTempSpawn = new Vector2();
            }

        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.player[Main.myPlayer];
            player.noThrow = 2;
            if (ModConf.EnableBasicContent)
            {
                player.showItemIcon = true;
                int type = GetItemTypeFromStyle(Main.tile[i, j].frameY);
                player.showItemIcon2 = type;
            }
        }

        private int GetItemTypeFromStyle(int frameY)
        {
            int style = frameY / (18 * _FRAMEHEIGHT);
            int type = 0;
            switch (style)
            {
                case 0:
                    type = mod.ItemType("CampTent");
                    break;
                case 1:
                    type = mod.ItemType("CampTentMakeshift");
                    break;
            }

            return type;
        }
    }
}