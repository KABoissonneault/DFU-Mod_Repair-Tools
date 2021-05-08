// Project:         RepairTools mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    6/27/2020, 4:00 PM
// Last Edit:		8/1/2020, 12:05 AM
// Version:			1.00
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		Hazelnut

using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;

namespace RepairTools
{
    public class RTTextTokenHolder
    {
        public static TextFile.Token[] ItemRepairTextTokens(uint tokenID, bool toolBroke, DaggerfallUnityItem itemRepaired)
        {
            if (toolBroke)
            {
                switch (tokenID)
                {
                    case 800:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You sharpen the blade of your {itemRepaired.LongName}.",
							"",
                            "The Whetstone has too much wear for further use,",
                            "you discard it.");
                    case 801:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You sew together cuts and loose seams in your {itemRepaired.LongName}.",
                            "",
                            "The Sewing Kit spool has ran out of thread,",
                            "you throw it away.");
                    case 802:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You flatten and reshape dings and dents in your {itemRepaired.LongName}.",
                            "",
                            "The Armorers Hammer handle snaps near the sledge and breaks down completely,",
                            "you toss it to your side.");
                    case 803:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "You bend together broken links and close gaps in your {itemRepaired.LongName}.",
                            "",
                            "The Jewelers Pliers break off at the pivot point.",
                            "You discard them.");
                    case 804:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You glue and reinforce splits and cracks in the frame of your {itemRepaired.LongName}.",
                            "",
                            "The Epoxy Glue bottle has run empty,",
                            "you throw the container away.");
                    case 805:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You sprinkle the strange powder on your {itemRepaired.LongName}. It surges with energy.",
                            "",
                            "The Charging Powder bag has gone empty,",
                            "you toss the empty container away");
                    default:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Text Token Not Found");
                }
            }
            else
            {
                switch (tokenID)
                {
                    case 800:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You sharpen your the blade of your {itemRepaired.LongName}.");
                    case 801:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You sew together cuts and loose seams in your {itemRepaired.LongName}.");
                    case 802:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You flatten and reshape dings and dents in your {itemRepaired.LongName}.");
                    case 803:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You bend together broken links and close gaps in your {itemRepaired.LongName}.");
                    case 804:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You glue and reinforce splits and cracks in the frame of your {itemRepaired.LongName}.");
                    case 805:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            $"You sprinkle the strange powder on your {itemRepaired.LongName}. It surges with energy.");
                    default:
                        return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Text Token Not Found");
                }
            }
        }
    }
}