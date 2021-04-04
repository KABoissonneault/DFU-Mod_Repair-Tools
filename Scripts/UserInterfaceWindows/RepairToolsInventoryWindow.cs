using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Items;


namespace RepairTools
{
    /// <summary>
    /// Implements inventory window.
    /// </summary>
    public class RepairToolsInventoryWindow : DaggerfallInventoryWindow
    {
        #region Constructors

        public RepairToolsInventoryWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {
        }

        #endregion

        #region Private Methods
        void UpdateItemInfoPanel(DaggerfallUnityItem item)
        {
            // Display info in local target icon panel, replacing justification tokens
            TextFile.Token[] tokens = ItemHelper.GetItemInfo(item, DaggerfallUnity.TextProvider);
            tokens = ModifyItemInfoTokens(tokens, item);
            MacroHelper.ExpandMacros(ref tokens, item);

            // Only keep the title part for paintings
            if (item.ItemGroup == ItemGroups.Paintings)
                tokens = new TextFile.Token[] { new TextFile.Token() { formatting = TextFile.Formatting.Text, text = tokens[tokens.Length - 1].text.Trim() } };

            UpdateItemInfoPanel(tokens);
        }

        private void UpdateItemInfoPanel(TextFile.Token[] tokens)
        {
            for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
            {
                if (tokens[tokenIdx].formatting == TextFile.Formatting.JustifyCenter)
                    tokens[tokenIdx].formatting = TextFile.Formatting.NewLine;
                if (tokens[tokenIdx].text != null)
                    tokens[tokenIdx].text = tokens[tokenIdx].text.Replace(kgSrc, kgRep).Replace(damSrc, damRep).Replace(arSrc, arRep);
            }
            itemInfoPanelLabel.SetText(tokens);
        }
        #endregion

        #region Item Click Event Handlers
        protected override void AccessoryItemsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            if(selectedActionMode == ActionModes.Info)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                // Get item
                EquipSlots slot = (EquipSlots)sender.Tag;
                DaggerfallUnityItem item = playerEntity.ItemEquipTable.GetItem(slot);
                if (item == null)
                    return;

                ShowInfoPopup(item);
            }
            else
            {
                base.AccessoryItemsButton_OnMouseClick(sender, position);
            }
        }

        protected override void AccessoryItemsButton_OnMouseEnter(BaseScreenComponent sender)
        {
            // Get item
            EquipSlots slot = (EquipSlots)sender.Tag;
            DaggerfallUnityItem item = playerEntity.ItemEquipTable.GetItem(slot);
            if (item == null)
                return;
            UpdateItemInfoPanel(item);
        }

        protected override void ItemListScroller_OnHover(DaggerfallUnityItem item)
        {
            UpdateItemInfoPanel(item);
        }
        #endregion

        #region Item Action Helpers
        TextFile.Token[] ModifyItemInfoTokens(TextFile.Token[] tokens, DaggerfallUnityItem item)
        {
            ItemProperties props = RepairTools.Instance.GetItemProperties(item);
            if (props.MaxCharge <= 0)
                return tokens;

            int conditionIndex = tokens.Select((token, i) => new { Token=token, Index=i }).First(kvp => kvp.Token.text.StartsWith("Condition")).Index;

            List<TextFile.Token> newTokens = new List<TextFile.Token>(tokens);
            var chargeTokens = DaggerfallUnity.TextProvider.CreateTokens(TextFile.Formatting.JustifyCenter, $"Charge: {props.CurrentCharge} / {props.MaxCharge}");
            newTokens.InsertRange(conditionIndex + 2, chargeTokens);

            return newTokens.ToArray();
        }

        new void ShowInfoPopup(DaggerfallUnityItem item)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            TextFile.Token[] tokens = ItemHelper.GetItemInfo(item, DaggerfallUnity.TextProvider);
            if (tokens != null && tokens.Length > 0)
            {
                tokens = ModifyItemInfoTokens(tokens, item);

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetTextTokens(tokens, item);

                if (item.IsPotionRecipe)
                {   // Setup the next message box with the potion recipe ingredients list.
                    DaggerfallMessageBox messageBoxRecipe = new DaggerfallMessageBox(uiManager, messageBox);
                    messageBoxRecipe.SetTextTokens(item.GetMacroDataSource().PotionRecipeIngredients(TextFile.Formatting.JustifyCenter));
                    messageBoxRecipe.ClickAnywhereToClose = true;
                    messageBox.AddNextMessageBox(messageBoxRecipe);
                    messageBox.Show();
                }
                else if (item.legacyMagic != null)
                {   // Setup the next message box with the magic effect info.
                    DaggerfallMessageBox messageBoxMagic = new DaggerfallMessageBox(uiManager, messageBox);
                    messageBoxMagic.SetTextTokens(1016, item);
                    messageBoxMagic.ClickAnywhereToClose = true;
                    messageBox.AddNextMessageBox(messageBoxMagic);
                    messageBox.Show();
                }
                else if (item.ItemGroup == ItemGroups.Paintings)
                {   // Setup the message box with the painting image generated by macro handlers
                    ImageData paintingImg = ImageReader.GetImageData(item.GetPaintingFilename(), item.GetPaintingFileIdx(), 0, true, true);
                    messageBox.ImagePanel.VerticalAlignment = VerticalAlignment.None;
                    messageBox.ImagePanel.Position = new Vector2(0, 5);
                    messageBox.ImagePanel.Size = new Vector2(paintingImg.width, paintingImg.height);
                    messageBox.ImagePanel.BackgroundTexture = paintingImg.texture;
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                }
                else
                {
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                }
            }
        }
        #endregion
    }
}