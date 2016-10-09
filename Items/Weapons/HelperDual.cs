using System;
using System.Collections;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponOut.Items.Weapons
{
    /// <summary>
    /// Horrible janky code hacked together to try and keep clients 
    /// in sync in every (feasible) possible scenario. Also handles
    /// the swapping of item values as well prefix value management. 
    /// 
    /// Methods are used in ModPlayer and ModItem
    /// </summary>
    public class HelperDual
    {
        //singleton
        public static bool[] dualItems;

        Item item;
        bool setToDefaults = true;

        // Prefix managed values
        private int rare;
        private int value;
        private bool preferAltDamage;

        private int[] damage = new int[2];
        public int Damage { set { damage[1] = value; } }
        private int[] useAnimation = new int[2];
        public int UseAnimation { set { useAnimation[1] = value; } }
        private int[] useTime = new int[2];
        public int UseTime { set { useTime[1] = value; } }
        private int[] reuseDelay = new int[2];
        public int ReuseDelay { set { reuseDelay[1] = value; } }
        private int[] mana = new int[2];
        public int Mana { set { mana[1] = value; } }
        private float[] knockBack = new float[2];
        public float KnockBack { set { knockBack[1] = value; } }
        private float[] scale = new float[2];
        public float Scale { set { scale[1] = value; } }
        private float[] shootSpeed = new float[2];
        public float ShootSpeed { set { shootSpeed[1] = value; } }
        private int[] crit = new int[2];
        public int Crit { set { crit[1] = value; } }

        //general
        private int[] useSound = new int[2];
        public int UseSound { set { useSound[1] = value; } }
        private bool[] melee = new bool[2];
        public bool Melee { set { melee[1] = value; } }
        private bool[] ranged = new bool[2];
        public bool Ranged { set { ranged[1] = value; } }
        private bool[] magic = new bool[2];
        public bool Magic { set { magic[1] = value; } }
        private int[] useStyle = new int[2];
        public int UseStyle { set { useStyle[1] = value; } }
        private bool[] noMelee = new bool[2];
        public bool NoMelee { set { noMelee[1] = value; } }
        private bool[] autoReuse = new bool[2];
        public bool AutoReuse { set { autoReuse[1] = value; } }
        private bool[] useTurn = new bool[2];
        public bool UseTurn { set { useTurn[1] = value; } }

        //ranged & magic
        private int[] ammo = new int[2];
        public int Ammo { set { ammo[1] = value; } }
        private int[] shoot = new int[2];
        public int Shoot { set { shoot[1] = value; } }

        /// <summary>
        /// Initialise the helper AFTER setting the item defaults, BEFORE setting alt stats
        /// </summary>
        /// <param name="item"></param>
        /// <param name="preferAltPrefix">If the damage type prefers the alt function</param>
        public HelperDual(Item item, bool preferAltPrefix)
        {
            //initiliase this static arrray if never been used before, dunno how 2 singleton with arrays
            if (dualItems == null) dualItems = new bool[Item.staff.Length];
            dualItems[item.type] = true;

            this.item = item;
            rare = item.rare;
            value = item.value;
            preferAltDamage = preferAltPrefix;

            //setup these variables
            damage[0] = item.damage; 
            damage[1] = item.damage;
            useAnimation[0] = item.useAnimation;
            useAnimation[1] = item.useAnimation;
            useTime[0] = item.useTime; 
            useTime[1] = item.useTime;
            reuseDelay[0] = item.reuseDelay; 
            reuseDelay[1] = item.reuseDelay;
            mana[0] = item.mana; 
            mana[1] = item.mana;
            knockBack[0] = item.knockBack; 
            knockBack[1] = item.knockBack;
            scale[0] = item.scale; 
            scale[1] = item.scale;
            shootSpeed[0] = item.shootSpeed; 
            shootSpeed[1] = item.shootSpeed;
            crit[0] = item.crit; 
            crit[1] = item.crit;
            useSound[0] = item.useSound;
            useSound[1] = item.useSound;
            melee[0] = item.melee; 
            ranged[0] = item.ranged; 
            magic[0] = item.magic; 
            useStyle[0] = item.useStyle; 
            useStyle[1] = item.useStyle;
            noMelee[0] = item.noMelee;
            noMelee[1] = item.noMelee;
            autoReuse[0] = item.autoReuse; 
            autoReuse[1] = item.autoReuse;
            useTurn[0] = item.useTurn; 
            useTurn[1] = item.useTurn;
            ammo[0] = item.ammo; 
            ammo[1] = item.ammo;
            shoot[0] = item.shoot; 
            shoot[1] = item.shoot;
        }

        public static void PreItemCheckDualItem(Player player)
        {
            //initiliase this static arrray if never been used before, dunno how 2 singleton with arrays
            if (dualItems == null) dualItems = new bool[Item.staff.Length];
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = player.inventory[player.selectedItem];
                if (dualItems[item.type])
                {
                    if (player.altFunctionUse > 0
                        && player.HasBuff(WeaponOut.BuffIDWeaponSwitch) == -1)
                    {
                        //add buff quietly
                        player.AddBuff(WeaponOut.BuffIDWeaponSwitch, 2, true);
                        item.modItem.CanUseItem(player);
                        //add buff network
                        if (player.itemAnimation > 0)
                        {
                            player.AddBuff(WeaponOut.BuffIDWeaponSwitch, player.itemAnimation, false);
                        }
                        else
                        {
                            player.AddBuff(WeaponOut.BuffIDWeaponSwitch, item.useAnimation, false);
                        }
                        player.altFunctionUse = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the item to either state on swing
        /// </summary>
        /// <param name="player"></param>
        public void CanUseItem(Player player)
        {
            int buff = player.HasBuff(WeaponOut.BuffIDWeaponSwitch);
            if (player.altFunctionUse == 0)
            {
                if (buff != -1 && !setToDefaults)
                {
                    player.altFunctionUse = 2;
                    setValues(true);
                    //Main.NewText(item.name + " is ALT + " + setToDefaults);
                }
                else
                {
                    setValues(false);
                    //Main.NewText(item.name + " is VANILLA + " + setToDefaults);
                }
            }
        }
        /// <summary>
        /// Manages multiplayer syncing of altFunction and weapon state
        /// </summary>
        /// <param name="player"></param>
        public void UseStyleMultiplayer(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
            {
                int buff = player.HasBuff(WeaponOut.BuffIDWeaponSwitch);
                if (buff != -1)
                {
                if (player.altFunctionUse == 0)
                {
                        //Main.NewText(item.name + " is net ALT, len " + Main.projectile.Length);
                        player.altFunctionUse = 2;
                        setValues(true);

                        //search for most recent shot and point
                        foreach (Projectile p in Main.projectile)
                        {
                            if (!p.active) continue;
                            //Main.NewText("[" + p.whoAmI + "] " + p.name);
                            if (p.type == item.shoot && p.owner == player.whoAmI)
                            {
                                //check a bit ahead to get better idea of direction
                                player.itemRotation = (float)Math.Atan2(
                                    (double)((p.Center.Y + p.velocity.Y * 2 - player.Center.Y) * (float)player.direction),
                                    (double)((p.Center.X + p.velocity.X * 2 - player.Center.X) * (float)player.direction));
                                //set bodyframe because nothing else does...
                                float num18 = player.itemRotation * (float)player.direction;
                                player.bodyFrame.Y = player.bodyFrame.Height * 3;
                                if ((double)num18 < -0.75)
                                {
                                    player.bodyFrame.Y = player.bodyFrame.Height * 2;
                                    if (player.gravDir == -1f)
                                    {
                                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                                    }
                                }
                                if ((double)num18 > 0.6)
                                {
                                    player.bodyFrame.Y = player.bodyFrame.Height * 4;
                                    if (player.gravDir == -1f)
                                    {
                                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                                        return;
                                    }
                                }

                                break;
                            }
                        }
                        //Main.NewText(item.name + " is net ALT, point at " + player.itemRotation);
                        player.DelBuff(buff);

                        //stop looping issue from never reaching 0
                    }
                }
                else
                {
                    //fix autoswing items doing silly things
                    //Main.NewText(player.name + " animation is " + player.itemAnimation + "/" + player.itemAnimationMax);
                    if (item.autoReuse
                        && !player.noItems
                        && player.itemAnimation == 1
                        && player.controlUseItem
                        && player.altFunctionUse != 0)
                    {
                        setValues(false, true);
                        //Main.NewText("buff is " + buff + ", altfunc = " + player.altFunctionUse);
                    }
                }/*
                //Main.NewText(buff + " : " +
                    item.autoReuse + " : " +
                    !player.noItems + " : " +
                    player.itemAnimation + " : " +
                    player.controlUseItem + " : " +
                    player.altFunctionUse);*/
            }
        }
        /// <summary>
        /// Resets the item state to default
        /// </summary>
        /// <param name="player"></param>
        public void HoldStyle(Player player)
        {
            if (player.itemAnimation == 0
                && setToDefaults)
            {
                setValues(false, true);
                //Main.NewText(item.name + " at END HOLDSTYLE");
            }
        }

        public void setValues(bool altFunction, bool showDefaults = false)
        {
            //setup vars
            if (showDefaults) altFunction = false;
            setToDefaults = !showDefaults;

            //get indices
            int index = altFunction ? 1 : 0;
            int inver = altFunction ? 0 : 1;
            //reset value and rarity (they get modified by prefix)
            item.rare = rare;
            item.value = value;

            //setup values
            item.useAnimation = useAnimation[index];
            item.useTime = useTime[index];
            item.reuseDelay = reuseDelay[index];
            item.scale = scale[index];
            item.melee = melee[index];
            item.ranged = ranged[index];
            item.magic = magic[index];
            if (showDefaults)
            {
                // for defaults, we want to display as much info as
                // possible to allow for players to know what the item
                // does, as well as let prefix assign correctly

                if (damage[index] <= 0) item.damage = damage[inver];
                if (mana[index] == 0) item.mana = mana[inver];
                if (shootSpeed[index] <= 0) item.shootSpeed = shootSpeed[inver];
                if (crit[index] <= 0) item.crit = crit[inver];
                if (ammo[index] <= 0) item.ammo = ammo[inver];
                if (shoot[index] <= 0) item.shoot = shoot[inver];
                if (shootSpeed[index] <= 0) item.shootSpeed = shootSpeed[inver];
                
                //give priority due to prefix assignment
                if (preferAltDamage)
                {
                    if (ranged[inver]) { item.melee = false; item.ranged = true; item.magic = false; }
                    if (magic[inver]) { item.melee = false; item.ranged = false; item.magic = true; }
                }
            }
            else
            {
                item.damage = damage[index];
                item.mana = mana[index];
                item.shootSpeed = shootSpeed[index];
                item.crit = crit[index];
                item.ammo = ammo[index];
                item.shoot = shoot[index];
                item.shootSpeed = shootSpeed[index];
            }
            item.useSound = useSound[index];
            item.useStyle = useStyle[index];
            item.noMelee = noMelee[index];
            item.autoReuse = autoReuse[index];
            item.useTurn = useTurn[index];


            //set prefix to modify numbers
            item.Prefix(item.prefix);
        }

    }
}
