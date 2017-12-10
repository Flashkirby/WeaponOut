using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;

namespace WeaponOut.Tiles
{
    public class CampTent : ModTile
    {
        public override void SetDefaults()
        {
            //extra info
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Camping Tent");
            AddMapEntry(new Color(90, 190, 20), name);
            dustType = 93;
            disableSmartCursor = true;

            //style is set up like a bed
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
            //width in blocks, and define required ground anchor
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            
            //height, and coordinates of each row
            TileObjectData.newTile.Height = 3;
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
                Item.NewItem(i * 16, j * 16, 80, 48, mod.ItemType("CampTent"));
            }
        }

        public override void RightClick(int i, int j)
        {
            //get middle bottom of tent
            Player player = Main.player[Main.myPlayer];
            Tile tile = Main.tile[i, j];
            int spawnX = i - tile.frameX / 18 + 2;
            int spawnY = j - tile.frameY / 18 + 2;
            if (tile.frameX > 90) spawnX += 5;

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
                player.showItemIcon2 = mod.ItemType("CampTent");
            }
        }
    }
}