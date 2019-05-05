using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponOut.Items.Weapons.Sabres.BeamSabres
{
    public class BeamSabrePurple : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Purple Beam Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Фиолетовый Световой Клинок");
            SetStaticTooltip();
        }

        public override Color SabreColour()
        { return new Color(0.7f, 0f, 1f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabrePurpleSlash>(); }

        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 15);
            recipe.AddIngredient(ItemID.Amethyst, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
    public class BeamSabrePurpleSlash : BeamSabreSlash
    {
        public override Vector3 SabreColour()
        { return new Vector3(0.7f, 0f, 1f); }
    }

    public class BeamSabreYellow : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Yellow Beam Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Жёлтый Световой Клинок");
            SetStaticTooltip();
        }

        public override Color SabreColour()
        { return new Color(1f, 1f, 0f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabreYellowSlash>(); }

        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 15);
            recipe.AddIngredient(ItemID.Topaz, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
    public class BeamSabreYellowSlash : BeamSabreSlash
    {
        public override Vector3 SabreColour()
        { return new Vector3(1f, 1f, 0f); }
    }

    public class BeamSabreBlue : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blue Beam Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Синий Световой Клинок");
            SetStaticTooltip();
        }

        public override Color SabreColour()
        { return new Color(0f, 0.1f, 1f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabreBlueSlash>(); }

        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 15);
            recipe.AddIngredient(ItemID.Sapphire, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
    public class BeamSabreBlueSlash : BeamSabreSlash
    {
        public override Vector3 SabreColour()
        { return new Vector3(0f, 0.1f, 1f); }
    }

    public class BeamSabreGreen : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Green Beam Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Зелёный Световой Клинок");
            SetStaticTooltip();
        }

        public override Color SabreColour()
        { return new Color(0.5f, 1f, 0f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabreGreenSlash>(); }

        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 15);
            recipe.AddIngredient(ItemID.Emerald, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
    public class BeamSabreGreenSlash : BeamSabreSlash
    {
        public override Vector3 SabreColour()
        { return new Vector3(0.5f, 1f, 0f); }
    }

    public class BeamSabreRed : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Red Beam Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Красный Световой Клинок");
            SetStaticTooltip();
        }

        public override Color SabreColour()
        { return new Color(1f, 0.1f, 0f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabreRedSlash>(); }

        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 15);
            recipe.AddIngredient(ItemID.Ruby, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
    public class BeamSabreRedSlash : BeamSabreSlash
    {
        public override Vector3 SabreColour()
        { return new Vector3(1f, 0.1f, 0f); }
    }

    public class BeamSabreWhite : BeamSabre
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("White Beam Saber");
            DisplayName.AddTranslation(GameCulture.Russian, "Белый Световой Клинок");
            SetStaticTooltip();
        }

        public override Color SabreColour()
        { return new Color(1f, 1f, 1f); }

        public override int SabreSlashType()
        { return mod.ProjectileType<BeamSabreWhiteSlash>(); }

        public override void AddRecipes()
        {
            if (!ModConf.EnableSabres) return;
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MeteoriteBar, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 15);
            recipe.AddIngredient(ItemID.Diamond, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
    public class BeamSabreWhiteSlash : BeamSabreSlash
    {
        public override Vector3 SabreColour()
        { return new Vector3(1f, 1f, 1f); }
    }
}
