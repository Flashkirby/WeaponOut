using System;
using System.Collections.Generic;
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
    /// NOTE: sounds aren't synced reliably, so use projectiles or
    /// other alternative to guarantee better net syncing.
    /// 
    /// HelperDual requires:
    ///     ModItem method calls:
    ///         HelperDual(Item item, bool preferAltPrefix)
    ///         FinishDefaults()
    ///         OnCraft(Item item)
    ///         CanUseItem(Player player)
    ///         UseStyleMultiplayer(Player player)
    ///         HoldStyle(Player player)
    ///     ModPlayer method call:
    ///         PreItemCheckDualItem(Player player)
    ///     Alt func buff:
    ///         WeaponSwitch.cs
    /// </summary>
    public class HelperDual
    {
        //singleton
        private static List<int> dualItems;
        public static List<int> DualItems
        {
            get
            {
                if (dualItems == null) dualItems = new List<int>();
                return dualItems;
            }
        }

        //buff used for detecthing altfunc
        public static int altbuff = WeaponOut.BuffIDWeaponSwitch;

        Item item;
        bool setToDefaults = true;

        // Prefix managed values
        private int rare;
        private int value;
        private bool autoReuse;
        private bool preferAltDamage;

        //saved default values (before prefixing)
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
        private bool[] useTurn = new bool[2];
        public bool UseTurn { set { useTurn[1] = value; } }

        //ranged & magic
        private int[] useAmmo = new int[2];
        public int UseAmmo { set { useAmmo[1] = value; } }
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
            if (!DualItems.Contains(item.type)) DualItems.Add(item.type);

            this.item = item;
            rare = item.rare;
            value = item.value;
            autoReuse = item.autoReuse;
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
            useTurn[0] = item.useTurn; 
            useTurn[1] = item.useTurn;
            useAmmo[0] = item.useAmmo;
            useAmmo[1] = item.useAmmo;
            shoot[0] = item.shoot; 
            shoot[1] = item.shoot;
        }

        /// <summary>
        /// Call this after setting up dual defaults to set the item to its correct calculated stat display
        /// </summary>
        public void FinishDefaults()
        {
            setValues(false, true);
        }

        /// <summary>
        /// Call in PreItemCheck to intercept and change the item's stats before the game realises
        /// </summary>
        /// <param name="player"></param>
        public static void PreItemCheckDualItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = player.inventory[player.selectedItem];
                if (DualItems.Contains(item.type))
                {
                    if (player.altFunctionUse == 1 //the frame you right click
                        && player.HasBuff(altbuff) == -1
                        && player.itemAnimation <= 0 + (item.autoReuse ? 1 : 0)) //why is autoreuse so awkward :(
                    {
                        //Main.NewText(player.altFunctionUse + " | " + player.itemAnimation + " <= " + (0 + (item.autoReuse ? 1 : 0)));
                        //add buff quietly
                        player.AddBuff(altbuff, 2, true);
                        item.modItem.CanUseItem(player);
                        //add buff network
                        if (player.itemAnimation > 0)
                        {
                            player.AddBuff(altbuff, player.itemAnimation, false);
                        }
                        else
                        {
                            player.AddBuff(altbuff, item.useAnimation, false);
                        }
                        player.altFunctionUse = 0;
                    }
                }
            }
        }

        /// <summary>
        /// For some reason the items fail to work unless they are re-instanced
        /// </summary>
        /// <param name="item"></param>
        public static void OnCraft(Item item)
        {
            int pre = item.prefix;
            item.SetDefaults(item.type);
            item.Prefix(pre);
        }

        /// <summary>
        /// Sets the item to either state on swing. 
        /// </summary>
        /// <param name="player"></param>
        public void CanUseItem(Player player)
        {
            int buff = player.HasBuff(altbuff);
            bool raceScenario = (buff != -1 && player.itemAnimation <= 0); //just before int weaponDamage is setup
            if (raceScenario) setToDefaults = false;
            if (player.altFunctionUse == 0 //during CanUseItem in ItemCheck
                || raceScenario)
            {
                if (buff != -1 && !setToDefaults)
                {
                    player.altFunctionUse = 2;
                    setValues(true);
                }
                else
                {
                    setValues(false);
                }
                //Main.NewText("anim: " + player.itemAnimation + " | weapon damage is " + player.GetWeaponDamage(item));
            }
        }
        /// <summary>
        /// Manages multiplayer syncing of altFunction and weapon state. 
        /// Very important because this is where multiplayer checks for the buff,
        /// and does the magic so it "works". Basically netcode boooooo
        /// </summary>
        /// <param name="player"></param>
        public void UseStyleMultiplayer(Player player)
        {
            //multiplayer clients not inc. host
            if (player.whoAmI != Main.myPlayer && Main.netMode == 1)
            {
                int buff = player.HasBuff(altbuff);
                if (buff != -1) //has the buff
                {
                    if (player.altFunctionUse == 0)
                    {
                        //Main.NewText(item.name + " is net ALT, len " + Main.projectile.Length);
                        player.altFunctionUse = 2;
                        setValues(true);

                        if (item.useStyle == 5) //guns don't sync, we gotta do this manually
                        {
                            //search for a shot and point at it, its probably the right one
                            foreach (Projectile p in Main.projectile)
                            {
                                if (!p.active) continue;
                                //Main.NewText("[" + p.whoAmI + "] " + p.name);
                                //is the projectile something we did
                                if (p.type == item.shoot && p.owner == player.whoAmI)
                                {
                                    //since projectiles spawn on use, see where its going first and point there.
                                    player.itemRotation = (float)Math.Atan2(
                                        (double)((p.Center.Y + p.velocity.Y * 2 - player.Center.Y) * (float)player.direction),
                                        (double)((p.Center.X + p.velocity.X * 2 - player.Center.X) * (float)player.direction));

                                    //set bodyframe because it dun don't sync bruh
                                    //this si just copypastoed
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
                        }
                        //Main.NewText(item.name + " is net ALT, point at " + player.itemRotation);
                        player.DelBuff(buff);
                        //stop looping issue from never reaching 0?? what is thi comment
                    }
                }
                else    //no buff
                {
                    //fix autoswing items doing silly things by resesting to defualt
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

        /// <summary>
        /// Where the magic happens. setToDefaults 
        /// toggles on and off here between usages.
        /// </summary>
        /// <param name="altFunction"></param>
        /// <param name="showDefaults"></param>
        private void setValues(bool altFunction, bool showDefaults = false)
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

            item.damage = damage[index];
            item.knockBack = knockBack[index];
            item.mana = mana[index];
            item.shootSpeed = shootSpeed[index];
            item.crit = crit[index];
            item.useAmmo = useAmmo[index];
            item.shoot = shoot[index];
            item.shootSpeed = shootSpeed[index];
            if (showDefaults)
            {
                // for defaults, we want to display as much info as
                // possible to allow for players to know what the item
                // does, as well as let prefix assign correctly

                if (damage[index] <= 0) item.damage = damage[inver];
                if (mana[index] == 0) item.mana = mana[inver];
                if (knockBack[index] == 0) item.knockBack = knockBack[inver];
                if (shootSpeed[index] <= 0) item.shootSpeed = shootSpeed[inver];
                if (crit[index] <= 0) item.crit = crit[inver];
                if (useAmmo[index] <= 0) item.useAmmo = useAmmo[inver];
                if (shoot[index] <= 0) item.shoot = shoot[inver];
                if (shootSpeed[index] <= 0) item.shootSpeed = shootSpeed[inver];
                
                //give priority due to prefix assignment
                if (preferAltDamage)
                {
                    if (ranged[inver]) { item.melee = false; item.ranged = true; item.magic = false; }
                    if (magic[inver]) { item.melee = false; item.ranged = false; item.magic = true; }
                }
            }
            item.useSound = useSound[index];
            item.useStyle = useStyle[index];
            item.noMelee = noMelee[index];
            item.useTurn = useTurn[index];

            //use original type if none specified
            if (!item.melee && !item.ranged && !item.magic)
            {
                item.melee = melee[inver];
                item.ranged = ranged[inver];
                item.magic = magic[inver];
            }

            if (altFunction) item.autoReuse = false; else item.autoReuse = autoReuse;

            //set prefix to modify numbers
            item.Prefix(item.prefix);
        }
    }
}
