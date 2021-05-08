// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/2/2020, 10:00 PM
// Version:			1.05
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut, Kab the Bird Ranger	

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
        public IEnumerable<KeyValuePair<ulong, ItemProperties>> ItemProperties { get { return itemProperties; } }

        #region Item Properties

        void InitializeProperties(ItemProperties props, DaggerfallUnityItem item)
        {
            if (!item.IsEnchanted)
                return;

            props.MaxCharge = Mathf.RoundToInt(item.ItemTemplate.hitPoints * GetEnchantmentBonusMultipler(item));
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
        }

        void Awake()
        {
            ModSettings settings = mod.GetSettings();
            debugLogs = settings.GetBool("Core", "DebugLogs");

            InitMod();

            mod.SaveDataInterface = instance;
            mod.MessageReceiver = MessageReceiver;
            mod.IsReady = true;
        }

        #region InitMod and Settings

        private void InitMod()
        {
            Debug.Log("Begin mod init: Greater Condition");

            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;

            itemHelper.RegisterCustomItem(ItemWhetstone.templateIndex, ItemGroups.UselessItems2, typeof(ItemWhetstone));
            itemHelper.RegisterCustomItem(ItemSewingKit.templateIndex, ItemGroups.UselessItems2, typeof(ItemSewingKit));
            itemHelper.RegisterCustomItem(ItemArmorersHammer.templateIndex, ItemGroups.UselessItems2, typeof(ItemArmorersHammer));
            itemHelper.RegisterCustomItem(ItemJewelersPliers.templateIndex, ItemGroups.UselessItems2, typeof(ItemJewelersPliers));
            itemHelper.RegisterCustomItem(ItemEpoxyGlue.templateIndex, ItemGroups.UselessItems2, typeof(ItemEpoxyGlue));
            itemHelper.RegisterCustomItem(ItemChargingPowder.templateIndex, ItemGroups.UselessItems2, typeof(ItemChargingPowder));

            itemHelper.RegisterCustomItem(ItemMetalScraps.templateIndex, ItemGroups.MiscItems, typeof(ItemMetalScraps));
            itemHelper.RegisterCustomItem(ItemClothScraps.templateIndex, ItemGroups.MiscItems, typeof(ItemClothScraps));
            itemHelper.RegisterCustomItem(ItemWoodScraps.templateIndex, ItemGroups.MiscItems, typeof(ItemWoodScraps));

            // Replace CastWhenHeld, CastWhenStrikes, and CastWhenUsed item effects to use magic charge instead of durability
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(new RepairToolsCastWhenHeld(), true);
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(new RepairToolsCastWhenStrikes(), true);
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(new RepairToolsCastWhenUsed(), true);

            // Custom windows
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(RepairToolsInventoryWindow));

            // New drops
            EnemyEntity.OnLootSpawned += OnEnemyLootSpawned;
            LootTables.OnLootSpawned += OnLootPileSpawned;

            Debug.Log("Finished mod init: Greater Condition");
        }

        void MessageReceiver(string message, object data, DFModMessageCallback callback)
        {
            switch(message)
            {
                case "GetMaxCharge":
                    GetMaxCharge(data, callback);
                    break;

                case "GetCurrentCharge":
                    GetCurrentCharge(data, callback);
                    break;

                case "RechargeItem":
                    RechargeItem(data, callback);
                    break;

                default:
                    Debug.LogError($"Greater Condition: Unknown message '{message}'");
                    break;
            }
        }

        void OnEnemyLootSpawned(object sender, EnemyLootSpawnedEventArgs e)
        {            
            if(IsHumanoid(e.MobileEnemy))
            {
                DaggerfallUnityItem drop = RandomScrap(e.MobileEnemy.Level);
                if (drop != null)
                {
                    e.Items.AddItem(drop);
                }
            }
        }

        void OnLootPileSpawned(object sender, TabledLootSpawnedEventArgs e)
        {
            // Orc Stronghold
            // Human Stronghold
            // Prison
            // Ruined Castle
            // Barbarian Stronghold
            // Cemetery
            // Giant Stronghold
            if (e.Key == "N" || e.Key == "F")
            {
                int level = GameManager.Instance.PlayerEntity.Level;
                DaggerfallUnityItem drop = RandomScrap(level);
                if (drop != null)
                {
                    e.Items.AddItem(drop);
                }
            }
            // Coven
            // Laboratory
            else if (e.Key == "Q" || e.Key == "U")
            {
                int level = Math.Max(Math.Min(GameManager.Instance.PlayerEntity.Level, 20) - 1, 1);
                DaggerfallUnityItem drop = RandomScrap(level);
                if (drop != null)
                {
                    e.Items.AddItem(drop);
                }
            }
            // Mine
            // Natural Cave
            // Volcanic Cave
            // Crypt
            // Desecrated Temple
            // Vampire Haunt
            else if (e.Key == "M" || e.Key == "K")
            {
                int level = Math.Max(Math.Min(GameManager.Instance.PlayerEntity.Level, 20) - 2, 1);
                DaggerfallUnityItem drop = RandomScrap(level);
                if (drop != null)
                {
                    e.Items.AddItem(drop);
                }
            }
        }

        bool IsHumanoid(MobileEnemy mobileEnemy)
        {
            if (mobileEnemy.Affinity == MobileAffinity.Human)
                return true;

            switch(mobileEnemy.ID)
            {
                case (int)MobileTypes.Orc:
                case (int)MobileTypes.Centaur:
                case (int)MobileTypes.OrcSergeant:
                case (int)MobileTypes.Giant:
                case (int)MobileTypes.OrcShaman:
                case (int)MobileTypes.OrcWarlord:
                case (int)MobileTypes.Zombie:
                case (int)MobileTypes.AncientLich:
                case (int)MobileTypes.Lich:
                case (int)MobileTypes.Vampire:
                case (int)MobileTypes.VampireAncient:
                    return true;
            }

            return false;
        }

        DaggerfallUnityItem RandomScrap(int level)
        {
            level = Math.Max(level, 1);

            int templateIndex;

            switch(UnityEngine.Random.Range(0, 30))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    templateIndex = ItemMetalScraps.templateIndex;
                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                    templateIndex = ItemClothScraps.templateIndex;
                    break;
                case 12:
                case 13:
                    templateIndex = ItemWoodScraps.templateIndex;
                    break;
                case 14:
                    templateIndex = ItemSoulCharges.templateIndex;
                    break;
                default:
                    return null;
            }

            int roll = UnityEngine.Random.Range(0, 20);
            int levelIndex = Mathf.Min(level, 20) - 1;
            int dropQuality = ScrapDropTable[levelIndex, roll];
            if (dropQuality == 0)
                return null;

            DaggerfallUnityItem scraps = ItemBuilder.CreateItem(ItemGroups.MiscItems, templateIndex);
            scraps.stackCount = dropQuality;
            return scraps;
        }

        void GetMaxCharge(object data, DFModMessageCallback callback)
        {
            ulong? arg = (ulong?)data;
            if(arg == null)
            {
                Debug.LogError("Greater Condition: GetMaxCharge argument not a valid ulong");
                callback?.Invoke("GetMaxCharge", 0);
                return;
            }

            ulong uid = arg.Value;
            ItemProperties props;
            if(!itemProperties.TryGetValue(uid, out props))
            {
                callback?.Invoke("GetMaxCharge", 0);
                return;
            }

            callback?.Invoke("GetMaxCharge", props.MaxCharge);
        }

        void GetCurrentCharge(object data, DFModMessageCallback callback)
        {
            ulong? arg = (ulong?)data;
            if (arg == null)
            {
                Debug.LogError("Greater Condition: GetCurrentCharge argument not a valid ulong");
                callback?.Invoke("GetCurrentCharge", 0);
                return;
            }

            ulong uid = arg.Value;
            ItemProperties props;
            if (!itemProperties.TryGetValue(uid, out props))
            {
                callback?.Invoke("GetCurrentCharge", 0);
                return;
            }

            callback?.Invoke("GetCurrentCharge", props.CurrentCharge);
        }

        void RechargeItem(object data, DFModMessageCallback callback)
        {
            ulong? arg = (ulong?)data;
            if (arg == null)
            {
                Debug.LogError("Greater Condition: RechargeItem argument not a valid ulong");
                return;
            }

            ulong uid = arg.Value;
            ItemProperties props;
            if (!itemProperties.TryGetValue(uid, out props))
            {
                return;
            }

            props.CurrentCharge = props.MaxCharge;
        }

        #endregion

        #region Formulas
        public static int GetEffectiveRepairSkill(PlayerEntity playerEntity)
        {
            int backgroundMod = playerEntity.Career.Name == "Knight" ? 6 : 0; // knights get +6 because of their backstory
            return Mathf.Clamp(playerEntity.Level * 5, 5, 100) + backgroundMod;
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

        public static float GetEnchantmentBonusMultipler(DaggerfallUnityItem item)
        {
            if (item.TemplateIndex == (int)Weapons.Staff)
            {
                if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Elven)
                    return 2.25f;
                else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                    return 2.50f;
                else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Adamantium)
                    return 3.00f;
                else
                    return 1.75f;
            }
            else if (item.TemplateIndex == (int)Weapons.Dagger)
            {
                if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Elven)
                    return 1.50f;
                else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                    return 1.75f;
                else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Adamantium)
                    return 2.00f;
                else
                    return 1.25f;
            }
            else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Elven)
                return 1.25f;
            else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                return 1.50f;
            else if (item.NativeMaterialValue == (int)WeaponMaterialTypes.Adamantium)
                return 1.75f;
            else if (item.TemplateIndex == (int)Jewellery.Mark || item.TemplateIndex == (int)Jewellery.Wand)
                return 2.50f;
            else if (item.TemplateIndex == (int)Jewellery.Amulet || item.TemplateIndex == (int)Jewellery.Torc || item.TemplateIndex == (int)Jewellery.Cloth_amulet)
                return 1.50f;
            else if (item.TemplateIndex == (int)Jewellery.Ring)
                return 1.25f;
            else if (item.TemplateIndex == (int)MensClothing.Plain_robes || item.TemplateIndex == (int)WomensClothing.Plain_robes)
                return 2.00f;
            else if (item.TemplateIndex == (int)MensClothing.Priest_robes || item.TemplateIndex == (int)WomensClothing.Priestess_robes)
                return 1.25f;

            return 1f;
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

        #region Tables
        int[,] ScrapDropTable =
        {
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1 }
            , { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1 }
            , { 0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1 }
            , { 0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1 }
            , { 0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }
            , { 0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2 }
            , { 0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2 }
            , { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2 }
            , { 1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2 }
            , { 1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2 }
            , { 1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,3,3 }
            , { 1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,3,3,3 }
            , { 1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,3,3,3,3 }
            , { 1,1,1,1,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3,3 }
            , { 1,1,1,1,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3 }
            , { 1,1,1,1,2,2,2,2,2,2,3,3,3,3,3,3,3,3,4,4 }
            , { 1,1,1,1,2,2,2,2,3,3,3,3,3,3,3,3,3,4,4,4 }
            , { 1,1,1,1,2,2,2,2,3,3,3,3,3,3,4,4,4,4,4,4 }
            , { 1,1,1,1,2,2,2,2,3,3,3,3,4,4,4,4,4,4,4,5 }
            , { 1,1,1,1,2,2,2,2,3,3,3,3,4,4,4,4,5,5,5,5 }
        };
        #endregion
    }
}