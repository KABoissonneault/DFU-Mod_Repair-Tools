// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/1/2020, 12:05 AM
// Version:			1.00
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace RepairTools
{
    public enum ScrapsType
    {
        Metal,
        Cloth,
        Wood,
        Soul,
    }

    public struct ScrapsCost
    {
        public ScrapsType Type;
        public int Count;
    }

    public static class ScrapsTypeMethods
    {
        public static ItemGroups GetItemGroups(this ScrapsType type)
        {
            switch (type)
            {
                case ScrapsType.Metal: return ItemMetalScraps.itemGroup;
                case ScrapsType.Cloth: return ItemClothScraps.itemGroup;
                case ScrapsType.Wood: return ItemWoodScraps.itemGroup;
                case ScrapsType.Soul: return ItemSoulCharges.itemGroup;
            }

            throw new System.Exception("That wasn't mean to happen");
        }

        public static int GetTemplateIndex(this ScrapsType type)
        {
            switch (type)
            {
                case ScrapsType.Metal: return ItemMetalScraps.templateIndex;
                case ScrapsType.Cloth: return ItemClothScraps.templateIndex;
                case ScrapsType.Wood: return ItemWoodScraps.templateIndex;
                case ScrapsType.Soul: return ItemSoulCharges.templateIndex;
            }

            throw new System.Exception("That wasn't mean to happen");
        }

        public static string GetLongName(this ScrapsType type)
        {
            switch (type)
            {
                case ScrapsType.Metal: return "Metal Scraps";
                case ScrapsType.Cloth: return "Cloth Scraps";
                case ScrapsType.Wood: return "Wood Scraps";
                case ScrapsType.Soul: return "Soul Crystals";
            }

            throw new System.Exception("That wasn't mean to happen");
        }
    }

    

    /// <summary>
    /// Abstract class for all repair items common behaviour
    /// </summary>
    public abstract class AbstractItemRepairTools : DaggerfallUnityItem
    {
        protected List<DaggerfallUnityItem> validRepairItems = new List<DaggerfallUnityItem>();
        protected readonly UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
        protected ItemCollection repairItemCollection;

        public AbstractItemRepairTools(ItemGroups itemGroup, int templateIndex) : base(itemGroup, templateIndex)
        {
        }

        public abstract uint GetItemID();

        public abstract bool IsValidForRepair(DaggerfallUnityItem item);

        public abstract float GetStaminaDrain(DaggerfallUnityItem item, PlayerEntity player);

        public abstract float GetTimeDrain(DaggerfallUnityItem item, PlayerEntity player);

        public abstract IEnumerable<ScrapsCost> GetScrapsCosts(DaggerfallUnityItem item, PlayerEntity player);

        public int GetAudioClipNum()
        {
            // Clip = 800 - itemId  (may need changing if that assumption becomes invalid)
            return (int) GetItemID() - ItemWhetstone.templateIndex;
        }

        // Depending on which Repair Tool was used, creates the appropriate list of items to display and be picked from.
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

            int playerSkill = RepairTools.GetEffectiveRepairSkill(playerEntity);

            for (int i = 0; i < playerEntity.Items.Count; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i);

                int targetSkill = RepairTools.GetSkillTarget(item);
                int maxPercentage = RepairTools.MaxConditionPercent(playerSkill, targetSkill);

                if (IsValidForRepair(item))
                {
                    IEnumerable<ScrapsCost> scrapsCosts = GetScrapsCosts(item, playerEntity);

                    validRepairItems.Add(item);
                    string validItemName = $"{item.LongName}  {item.ConditionPercentage}% | {maxPercentage} %";

                    if(scrapsCosts.Count() > 0)
                    {
                        ScrapsCost firstCost = scrapsCosts.First();
                        validItemName += $"  ({firstCost.Count} {firstCost.Type}";
                                               
                        foreach(ScrapsCost scrapCost in scrapsCosts.Skip(1))
                        {
                            validItemName += $", {scrapCost.Count} {scrapCost.Type}";
                        }

                        validItemName += ")";
                    }

                    validItemPicker.ListBox.AddItem(validItemName);
                }
            }

            if (validItemPicker.ListBox.Count > 0)
                uiManager.PushWindow(validItemPicker);
            else
                DaggerfallUI.MessageBox("You have no items that can be repaired with this tool.");

            return true;
        }

        protected virtual float GetRepairPercentage(DaggerfallUnityItem itemToRepair, int percentageThreshold)
        {
            int maxRepairThresh = (int)Mathf.Round(itemToRepair.maxCondition * (percentageThreshold / 100f));
            int repairAmount = maxRepairThresh - itemToRepair.currentCondition;
            return (float)repairAmount / itemToRepair.maxCondition;
        }

        protected virtual void DoRepair(DaggerfallUnityItem itemToRepair, int percentageThreshold)
        {
            itemToRepair.currentCondition = percentageThreshold * itemToRepair.maxCondition / 100;
        }
 
        // Method for calculations and work after a list item has been selected.
        public void RepairItem_OnItemPicked(int index, string itemName)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.UIManager.PopWindow();
            DaggerfallUnityItem itemToRepair = validRepairItems[index]; // Gets the item object associated with what was selected in the list window.

            if (itemToRepair.currentCondition <= 0)
            {
                ShowCustomTextBox(GetTooDamagedTokens(itemToRepair)); // Shows the specific text-box when trying to repair a completely broken item.
                return;
            }

            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

            int playerSkill = RepairTools.GetEffectiveRepairSkill(playerEntity);
            int targetSkill = RepairTools.GetSkillTarget(itemToRepair);
            int maxPercentage = RepairTools.MaxConditionPercent(playerSkill, targetSkill);

            if (itemToRepair.ConditionPercentage >= maxPercentage)
            {
                ShowCustomTextBox(GetNotDamagedEnoughTokens(itemToRepair));
                return;
            }
            
            float repairEfficiency = RepairTools.RepairEfficiencyRatio(playerSkill, targetSkill);

            float repairPercentage = GetRepairPercentage(itemToRepair, maxPercentage);

            int staminaDrainValue = (int)(GetStaminaDrain(itemToRepair, playerEntity) * repairPercentage / repairEfficiency * DaggerfallEntity.FatigueMultiplier);
            float timeDrainValue = GetTimeDrain(itemToRepair, playerEntity) * repairPercentage / repairEfficiency;

            if(playerEntity.CurrentFatigue < staminaDrainValue)
            {
                ShowCustomTextBox(GetTooTiredTokens(itemToRepair));
                return;
            }

            var scrapsCosts = GetScrapsCosts(itemToRepair, playerEntity);
            if (!TryConsumeResources(scrapsCosts, playerEntity))
            {
                ShowCustomTextBox(GetNotEnoughScrapsTokens(itemToRepair, scrapsCosts));
                return;
            }

            DoRepair(itemToRepair, maxPercentage);

            bool toolBroke = currentCondition == 1;
            LowerConditionWorkaround(1, playerEntity, repairItemCollection); // Damages repair tool condition.

            // Force inventory window update
            DaggerfallUI.Instance.InventoryWindow.Refresh();

            PlayAudioTrack(); // Plays the appropriate sound effect for a specific repair tool.
            playerEntity.DecreaseFatigue(staminaDrainValue); // Reduce player current stamina value from the action of repairing.
            DaggerfallUnity.Instance.WorldTime.Now.RaiseTime(timeDrainValue); // Forwards time by an amount of minutes in-game time.

            ShowCustomTextBox(RTTextTokenHolder.ItemRepairTextTokens(GetItemID(), toolBroke, itemToRepair));

            if (RepairTools.Instance.DebugLogs)
                Debug.Log($"Skill: {playerSkill}, Target: {targetSkill}, Eff.: {repairEfficiency}, Fatigue: {staminaDrainValue}, Time: {timeDrainValue}");
        }

        private bool TryConsumeResources(IEnumerable<ScrapsCost> scrapsCosts, PlayerEntity player)
        {
            var scrapsItems = new List<DaggerfallUnityItem>();

            foreach(ScrapsCost cost in scrapsCosts)
            {
                var result = player.Items.SearchItems(cost.Type.GetItemGroups(), cost.Type.GetTemplateIndex());
                if (result.Count == 0)
                    return false;

                DaggerfallUnityItem scraps = result[0];
                if (scraps.stackCount < cost.Count)
                    return false;

                scrapsItems.Add(scraps);
            }

            for(int i = 0; i < scrapsCosts.Count(); ++i)
            {
                scrapsItems[i].stackCount -= scrapsCosts.ElementAt(i).Count;
            }

            return true;
        }

        // Like DaggerfallUnityItem's LowerCondition, but without taking DaggerfallUnity.Settings.AllowMagicRepairs into account
        public void LowerConditionWorkaround(int amount, DaggerfallEntity unequipFromOwner = null, ItemCollection removeFromCollectionWhenBreaks = null)
        {
            currentCondition -= amount;
            if (currentCondition <= 0)
            {
                currentCondition = 0;
                ItemBreaks(unequipFromOwner);
                removeFromCollectionWhenBreaks.RemoveItem(this);
            }
        }

        private TextFile.Token[] GetNotDamagedEnoughTokens(DaggerfallUnityItem itemToRepair)
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                $"You cannot improve the condition of this {itemToRepair.LongName} with your current abilities.",
                "You will need to seek the services of a professional.");
        }

        private TextFile.Token[] GetTooDamagedTokens(DaggerfallUnityItem itemToRepair)
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                $"This {itemToRepair.LongName} is damaged beyond your abilities.",
                "You will need to seek the services of a professional.");
        }

        private TextFile.Token[] GetTooTiredTokens(DaggerfallUnityItem itemToRepair)
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                $"You are too exhausted to repair this {itemToRepair.LongName}.");
        }

        private TextFile.Token[] GetNotEnoughScrapsTokens(DaggerfallUnityItem itemToRepair, IEnumerable<ScrapsCost> scrapsCosts)
        {
            TextFile.Token[] messageTokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                $"You do not have the resources to repair this {itemToRepair.LongName}."
                );

            var costLines = scrapsCosts.Select(cost => $"- {cost.Count} {cost.Type.GetLongName()}");
            TextFile.Token[] costTokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyLeft,
                new string[] { "You need at least:" }.Concat(costLines).ToArray()
                );

            return messageTokens.Concat(costTokens).ToArray();
        }

        // Creates the custom text-box after repairing an item.
        private void ShowCustomTextBox(TextFile.Token[] tokens)
        {
            DaggerfallMessageBox itemRepairedText = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
            itemRepairedText.SetTextTokens(tokens);
            itemRepairedText.ClickAnywhereToClose = true;
            uiManager.PushWindow(itemRepairedText);
        }

        // Find the appropriate audio track of the used repair tool, then plays a one-shot of it.
        public void PlayAudioTrack()
        {
            AudioClip clip = RepairTools.Mod.GetAsset<AudioClip>(RepairTools.audioClips[GetAudioClipNum()]);
            RepairTools.Instance.AudioSource.PlayOneShot(clip);
        }
    }
}