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
using UnityEngine;
using Wenzil.Console;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;

namespace RepairTools
{
    public static class RepairToolsConsoleCommands
    {
        static int[] ToolTemplateIndices = {
            ItemWhetstone.templateIndex,
            ItemSewingKit.templateIndex,
            ItemArmorersHammer.templateIndex,
            ItemJewelersPliers.templateIndex,
            ItemEpoxyGlue.templateIndex,
            ItemChargingPowder.templateIndex
        };

        public static void RegisterCommands()
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(DamageEquipment.name, DamageEquipment.description, DamageEquipment.usage, DamageEquipment.Execute);
                ConsoleCommandsDatabase.RegisterCommand(RepairEquipment.name, RepairEquipment.description, RepairEquipment.usage, RepairEquipment.Execute);
                ConsoleCommandsDatabase.RegisterCommand(ClearInventory.name, ClearInventory.description, ClearInventory.usage, ClearInventory.Execute);
                ConsoleCommandsDatabase.RegisterCommand(AddRepairTools.name, AddRepairTools.description, AddRepairTools.usage, AddRepairTools.Execute);
                ConsoleCommandsDatabase.RegisterCommand(RechargeAllItems.name, RechargeAllItems.description, RechargeAllItems.usage, RechargeAllItems.Execute);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering RepairTools Console commands: {0}", e.Message));
            }
        }

        private static class DamageEquipment
        {
            public static readonly string name = "damage_equip";
            public static readonly string description = "Damages All Equipment In Inventory By 10% Per Use, For Testing";
            public static readonly string usage = "DAMAGE_EQUIP";

            public static string Execute(params string[] args)
            {
                PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

                for (int i = 0; i < playerEntity.Items.Count; i++)
                {
                    DaggerfallUnityItem item = playerEntity.Items.GetItem(i);

                    // Don't damage our repair tools, this is just annoying
                    if (ToolTemplateIndices.Contains(item.TemplateIndex))
                        continue;

                    int percentReduce = (int)Mathf.Ceil(item.maxCondition * 0.10f);
                    item.LowerCondition(percentReduce);
                }

                return "All items damaged by 10%";
            }
        }

        private static class RepairEquipment
        {
            public static readonly string name = "repair_equip";
            public static readonly string description = "Repairs All Equipment In Inventory By 10% Per Use, For Testing";
            public static readonly string usage = "REPAIR_EQUIP";

            public static string Execute(params string[] args)
            {
                PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

                for (int i = 0; i < playerEntity.Items.Count; i++)
                {
                    DaggerfallUnityItem item = playerEntity.Items.GetItem(i);

                    int percentIncrease = (int)Mathf.Ceil(item.maxCondition * 0.10f);
                    int repairAmount = (int)Mathf.Ceil(item.maxCondition * (percentIncrease / 100f));

                    item.currentCondition = Mathf.Min(item.currentCondition + repairAmount, item.maxCondition);
                }

                return "All items repaired by 10%";
            }
        }

        private static class ClearInventory
        {
            public static readonly string name = "clear_inventory";
            public static readonly string description = "Delete The Entire Current Inventory Of The Player, For Testing";
            public static readonly string usage = "CLEAR_INVENTORY";

            public static string Execute(params string[] args)
            {
                PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                ItemCollection itemCollection = playerEntity.Items;

                for (int i = 0; i < playerEntity.Items.Count; i++)
                {
                    DaggerfallUnityItem item = playerEntity.Items.GetItem(i);
                    itemCollection.RemoveItem(item);
                }

                // Force inventory window update
                DaggerfallUI.Instance.InventoryWindow.Refresh();

                return "Inventory has been cleared";
            }
        }

        private static class AddRepairTools
        {
            public static readonly string name = "add_repairtools";
            public static readonly string description = "Adds one of each repair tool to the player's inventory";
            public static readonly string usage = "ADD_REPAIRTOOLS";

            public static string Execute(params string[] args)
            {
                foreach (int templateIndex in ToolTemplateIndices)
                {
                    DaggerfallUnityItem item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, templateIndex);
                    GameManager.Instance.PlayerEntity.Items.AddItem(item);
                }

                return "Repair tools have been added";
            }
        }

        private static class RechargeAllItems
        {
            public static readonly string name = "recharge_items";
            public static readonly string description = "Refills all magic items' charge";
            public static readonly string usage = "RECHARGE_ITEMS";

            public static string Execute(params string[] args)
            {
                foreach(var kvp in RepairTools.Instance.ItemProperties)
                {
                    kvp.Value.CurrentCharge = kvp.Value.MaxCharge;
                }

                return "Magic items have been recharged";
            }

        }
    }
}
