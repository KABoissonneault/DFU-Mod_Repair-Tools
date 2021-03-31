// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/2/2020, 10:00 PM
// Version:			1.05
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut	

using System.Linq;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Entity;

namespace RepairTools
{
    public class RepairTools : MonoBehaviour
    {
        static RepairTools instance;

        public static RepairTools Instance
        {
            get { return instance ?? (instance = FindObjectOfType<RepairTools>()); }
        }

        static Mod mod;

        public static Mod Mod { get { return mod; } }

        bool debugLogs;
        public AudioSource audioSource;

        public bool DebugLogs { get { return debugLogs; } }
        public AudioSource AudioSource {  get { return audioSource; } }

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
            var go = new GameObject("RepairTools");
            instance = go.AddComponent<RepairTools>(); // Add script to the scene.
            instance.audioSource = go.AddComponent<AudioSource>();
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
            Debug.Log("Begin mod init: RepairTools");

            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;

            itemHelper.RegisterCustomItem(ItemWhetstone.templateIndex, ItemGroups.UselessItems2, typeof(ItemWhetstone));
            itemHelper.RegisterCustomItem(ItemSewingKit.templateIndex, ItemGroups.UselessItems2, typeof(ItemSewingKit));
            itemHelper.RegisterCustomItem(ItemArmorersHammer.templateIndex, ItemGroups.UselessItems2, typeof(ItemArmorersHammer));
            itemHelper.RegisterCustomItem(ItemJewelersPliers.templateIndex, ItemGroups.UselessItems2, typeof(ItemJewelersPliers));
            itemHelper.RegisterCustomItem(ItemEpoxyGlue.templateIndex, ItemGroups.UselessItems2, typeof(ItemEpoxyGlue));
            itemHelper.RegisterCustomItem(ItemChargingPowder.templateIndex, ItemGroups.UselessItems2, typeof(ItemChargingPowder));

            Debug.Log("Finished mod init: RepairTools");
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

    }
}