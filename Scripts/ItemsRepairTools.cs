// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/2/2020, 10:00 PM
// Version:			1.05
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut

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

        public override int DurabilityLoss => 20;

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

        public override int GetRepairPercentage(int luckMod, DaggerfallUnityItem itemToRepair)
        {
            return Random.Range(14 + luckMod, 26 + luckMod);
        }

        public override int GetStaminaDrain(int endurMod)
        {
            return 10 - endurMod;
        }

        public override int GetTimeDrain(int speedMod, int agiliMod)
        {
            return 1800 - (speedMod * 100) - (agiliMod * 50);
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

        public override int DurabilityLoss => 20;

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

        public override int GetRepairPercentage(int luckMod, DaggerfallUnityItem itemToRepair)
        {
            return Random.Range(20 + luckMod, 38 + luckMod);
        }

        public override int GetStaminaDrain(int endurMod)
        {
            return 4;
        }

        public override int GetTimeDrain(int speedMod, int agiliMod)
        {
            return 1800 - (speedMod * 70) - (agiliMod * 80);
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

        public override int DurabilityLoss => 30;

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            return item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron;
        }

        public override int GetRepairPercentage(int luckMod, DaggerfallUnityItem itemToRepair)
        {
            return Random.Range(14 + luckMod, 22 + luckMod);
        }

        public override int GetStaminaDrain(int endurMod)
        {
            return 14 - endurMod;
        }

        public override int GetTimeDrain(int speedMod, int agiliMod)
        {
            return 1800 - (speedMod * 50) - (agiliMod * 30);
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

        public override int DurabilityLoss => 25;

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            // This is using knowledge of the R&R:Items internals and may break if that mod ever changes.
            return item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Chain &&
                item.NativeMaterialValue <= (int)ArmorMaterialTypes.Daedric - 0x100;
        }

        public override int GetRepairPercentage(int luckMod, DaggerfallUnityItem itemToRepair)
        {
            return Random.Range(14 + luckMod, 22 + luckMod);
        }

        public override int GetStaminaDrain(int endurMod)
        {
            return 11 - endurMod;
        }

        public override int GetTimeDrain(int speedMod, int agiliMod)
        {
            return 1800 - (speedMod * 60) - (agiliMod * 50);
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

        public override int DurabilityLoss => 10;

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            DFCareer.Skills skill = item.GetWeaponSkillID();

            return (skill == DFCareer.Skills.BluntWeapon || skill == DFCareer.Skills.Archery);
        }

        public override int GetRepairPercentage(int luckMod, DaggerfallUnityItem itemToRepair)
        {
            return Random.Range(12 + luckMod, 20 + luckMod);
        }

        public override int GetStaminaDrain(int endurMod)
        {
            return 12 - endurMod;
        }

        public override int GetTimeDrain(int speedMod, int agiliMod)
        {
            return 1800 - (speedMod * 40) - (agiliMod * 20);
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

        public override int DurabilityLoss => 20;

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            return item.IsEnchanted;
        }

        public override int GetRepairPercentage(int luckMod, DaggerfallUnityItem itemToRepair)
        {
            int repairPercentage = Random.Range(7 + luckMod, 13 + luckMod);

            // Adds bonus repair value amount with Charging Powder repairing more for staves and adamantium items, etc.
            return (int)Mathf.Round(repairPercentage * GetBonusMultiplier(itemToRepair));
        }

        public override int GetStaminaDrain(int endurMod)
        {
            return 4;
        }

        public override int GetTimeDrain(int speedMod, int agiliMod)
        {
            return 1200 - (speedMod * 20) - (agiliMod * 10);
        }

        private float GetBonusMultiplier(DaggerfallUnityItem item)
        {
            if (item.TemplateIndex == (int)Weapons.Staff)
            {
                if (item.NativeMaterialValue == 2)       // Silver Staff
                    return 2.25f;
                else if (item.NativeMaterialValue == 4)  // Dwarven Staff
                    return 2.50f;
                else if (item.NativeMaterialValue == 6)  // Adamantium Staff
                    return 3.00f;
                else                                // All Other Staves
                    return 1.75f;
            }
            else if (item.TemplateIndex == (int)Weapons.Dagger)
            {
                if (item.NativeMaterialValue == 2)       // Silver Dagger
                    return 1.50f;
                else if (item.NativeMaterialValue == 4)  // Dwarven Dagger
                    return 1.75f;
                else if (item.NativeMaterialValue == 6)  // Adamantium Dagger
                    return 2.00f;
                else                                // All Other Daggers
                    return 1.25f;
            }
            else if (item.NativeMaterialValue == 4)      // Dwarven Item
                return 1.25f;
            else if (item.NativeMaterialValue == 2)      // Silver Item
                return 1.50f;
            else if (item.NativeMaterialValue == 6)      // Adamantium Item
                return 1.75f;
            else if (item.TemplateIndex == (int)Jewellery.Wand)
                return 2.50f;
            else if (item.TemplateIndex == (int)Jewellery.Amulet || TemplateIndex == (int)Jewellery.Torc)
                return 1.50f;
            else if (item.TemplateIndex == (int)Jewellery.Ring)
                return 1.25f;
            else if (item.TemplateIndex == (int)MensClothing.Plain_robes || TemplateIndex == (int)WomensClothing.Plain_robes)
                return 2.00f;
            else if (item.TemplateIndex == (int)MensClothing.Priest_robes || TemplateIndex == (int)WomensClothing.Priestess_robes)
                return 1.25f;

            return 1f;
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

        // Method for calculations and work after a list item has been selected.
        new void RepairItem_OnItemPicked(int index, string itemName)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.UIManager.PopWindow();
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            DaggerfallUnityItem itemToRepair = validRepairItems[index]; // Gets the item object associated with what was selected in the list window.
            ItemProperties props = RepairTools.Instance.GetItemProperties(itemToRepair);

            int luckMod = (int)Mathf.Round((playerEntity.Stats.LiveLuck - 50f) / 10);
            int endurMod = (int)Mathf.Round((playerEntity.Stats.LiveEndurance - 50f) / 10);
            int speedMod = (int)Mathf.Round((playerEntity.Stats.LiveSpeed - 50f) / 10);
            int agiliMod = (int)Mathf.Round((playerEntity.Stats.LiveAgility - 50f) / 10);

            float rechargePercentage = GetRepairPercentage(luckMod, itemToRepair);
            int staminaDrainValue = GetStaminaDrain(endurMod);
            int TimeDrainValue = GetTimeDrain(speedMod, agiliMod);

            int rechargeAmount = (int)Mathf.Round(props.MaxCharge * (rechargePercentage / 100f));
            props.CurrentCharge = Mathf.Min(props.CurrentCharge + rechargeAmount, props.MaxCharge);

            bool toolBroke = currentCondition <= DurabilityLoss;
            LowerConditionWorkaround(DurabilityLoss, playerEntity, repairItemCollection); // Damages repair tool condition.

            // Force inventory window update
            DaggerfallUI.Instance.InventoryWindow.Refresh();

            PlayAudioTrack(); // Plays the appropriate sound effect for a specific repair tool.
            playerEntity.DecreaseFatigue(staminaDrainValue, true); // Reduce player current stamina value from the action of repairing.
            DaggerfallUnity.Instance.WorldTime.Now.RaiseTime(TimeDrainValue); // Forwards time by an amount of minutes in-game time.
            ShowCustomTextBox(toolBroke, itemToRepair, false); // Shows the specific text-box after repairing an item.
        }
    }

    public class ItemMetalScraps : DaggerfallUnityItem
    {
        public const int templateIndex = 810;

        public ItemMetalScraps()
            : base(ItemGroups.UselessItems2, templateIndex)
        {

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

        public ItemClothScraps()
            : base(ItemGroups.UselessItems2, templateIndex)
        {

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

        public ItemWoodScraps()
            : base(ItemGroups.UselessItems2, templateIndex)
        {

        }
        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = "ItemWoodScraps";
            return data;
        }
    }
}

