// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/2/2020, 10:00 PM
// Version:			1.05
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut

using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallConnect;

namespace RepairTools
{
    //Whetstone
    public class ItemWhetstone : AbstractItemRepairTools
    {
        public const int templateIndex = 800;

        public ItemWhetstone() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetItemID()
        {
            return templateIndex;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemWhetstone).ToString();
            return data;
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            DFCareer.Skills skill = item.GetWeaponSkillID();

            return (skill == DFCareer.Skills.ShortBlade || skill == DFCareer.Skills.LongBlade || skill == DFCareer.Skills.Axe);
        }

        public override IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            DFCareer.Skills skill = item.GetWeaponSkillID();
            if(skill == DFCareer.Skills.Axe)
               return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Metal, Count = 1 }, new ScrapsCost { Type = ScrapsType.Wood, Count = 1 } };
            else
               return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Metal, Count = 2 } };
        }

        public override float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float strengthMod = playerEntity.Stats.LiveStrength / 2f;
            return Mathf.Max(75 - strengthMod, 25);
        }

        public override float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float strengthMod = playerEntity.Stats.LiveStrength * 3;
            float enduranceMod = playerEntity.Stats.LiveEndurance * 6;
            float speedMod = playerEntity.Stats.LiveSpeed * 6;
            return Mathf.Max(3600 - enduranceMod - speedMod - strengthMod, 1200);
        }
    }

    //Sewing Kit
    public class ItemSewingKit : AbstractItemRepairTools
    {
        public const int templateIndex = 801;

        public ItemSewingKit() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetItemID()
        {
            return templateIndex;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemSewingKit).ToString();
            return data;
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            // This is using knowledge of the R&R:Items internals and may break if that mod ever changes.
            return (item.ItemGroup == ItemGroups.Armor
                    && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Leather
                    && item.NativeMaterialValue <= (int)ArmorMaterialTypes.Daedric - 0x200
                || item.ItemGroup == ItemGroups.MensClothing
                || item.ItemGroup == ItemGroups.WomensClothing
                || item.TemplateIndex == 530);
        }

        public override IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Cloth, Count = 2 } };
        }

        public override float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float agiMod = playerEntity.Stats.LiveAgility * 0.3f;
            return Mathf.Max(40 - agiMod, 10);
        }

        public override float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float agiMod = playerEntity.Stats.LiveAgility * 9;
            float speedMod = playerEntity.Stats.LiveSpeed * 6;
            return Mathf.Max(3600 - agiMod - speedMod, 1200);
        }
    }

    //Armorers Hammer
    public class ItemArmorersHammer : AbstractItemRepairTools
    {
        public const int templateIndex = 802;

        public ItemArmorersHammer() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetItemID()
        {
            return templateIndex;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemArmorersHammer).ToString();
            return data;
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            return item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron;
        }

        public override IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Metal, Count = 2 } };
        }

        public override float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float strengthMod = playerEntity.Stats.LiveStrength / 4f;
            float agiMod = playerEntity.Stats.LiveAgility / 4f;
            return Mathf.Max(75 - strengthMod - agiMod, 25);
        }

        public override float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float strengthMod = playerEntity.Stats.LiveStrength * 6;
            float enduranceMod = playerEntity.Stats.LiveEndurance * 6;
            float speedMod = playerEntity.Stats.LiveSpeed * 3;
            return Mathf.Max(3600 - enduranceMod - speedMod - strengthMod, 1200);
        }
    }

    //Jewelers Pliers
    public class ItemJewelersPliers : AbstractItemRepairTools
    {
        public const int templateIndex = 803;

        public ItemJewelersPliers() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetItemID()
        {
            return templateIndex;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemJewelersPliers).ToString();
            return data;
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            // This is using knowledge of the R&R:Items internals and may break if that mod ever changes.
            return item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Chain &&
                item.NativeMaterialValue <= (int)ArmorMaterialTypes.Daedric - 0x100;
        }

        public override IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Metal, Count = 2 } };
        }

        public override float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float agiMod = playerEntity.Stats.LiveAgility * 0.3f;
            return Mathf.Max(40 - agiMod, 10);
        }

        public override float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float agiMod = playerEntity.Stats.LiveAgility * 9;
            float speedMod = playerEntity.Stats.LiveSpeed * 6;
            return Mathf.Max(3600 - agiMod - speedMod, 1200);
        }
    }

    //Epoxy Glue
    public class ItemEpoxyGlue : AbstractItemRepairTools
    {
        public const int templateIndex = 804;

        public ItemEpoxyGlue() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetItemID()
        {
            return templateIndex;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemEpoxyGlue).ToString();
            return data;
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            DFCareer.Skills skill = item.GetWeaponSkillID();

            return (skill == DFCareer.Skills.BluntWeapon || skill == DFCareer.Skills.Archery);
        }

        public override IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            DFCareer.Skills skill = item.GetWeaponSkillID();
            if (skill == DFCareer.Skills.Archery)
            {
                return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Wood, Count = 1 }, new ScrapsCost { Type = ScrapsType.Cloth, Count = 1 } };
            }
            else
            {
                return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Wood, Count = 2 } };
            }
        }

        public override float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float agiMod = playerEntity.Stats.LiveAgility * 0.3f;
            return Mathf.Max(40 - agiMod, 10);
        }

        public override float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float agiMod = playerEntity.Stats.LiveAgility * 9;
            float speedMod = playerEntity.Stats.LiveSpeed * 6;
            return Mathf.Max(3600 - agiMod - speedMod, 1200);
        }
    }

    //Charging Powder
    public class ItemChargingPowder : AbstractItemRepairTools
    {
        public const int templateIndex = 805;

        public ItemChargingPowder() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetItemID()
        {
            return templateIndex;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemChargingPowder).ToString();
            return data;
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            return item.IsEnchanted;
        }

        public override IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            return new ScrapsCost[] { new ScrapsCost { Type = ScrapsType.Soul, Count = 2 } };
        }

        public override float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float intMod = playerEntity.Stats.LiveIntelligence * 0.3f;
            return Mathf.Max(40 - intMod, 10) / RepairTools.GetEnchantmentBonusMultipler(item);
        }

        public override float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity playerEntity)
        {
            float intMod = playerEntity.Stats.LiveIntelligence * 6f;
            float wilMod = playerEntity.Stats.LiveWillpower * 6f;
            float speedMod = playerEntity.Stats.LiveSpeed * 3f;
            return Mathf.Max(3600 - intMod - wilMod - speedMod, 1200) / RepairTools.GetEnchantmentBonusMultipler(item);
        }

        protected override void DoRepair(DaggerfallUnityItem itemToRepair, int percentageThreshold)
        {
            ItemProperties props = RepairTools.Instance.GetItemProperties(itemToRepair);
            props.CurrentCharge = (int)Mathf.Round(props.MaxCharge * (percentageThreshold / 100f));
        }

        public override bool UseItem(ItemCollection collection)
        {
            if (GameManager.Instance.AreEnemiesNearby(true))
            {
                DaggerfallUI.MessageBox("Can't use that with enemies around.");
                return true;
            }
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            if (playerEntity.CurrentFatigue <= (20 * DaggerfallEntity.FatigueMultiplier))
            {
                DaggerfallUI.MessageBox("You are too exhausted to do that.");
                return true;
            }

            repairItemCollection = collection;

            DaggerfallListPickerWindow validItemPicker = new DaggerfallListPickerWindow(uiManager, uiManager.TopWindow);
            validItemPicker.OnItemPicked += RepairItem_OnItemPicked;
            validRepairItems.Clear(); // Clears the valid item list before every repair tool use.

            for (int i = 0; i < playerEntity.Items.Count; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i);
                ItemProperties props = RepairTools.Instance.GetItemProperties(item);

                if (props.CurrentCharge != props.MaxCharge && IsValidForRepair(item))
                {
                    validRepairItems.Add(item);
                    string validItemName = $"{props.CurrentCharge} / {props.MaxCharge}  {item.LongName}";
                    validItemPicker.ListBox.AddItem(validItemName);
                }
            }

            if (validItemPicker.ListBox.Count > 0)
                uiManager.PushWindow(validItemPicker);
            else
                DaggerfallUI.MessageBox("You have no such items in need of repair.");

            return true;
        }
    }

    public class ItemMetalScraps : DaggerfallUnityItem
    {
        public const int templateIndex = 810;
        public const ItemGroups itemGroup = ItemGroups.UselessItems2;

        public ItemMetalScraps()
            : base(itemGroup, templateIndex)
        {

        }

        public override bool IsStackable()
        {
            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = "ItemMetalScraps";
            return data;
        }
    }

    public class ItemClothScraps : DaggerfallUnityItem
    {
        public const int templateIndex = 811;
        public const ItemGroups itemGroup = ItemGroups.UselessItems2;

        public ItemClothScraps()
            : base(itemGroup, templateIndex)
        {

        }

        public override bool IsStackable()
        {
            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = "ItemClothScraps";
            return data;
        }
    }

    public class ItemWoodScraps : DaggerfallUnityItem
    {
        public const int templateIndex = 812;
        public const ItemGroups itemGroup = ItemGroups.UselessItems2;

        public ItemWoodScraps()
            : base(itemGroup, templateIndex)
        {

        }

        public override bool IsStackable()
        {
            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = "ItemWoodScraps";
            return data;
        }
    }

    public class ItemSoulCharges : DaggerfallUnityItem
    {
        public const int templateIndex = 813;
        public const ItemGroups itemGroup = ItemGroups.UselessItems2;

        public ItemSoulCharges()
            : base(itemGroup, templateIndex)
        {

        }

        public override bool IsStackable()
        {
            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = "ItemSoulCharges";
            return data;
        }
    }
}

