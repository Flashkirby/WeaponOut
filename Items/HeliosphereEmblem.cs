using System;
using System.Collections.Generic;

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
            item.toolTip = @"Supercharges melee weapons to their lunar potential
12% increased melee speed";
            item.toolTip2 = "'Rekindling old flames'";
            item.width = 28;
            item.height = 28;
            item.rare = 10;
            item.value = Item.sellPrice(0, 25, 0, 0);
            item.accessory = true;
            item.expert = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShinyStone, 1);
            recipe.AddIngredient(ItemID.WarriorEmblem, 1);
            recipe.AddIngredient(ItemID.Meowmere, 1);
            recipe.AddIngredient(ItemID.Terrarian, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HeliosphereEmblem.SetBonus(player, 0);
            player.meleeSpeed += 0.12f;
            player.magmaStone = true;
        }

        #region General Emblem Code

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bonusType">0 melee, 1 ranged, 2 thrown, 3 magic, 4 summon</param>
        /// <returns></returns>
        public static float SetBonus(Player player, int bonusType)
        {
            if (player.inventory[player.selectedItem].type == 0) return 0f; //exit for empty slot

            //keep track of current weapon state
            Item heldItem = player.inventory[player.selectedItem];

            //keep track of default stats disregarding prefixes and other bonues effects
            Item defaultItem = new Item();
            defaultItem.SetDefaults(player.inventory[player.selectedItem].type);

            float rawIncrease = 0;

            if (heldItem.melee && bonusType == 0)
            {
                //melee
                rawIncrease = SetBonusMelee(heldItem, defaultItem, rawIncrease);
                NerfMultiShots(player, rawIncrease);
                ApplyAutoReuse(player, heldItem);
            }
            else if (heldItem.ranged && bonusType == 1)
            {
                //ranged
                rawIncrease = SetBonusRanged(player, heldItem, defaultItem, rawIncrease);
                NerfMultiShots(player, rawIncrease);
                ApplyAutoReuse(player, heldItem);
            }
            else if (heldItem.thrown && bonusType == 2)
            {
                //throwing
                rawIncrease = SetBonusThrowing(defaultItem, heldItem, rawIncrease);
                NerfMultiShots(player, rawIncrease);
                ApplyAutoReuse(player, heldItem);
            }
            else if (heldItem.magic && bonusType == 3)
            {
                //magic
                rawIncrease = SetBonusMagic(defaultItem, heldItem, rawIncrease);
                NerfMultiShots(player, rawIncrease);
                ApplyAutoReuse(player, heldItem);
            }
            else if (heldItem.summon && bonusType == 4)
            {
                //minions
                rawIncrease = SetBonusSummon(defaultItem, rawIncrease);
            }

            //calculate wepaon bonus
            float bonus = 0f;
            if (rawIncrease > 0)
            {
                bonus = (defaultItem.damage + rawIncrease) / defaultItem.damage;
                if (heldItem.melee || (!heldItem.noMelee && heldItem.shoot != 0)) player.meleeDamage += bonus - 1f;
                if (heldItem.ranged) player.rangedDamage += bonus - 1f;
                if (heldItem.thrown) player.thrownDamage += bonus - 1f;
                if (heldItem.magic)
                {
                    //modfy mana costs
                    player.magicDamage += bonus - 1f;
                    player.manaCost += CalculateRawManaCost(player, defaultItem) / defaultItem.mana;
                }
                if (heldItem.summon) player.minionDamage += bonus - 1f;
            }

            return bonus;
        }

        /// <summary>
        /// Custom auto rese script, because normal autoreuse breaks things like spears
        /// </summary>
        /// <param name="player"></param>
        /// <param name="heldItem"></param>
        private static void ApplyAutoReuse(Player player, Item heldItem)
        {
            //Main.NewText("auto is melee? " + heldItem.melee + " | reuse? " + heldItem.autoReuse);
            if (!heldItem.autoReuse)
            {
                if (player.itemAnimation == 0)
                {
                    player.releaseUseItem = true;
                }
            }
        }

        private static float SetBonusSummon(Item defaultItem, float rawIncrease)
        {
            //minions all have similar damage output that increases with game progress and complexity
            rawIncrease = Math.Max(0, 50 + (10 - defaultItem.rare) * 5 - defaultItem.damage);
            return rawIncrease;
        }

        private static float CalculateRawManaCost(Player player, Item defaultItem)
        {
            //silly formula mostly does what its meant to...
            float modHitsPerSecond = 30 / Math.Max(1, defaultItem.useAnimation);
            return 1.5f * Math.Max(0, 10 - defaultItem.rare) / modHitsPerSecond;
        }
        private static float SetBonusMagic(Item defaultItem, Item heldItem, float rawIncrease)
        {
            Projectile p = new Projectile();
            p.SetDefaults(heldItem.shoot);
            if (p.usesLocalNPCImmunity || p.aiStyle == 75)
            {
                //projectiles with usesLocalNPCImmunity hit much more often, so do lunar weapons eg last prism
                rawIncrease = CalculateBonusRaw(magicDPS, 1, defaultItem.damage,
                    defaultItem.crit, Math.Min(defaultItem.useAnimation, 2), Math.Min(defaultItem.useTime, 2), defaultItem.reuseDelay);
            }
            else if (isPenetrating(p.penetrate))
            {
                //projectiles with penetrate = -1 tend to rely on 6hps repeated damage
                rawIncrease = CalculateBonusRaw(magicDPS, 1, defaultItem.damage,
                    defaultItem.crit, Math.Min(defaultItem.useAnimation, 10), Math.Min(defaultItem.useTime, 10), defaultItem.reuseDelay);
            }
            else
            {
                //standard calculiation
                rawIncrease = CalculateBonusRaw(magicDPS, 1, defaultItem.damage,
                    defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
            }
            return rawIncrease;
        }

        private static float SetBonusThrowing(Item defaultItem, Item heldItem, float rawIncrease)
        {
            Projectile p = new Projectile();
            p.SetDefaults(heldItem.shoot);
            //Throwing weapons do have melee, but realistically it doesn't factor in so dmgb source is always 1
            if (isPenetrating(p.penetrate))
            {
                rawIncrease = CalculateBonusRaw(throwingDPS, 1, defaultItem.damage,
                    defaultItem.crit, Math.Min(defaultItem.useAnimation, 10), Math.Min(defaultItem.useTime, 10), defaultItem.reuseDelay);
            }
            else
            {
                //"standard" calcitlation, no throwing weapons are even past hardmode anyway so this is... mods?
                rawIncrease = CalculateBonusRaw(throwingDPS, 1, defaultItem.damage,
                    defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
            }
            return rawIncrease;
        }

        private static float SetBonusRanged(Player player, Item heldItem, Item defaultItem, float rawIncrease)
        {
            Projectile p = new Projectile();
            p.SetDefaults(heldItem.shoot);
            float ammoInfluence = 1f;
            //arrow
            if (heldItem.useAmmo == 1 || heldItem.useAmmo == 323)
            {
                if (p.aiStyle == 75)
                    // lunar weapon behaviours are busted
                    rawIncrease = CalculateBonusRaw(arrowDPS, 1, defaultItem.damage,
                        defaultItem.crit, 2, 2, defaultItem.reuseDelay);
                else
                    rawIncrease = CalculateBonusRaw(arrowDPS, 1, defaultItem.damage + testArrow,
                        defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
                //modify damage due to differences caused by ammo damage relative to weapon damage
                ammoInfluence = (float)testArrow / defaultItem.damage;
            }
            //bullet
            else if (heldItem.useAmmo == 14 || heldItem.useAmmo == 311)
            {
                if (p.aiStyle == 75)
                    // lunar weapon behaviours are busted
                    rawIncrease = CalculateBonusRaw(bulletDPS, 1, defaultItem.damage,
                        defaultItem.crit, 2, 2, defaultItem.reuseDelay);
                else
                    rawIncrease = CalculateBonusRaw(bulletDPS, 1, defaultItem.damage + testBullet,
                        defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);

                //modify damage due to differences caused by ammo damage relative to weapon damage
                ammoInfluence = (float)testBullet / defaultItem.damage;
            }
            //rocket
            else if (heldItem.useAmmo == 771 || heldItem.useAmmo == 246 || heldItem.useAmmo == 312 || heldItem.useAmmo == 514)
            {
                rawIncrease = CalculateBonusRaw(rocketDPS, 1, defaultItem.damage + testRocket,
                    defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
                
                //modify damage due to differences caused by ammo damage relative to weapon damage
                ammoInfluence = (float)testRocket / defaultItem.damage;
            }
            //non-standard or non-ammo benefiting weapon, eg. flamethrower
            else
            {
                if (isPenetrating(p.penetrate))
                {
                    //projectiles with penetrate = -1 tend to rely on 6hps repeated damage
                    rawIncrease = CalculateBonusRaw(bulletDPS, 1, defaultItem.damage,
                        defaultItem.crit, Math.Min(defaultItem.useAnimation, 10), Math.Min(defaultItem.useTime, 10), defaultItem.reuseDelay);
                }
                else
                {
                    rawIncrease = CalculateBonusRaw(bulletDPS, 1, defaultItem.damage,
                        defaultItem.crit, defaultItem.useAnimation, defaultItem.useTime, defaultItem.reuseDelay);
                }
            }
            rawIncrease /= 0.5f + ammoInfluence / 2f;

            return rawIncrease;
        }

        private static float SetBonusMelee(Item heldItem, Item defaultItem, float rawIncrease)
        {
            int damageSources = 1; //deadls damage from melee hits
            //Main.NewText("myDPS = " + CalculateDPS(damageSources, item.damage, item.crit, item.useAnimation));
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
                else if (isPenetrating(p.penetrate) && defaultItem.useTime < (10 * damageSources))
                    // penetrating projectiles cannot hit faster than 10 usetime due to npc immune
                    rawIncrease = CalculateBonusRaw(meleeDPS, damageSources, defaultItem.damage,
                        defaultItem.crit, 10, 10, defaultItem.reuseDelay);
                else
                    if(p.penetrate <= 1) damageSources++; //fires (that don't interefere with sword damage)
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
            return rawIncrease;
        }

        private static bool isPenetrating(int penetrateAmount) { return penetrateAmount < 0 || penetrateAmount >= 5; }

        private static void NerfMultiShots(Player player, float rawIncrease)
        {
            Projectile check = new Projectile();
            List<Projectile> myProjs = new List<Projectile>();
            foreach (Projectile proj in Main.projectile)
            {
                if (!proj.active ||
                    !proj.friendly ||
                    (!proj.melee && !proj.ranged && !proj.magic && !proj.thrown) ||
                    proj.npcProj ||
                    proj.owner != player.whoAmI) continue;
                //if (Main.netMode == 1) Main.NewText("proj: " + p.name + " to be modded - " + p.penetrate + " | " + p.maxPenetrate);
                check.SetDefaults(proj.type);
                if (proj.timeLeft == (check.timeLeft - (1 + check.extraUpdates))
                    && !isPenetrating(proj.penetrate))
                {
                    //spawn just now
                    myProjs.Add(proj);
                }
            }
            if (myProjs.Count > 1)
            {
                //Main.NewText("Balancing " + myProjs[0].name + " damage");
                foreach (Projectile proj in myProjs)
                {
                    float semiBaseDmg = proj.damage - rawIncrease;
                    //Divide damage by count, because the emblem already buffs it
                    //Main.NewText((proj.damage / myProjs.Count * 2) + " | " + semiBaseDmg);
                    proj.damage /= myProjs.Count;
                    if (proj.damage * 1.5 < semiBaseDmg) proj.damage = (int)(semiBaseDmg + rawIncrease / myProjs.Count);
                }
            }
        }

        public static float CalculateBonusRaw(float goalDPS, int damageSources, int damage, int crit, int useAnimation, int useTime, int reuseDelay)
        {
            if(damageSources < 1) return 1;
            int hits = CalculateNumOfHits(useAnimation, useTime);
            int pureDamage = (int)(damage * (1f + 0.01f * (crit + 4)));
            float trueAnimation = Math.Max(useAnimation + reuseDelay, 1);
            float hps = 60f / trueAnimation;

            //Main.NewText("src " + damageSources + " | hits " + hits + " | dmg " + +pureDamage + " | hps " + hps);

            //Main.NewText("goalDPS = " + goalDPS + " | puredmg = " + pureDamage);
            //Main.NewText("goalDPS / anim/hits/dmgsrc = " + (goalDPS / (60 / trueAnimation) / hits / damageSources));
            //Main.NewText("all = " + (goalDPS / hps / hits / damageSources - pureDamage));

            float rawBonus = goalDPS / hps / hits / damageSources - pureDamage;
            return rawBonus;
        }
        #endregion

        #region Global Emblem Calculations

        public static float meleeDPS;
        public static float arrowDPS;
        public static float bulletDPS;
        public static float rocketDPS;
        public static float rangedDPS;
        public static float throwingDPS;
        public static float magicDPS;
        public static void SetUpGlobalDPS()
        {
            meleeDPS = CalculateDPS(1, 190, 10, 10); //terrarian (yoyos hit 6 times per second)
            arrowDPS = CalculateDPS(1, 50 + testArrow, 0, 2); //phantasm (it's really fast)
            bulletDPS = CalculateDPS(1, 77 + testBullet, 5, 5); //S.D.M.G (boring, but effecient)
            rocketDPS = CalculateDPS(2, 65 + testRocket, 10, 29); //Celebration (which contrary to popular belief is higher damage due to double damage when direct hit)
            throwingDPS = CalculateDPS(1, 200, 0, 11); //Bone (but dem bones be 5 times as powerful, also has melee)
            magicDPS = CalculateDPS(3, 100, 0, 10); //lunar flare, which fires 3 shots
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
