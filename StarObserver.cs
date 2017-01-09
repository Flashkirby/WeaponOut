using Terraria;
using Terraria.ModLoader;

namespace WeaponOut
{
    /// <summary>
    /// OUr mission? TO look up and watch the stars. 
    /// Mainly to test multiplayer bugs though.
    /// </summary>
    public class StarObserver : ModWorld
    {
        public override bool Autoload(ref string name)
        {
            return false;
        }

        public override void PostUpdate()
        {
            if (Main.time % 120 == 0)
            {
                Main.NewText("call");
                System.Console.WriteLine("Firing Star");

                Player player = Main.player[0];
                Projectile.NewProjectile(player.Center.X, player.Center.Y - 100, player.velocity.X, 5f, 12, 1000, 10f, Main.myPlayer, 0f, 0f);
            }
            
            foreach (Projectile p in Main.projectile)
            {
                if(p.active && p.type == 12)
                {
                    string message = "Falling Star seen at " +
                        p.Center.ToTileCoordinates().X +
                        ", from pleyer" +
                        Main.player[0].Center.ToTileCoordinates().X;
                    System.Console.WriteLine(message);
                    Main.NewText(message);
                }
            }
            
        }
    }
}
