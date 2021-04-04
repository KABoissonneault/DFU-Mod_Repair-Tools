// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/2/2020, 10:00 PM
// Version:			1.05
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut	

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace RepairTools
{
    public class ItemProperties
    {
        public int CurrentCharge { get; set; }
        public int MaxCharge { get; set; }

        public bool IsDefault()
        {
            return CurrentCharge == 0 && MaxCharge == 0;
        }
    }

    [FullSerializer.fsObject("v1")]
    public class RepairToolsSaveData
    {
        public List<KeyValuePair<ulong, ItemProperties>> ItemList;

        public RepairToolsSaveData(IEnumerable<KeyValuePair<ulong, ItemProperties>> items)
        {
            // Filter out items the player doesn't have anymore
            items = items.Where(
                kvp => RepairTools.PlayerItemExists(kvp.Key)
                && !kvp.Value.IsDefault()
                );

            ItemList = new List<KeyValuePair<ulong, ItemProperties>>(items);
        }
    }

    public class RepairTools : MonoBehaviour, IHasModSaveData
    {
        static RepairTools instance;

        public static RepairTools Instance
        {
            get { return instance; }
        }

        static Mod mod;

        public static Mod Mod { get { return mod; } }

        bool debugLogs;
        public AudioSource audioSource;
        Dictionary<ulong, ItemProperties> itemProperties = new Dictionary<ulong, ItemProperties>();

        public bool DebugLogs { get { return debugLogs; } }
        public AudioSource AudioSource {  get { return audioSource; } }

        #region Item Properties

        void InitializeProperties(ItemProperties props, DaggerfallUnityItem item)
        {
            if (!item.IsEnchanted)
                return;

            props.MaxCharge = item.ItemTemplate­.hitPoints;
            props.CurrentCharge = props.MaxCharge;
        }

        public ItemProperties GetItemProperties(DaggerfallUnityItem item)
        {
            ItemProperties props;
            if (itemProperties.TryGetValue(item.UID, out props))
                return props;

            props = new ItemProperties();
            InitializeProperties(props, item);
            itemProperties.Add(item.UID, props);
            return props;
        }

        static bool FindInCollection(ulong uid, ItemCollection collection, out DaggerfallUnityItem item)
        {
            item = collection.GetItem(uid);
            return item != null;
        }

        static bool FindInRange(ulong uid, IEnumerable<DaggerfallUnityItem> collection, out DaggerfallUnityItem item)
        {
            item = collection.FirstOrDefault(i => i.UID == uid);
            return item != null;
        }

        public static bool FindPlayerItem(ulong uid, out DaggerfallUnityItem item)
        {
            var playerEntity = GameManager.Instance.PlayerEntity;
            return FindInCollection(uid, playerEntity.Items, out item)
                || FindInRange(uid, playerEntity.ItemEquipTable.EquipTable, out item)
                || FindInCollection(uid, playerEntity.WagonItems, out item)
                || FindInCollection(uid, playerEntity.OtherItems, out item);
        }

        public static bool PlayerItemExists(ulong uid)
        {
            DaggerfallUnityItem dummy;
            return FindPlayerItem(uid, out dummy);
        }

        #endregion

        //list of audio clip assets bundled in mod
        public static readonly List<string> audioClips = new List<string>()
        {
            "Blade Sharpen WhetStone 1.mp3",
            "Sewing Kit Repair 1.mp3",
            "Armorers Hammer Repair 1.mp3",
            "Jewelers Pliers Repair 1.mp3",
            "Epoxy Glue Repair 1.mp3",
            "Charging Powder Repair 1.mp3"
        };

        void Start()
        {
            RepairToolsConsoleCommands.RegisterCommands();
        }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("GreaterCondition");
            instance = go.AddComponent<RepairTools>(); // Add script to the scene.
            instance.audioSource = go.AddComponent<AudioSource>();

            mod.SaveDataInterface = instance;
        }

        void Awake()
        {
            ModSettings settings = mod.GetSettings();
            debugLogs = settings.GetBool("Core", "DebugLogs");

            InitMod();

            mod.IsReady = true;
        }

        #region InitMod and Settings

        private static void InitMod()
        {
            Debug.Log("Begin mod init: Greater Condition");

            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;

            itemHelper.RegisterCustomItem(ItemWhetstone.templateIndex, ItemGroups.UselessItems2, typeof(ItemWhetstone));
            itemHelper.RegisterCustomItem(ItemSewingKit.templateIndex, ItemGroups.UselessItems2, typeof(ItemSewingKit));
            itemHelper.RegisterCustomItem(ItemArmorersHammer.templateIndex, ItemGroups.UselessItems2, typeof(ItemArmorersHammer));
            itemHelper.RegisterCustomItem(ItemJewelersPliers.templateIndex, ItemGroups.UselessItems2, typeof(ItemJewelersPliers));
            itemHelper.RegisterCustomItem(ItemEpoxyGlue.templateIndex, ItemGroups.UselessItems2, typeof(ItemEpoxyGlue));
            itemHelper.RegisterCustomItem(ItemChargingPowder.templateIndex, ItemGroups.UselessItems2, typeof(ItemChargingPowder));

            // Replace CastWhenHeld, CastWhenStrikes, and CastWhenUsed item effects to use magic charge instead of durability
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(new RepairToolsCastWhenHeld(), true);
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(new RepairToolsCastWhenStrikes(), true);
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(new RepairToolsCastWhenUsed(), true);

            // Custom windows
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(RepairToolsInventoryWindow));

            Debug.Log("Finished mod init: Greater Condition");
        }

        #endregion

        #region Formulas
        public static int GetEffectiveRepairSkill(PlayerEntity playerEntity)
        {
            int luckMod = (int)Mathf.Round((playerEntity.Stats.LiveLuck - 50f) / 10);
            int agiMod = (int)Mathf.Round((playerEntity.Stats.LiveAgility - 50f) / 10);
            int backgroundMod = playerEntity.Career.Name == "Knight" ? 6 : 0; // knights get +6 because of their backstory
            return Mathf.Clamp(playerEntity.Level * 5, 5, 100) + luckMod + agiMod + backgroundMod;
        }

        public static int GetSkillTarget(DaggerfallUnityItem item)
        {
            if(item.ItemGroup == ItemGroups.Weapons)
            {
                return (item.NativeMaterialValue + 1) * 10;
            }
            else if(item.ItemGroup == ItemGroups.Armor)
            {
                if(item.NativeMaterialValue < (int)ArmorMaterialTypes.Chain)
                {
                    // Leather armor
                    return (item.NativeMaterialValue + 1) * 10;
                }
                else if(item.NativeMaterialValue < (int)ArmorMaterialTypes.Iron)
                {
                    // Chain armor
                    return (item.NativeMaterialValue - (int)ArmorMaterialTypes.Chain + 1) * 10;
                }
                else
                {
                    // Plate
                    return (item.NativeMaterialValue - (int)ArmorMaterialTypes.Iron + 1) * 10;
                }
            }
            else
            {
                return 10;
            }
        }

        // Returns a value between 20 and 100% (exclusive)
        // At 5 under target, you get 40%
        // On target, you get 60%
        // At 5 above target, you get 80%
        public static int MaxConditionPercent(int effectiveSkill, int targetSkill)
        {
            const float a = 80f / Mathf.PI;
            const float b = 60f;
            const float c = 5f;

            int diff = Mathf.Max(effectiveSkill - targetSkill, -10);
            float r = a * Mathf.Atan(diff / c) + b;
            return (int)Mathf.Round(r);
        }

        // Returns a value between 0.5 and 1.5 (exclusive)
        // At 5 under target, you get 0.75
        // On target, you get 1
        // At 5 above target, you get 1.25
        public static int RepairEfficiencyRatio(int effectiveSkill, int targetSkill)
        {
            const float a = 1f / Mathf.PI;
            const float b = 1f;
            const float c = 5f;

            int diff = Mathf.Max(effectiveSkill - targetSkill, -10);
            float r = a * Mathf.Atan(diff / c) + b;
            return (int)Mathf.Round(r);
        }

        #endregion

        #region Save Data
        public Type SaveDataType { get { return typeof(RepairToolsSaveData); } }

        public object NewSaveData()
        {
            return new RepairToolsSaveData(Enumerable.Empty<KeyValuePair<ulong, ItemProperties>>());
        }

        public object GetSaveData()
        {
            return new RepairToolsSaveData(itemProperties);
        }

        public void RestoreSaveData(object saveData)
        {
            var repairToolsSaveData = (RepairToolsSaveData)saveData;
            if (repairToolsSaveData.ItemList == null || repairToolsSaveData.ItemList.Count() == 0)
            {
                itemProperties = new Dictionary<ulong, ItemProperties>();
            }
            else
            {
                itemProperties = repairToolsSaveData.ItemList.ToDictionary(x => x.Key, x => x.Value);
            }
        }
        #endregion

        #region Utils
        public static bool IsTraveling()
        {
            if (GameManager.Instance.EntityEffectBroker.SyntheticTimeIncrease)
                return true;

            bool isTraveling = false;
            ModManager.Instance.SendModMessage("TravelOptions", "isTravelActive", null, (string _, object data) => isTraveling = (bool)data);
            return isTraveling;
        }
        #endregion

    }
}