using System;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items
{
    public class HeliosphereEmblem : ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Heliosphere Emblem";
            item.toolTip = "Supercharges melee weapons";
            item.toolTip2 = "'Rekindling old flames'";
            item.width = 24;
            item.height = 24;
            item.rare = 10;
            item.value = Item.sellPrice(0, 25, 0, 0);
            item.accessory = true;
            item.expert = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShinyStone, 1);
            recipe.AddIngredient(ItemID.Terrarian, 1);
            recipe.AddIngredient(ItemID.Meowmere, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 0);
            HeliosphereEmblem.SetBonus(player, 1);
            HeliosphereEmblem.SetBonus(player, 2);
            HeliosphereEmblem.SetBonus(player, 3);
            HeliosphereEmblem.SetBonus(player, 4);
        }

        public static float SetBonus(Player player, int bonusType)
        {
            if (player.inventory[player.selectedItem].type == 0) return 0f; //exit for empty slot

            //keep track of current weapon state
            Item heldItem = player.inventory[player.selectedItem];

            //keep track of default stats disregarding prefixes and other bonues effects
            Item defaultItem = new Item();
            defaultItem.SetDefaults(player.inventory[player.selectedItem].type);

            int damageSources = 0;
            float rawIncrease = 0;

            //Main.NewText("myDPS = " + CalculateDPS(damageSources, item.damage, item.crit, item.useAnimation));
            if (!heldItem.noMelee) damageSources++; //deadls damage from melee hits
            if (heldItem.shoot > 0) damageSources++; //fires projectiles
            if (heldItem.melee && bonusType == 0)
            {
                //melee
                
                if (heldItem.shoot > 0)
                {
                    Projectile p = new Projectile();
                    p.SetDefaults(heldItem.shoot);
                    if (p.aiStyle == 99) 
                        // YOYO is aiStyle 99, 10 real usespeed due to constant hits
                        rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, defaultItem.crit, 10, 10, defaultItem.reuseDelay);
                    else if (p.aiStyle == 75) 
                        // moonlord weapon behaviours are busted
                        rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, 
                            defaultItem.crit, 5, 5, defaultItem.reuseDelay);
                    else if (p.penetrate < 0 && defaultItem.useTime < 10)
                        // penetrating projectiles cannot hit faster than 10 usetime due to npc immune
                        rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, 
                            defaultItem.crit, defaultItem.useAnimation, 10, defaultItem.reuseDelay);
                    else
                        //standard calculation
                        rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, 
                            defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
                }
                else
                {
                    if (heldItem.pick > 0
                        || heldItem.axe > 0
                        || heldItem.hammer > 0)
                    {
                        //tools don't benefit from usetime in combat, so only swing as normal
                        if (heldItem.shoot > 0)
                        {
                            //drills yo
                            rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, 
                                defaultItem.crit, 10, 10, defaultItem.reuseDelay);
                        }
                        else
                        {
                            //not drills, yo
                            rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, 
                                defaultItem.crit, defaultItem.useAnimation, defaultItem.useAnimation, defaultItem.reuseDelay);
                        }
                    }
                    else
                    {
                        //standard caluclation
                        rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage, 
                            defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
                    }
                }
            }
            else if (heldItem.ranged && bonusType == 1)
            {
                Projectile p = new Projectile();
                p.SetDefaults(heldItem.shoot);
                if (heldItem.useAmmo == 1 || heldItem.useAmmo == 323)
                {
                    //arrow
                    if (p.aiStyle == 75)
                        // lunar weapon behaviours are busted
                        rawIncrease = CalculateBonusRaw(arrowDPS, damageSources, defaultItem.damage,
                            defaultItem.crit, 2, 2, defaultItem.reuseDelay);
                    else
                        rawIncrease = CalculateBonusRaw(arrowDPS, damageSources, defaultItem.damage + testArrow, 
                            //-1 because autoreuse usually matters a lot with these weapons
                            defaultItem.crit, defaultItem.useAnimation - 1, defaultItem.useTime - 1, defaultItem.reuseDelay);
                }
                else if (heldItem.useAmmo == 14 || heldItem.useAmmo == 311)
                {
                    //bullet - to check, makes minishark busted af
                    if (p.aiStyle == 75)
                        // lunar weapon behaviours are busted
                        rawIncrease = CalculateBonusRaw(bulletDPS, damageSources, defaultItem.damage,
                            defaultItem.crit, 2, 2, defaultItem.reuseDelay);
                    else
                        rawIncrease = CalculateBonusRaw(bulletDPS, damageSources, defaultItem.damage + testBullet,
                            //-1 because autoreuse usually matters a lot with these weapons
                            defaultItem.crit, defaultItem.useAnimation - 1, defaultItem.useTime - 1, defaultItem.reuseDelay);
                }
                else if (heldItem.useAmmo == 771 || heldItem.useAmmo == 246 || heldItem.useAmmo == 312 || heldItem.useAmmo == 514)
                {
                    //rocket
                    rawIncrease = CalculateBonusRaw(rocketDPS, damageSources, defaultItem.damage + testRocket,
                        defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
                }
                else
                {
                    //non-standard or non-ammo benefiting weapon, eg. flamethrower
                    // = rangedDPS;
                }
            }
            else if (heldItem.thrown && bonusType == 2)
            {
                //throwing
                // = throwingDPS;
            }
            else if (heldItem.magic && bonusType == 3)
            {
                //magic
                // = magicDPS;
            }
            else if (heldItem.summon && bonusType == 4)
            {
                //minions
                // = summonDPS;
            }

            //calculate wepaon bonus
            float bonus = 0f;
            if (rawIncrease > 0)
            {
                bonus = (defaultItem.damage + rawIncrease) / defaultItem.damage;
                if (heldItem.melee) player.meleeDamage += bonus - 1f;
                if (heldItem.ranged) player.rangedDamage += bonus - 1f;
                if (heldItem.thrown) player.thrownDamage += bonus - 1f;
                if (heldItem.magic)
                {
                    player.magicDamage += bonus - 1f;
                    player.manaCost += 1f;
                }
                if (heldItem.summon) player.minionDamage += bonus - 1f;
            }

            return bonus;
        }

        public static float CalculateBonusRaw(float goalDPS, int damageSources, int damage, int crit, int useAnimation, int useTime, int reuseDelay)
        {
            if(damageSources <= 0) return 0;
            int hits = CalculateNumOfHits(useAnimation, useTime);
            int pureDamage = (int)(damage * (1f + 0.01f * (crit + 4)));
            float trueAnimation = Math.Max(useAnimation + reuseDelay, 1);
            float hps = 60f / trueAnimation;

            Main.NewText("src " + damageSources + " | hits " + hits + " | dmg " + +pureDamage + " | hps " + hps);

            //Main.NewText("goalDPS = " + goalDPS + " | puredmg = " + pureDamage);
            //Main.NewText("goalDPS / anim/hits/dmgsrc = " + (goalDPS / (60 / trueAnimation) / hits / damageSources));
            Main.NewText("all = " + (goalDPS / hps / hits / damageSources - pureDamage));

            float rawBonus = goalDPS / hps / hits / damageSources - pureDamage;

            return rawBonus * 0.667f; //reduce by a third due to crit double principle - high dmg weapons will get better crits
        }

        #region initial calculations

        public static float meleeDPS;
        public static float arrowDPS;
        public static float bulletDPS;
        public static float rocketDPS;
        public static float rangedDPS;
        public static float throwingDPS;
        public static float magicDPS;
        public static float summonDPS;
        public static void SetUpGlobalDPS()
        {
            meleeDPS = CalculateDPS(2, 200, 0, 16); //meowmere (spawns laser rainbow cats)
            arrowDPS = CalculateDPS(1, 50 + testArrow, 0, 2); //phantasm (it's really fast)
            bulletDPS = CalculateDPS(1, 77 + testBullet, 10, 10); //S.D.M.G (boring, but effecient)
            rocketDPS = CalculateDPS(2, 65 + testRocket, 10, 29); //Celebration because its the moonlord rocket, though snowman is "better"
        }

        public const int testDEF = 30; //high def enemies, not moonlord (who is 50) - also ichor's busted def reduction
        public const int testArrow = 17; //venom/cursed arrow
        public const int testBullet = 14; //venom bullet
        public const int testRocket = 65; //rocket 3/4
        public static float CalculateDPS(int damageSources, int damage, int crit, int useAnimation)
        {
            return CalculateDPS(damageSources, damage, useAnimation, useAnimation, 0, 0);
        }
        public static float CalculateDPS(int damageSources, int damage, int crit, int useAnimation, int useTime, int reuseDelay)
        {
            int hits = CalculateNumOfHits(useAnimation, useTime);
            //Main.NewText("hits = " + hits);
            float trueDamage = CalculateDamageFromDefAndCrit(damage, crit);
            float trueAnimation = Math.Max(useAnimation + reuseDelay, 1);

            //Main.NewText("calculating " + damageSources + trueDamage + " * " + hits + " * 60 / " + trueAnimation + " | crit " + crit);
            return damageSources * trueDamage * hits * 60 / trueAnimation;
        }
        private static float CalculateDamageFromDefAndCrit(int damage, int crit)
        {
            float trueDamage = Math.Max(damage - testDEF, 1);
            trueDamage *= 1f + 0.01f * (crit + 4);
            return trueDamage;
        }
        private static int CalculateNumOfHits(int useAnimation, int useTime)
        {
            if (useTime < 1) useTime = useAnimation;
            int hits = 1 + (useAnimation - 1) / useTime;
            return hits;
        }
        #endregion
    }
}
