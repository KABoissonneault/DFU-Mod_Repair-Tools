using DaggerfallConnect.Save;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Formulas;

namespace RepairTools
{
    class RepairToolsCastWhenHeld : CastWhenHeld
    {
        const int normalMagicItemDegradeRate = 4;
        const int restingMagicItemDegradeRate = 60;

        public override void SetProperties()
        {
            base.SetProperties();
            properties.DisableReflectiveEnumeration = true;
        }

        public override PayloadCallbackResults? EnchantmentPayloadCallback(EnchantmentPayloadFlags context, EnchantmentParam? param = null, DaggerfallEntityBehaviour sourceEntity = null, DaggerfallEntityBehaviour targetEntity = null, DaggerfallUnityItem sourceItem = null, int sourceDamage = 0)
        {
            // Validate
            if ((context != EnchantmentPayloadFlags.Equipped &&
                 context != EnchantmentPayloadFlags.MagicRound &&
                 context != EnchantmentPayloadFlags.RerollEffect) ||
                param == null || sourceEntity == null || sourceItem == null)
                return null;

            // Get caster effect manager
            EntityEffectManager casterManager = sourceEntity.GetComponent<EntityEffectManager>();
            if (!casterManager)
                return null;

            if (context == EnchantmentPayloadFlags.Equipped)
            {
                // Cast when held enchantment invokes a spell bundle that is permanent until item is removed
                InstantiateSpellBundle(param.Value, sourceEntity, sourceItem, casterManager);
            }
            else if (context == EnchantmentPayloadFlags.MagicRound)
            {
                // Apply CastWhenHeld charge loss
                ApplyChargeLoss(sourceItem);
            }
            else if (context == EnchantmentPayloadFlags.RerollEffect)
            {
                // Recast spell bundle - previous instance has already been removed by EntityEffectManager prior to callback
                InstantiateSpellBundle(param.Value, sourceEntity, sourceItem, casterManager, true);
            }

            return null;
        }

        void ApplyChargeLoss(DaggerfallUnityItem item)
        {
            if (RepairTools.IsTraveling())
                return;

            int degradeRate = GameManager.Instance.PlayerEntity.IsResting ? restingMagicItemDegradeRate : normalMagicItemDegradeRate;
            if (GameManager.Instance.EntityEffectBroker.MagicRoundsSinceStartup % degradeRate != 0)
                return;

            ItemProperties props = RepairTools.Instance.GetItemProperties(item);
            if (props.CurrentCharge > 0)
            {
                props.CurrentCharge -= 1;
            }
        }

        void InstantiateSpellBundle(EnchantmentParam param, DaggerfallEntityBehaviour sourceEntity, DaggerfallUnityItem sourceItem, EntityEffectManager casterManager, bool recast = false)
        {
            if (!string.IsNullOrEmpty(param.CustomParam))
            {
                // TODO: Instantiate a custom spell bundle
                return;
            }
           
            // Instantiate a classic spell bundle
            SpellRecord.SpellRecordData spell;
            if (!GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(param.ClassicParam, out spell))
                return;

            ItemProperties props = RepairTools.Instance.GetItemProperties(sourceItem);
            int amount = !recast ? FormulaHelper.CalculateCastingCost(spell, false) : 0;
            if (recast)
            {
                if (props.CurrentCharge <= 0)
                {
                    if (sourceItem.TemplateIndex == (int)Armor.Boots || sourceItem.TemplateIndex == (int)Armor.Gauntlets || sourceItem.TemplateIndex == (int)Armor.Greaves)
                        DaggerfallUI.Instance.PopupMessage($"{sourceItem.LongName} have run out.");
                    else
                        DaggerfallUI.Instance.PopupMessage($"{sourceItem.LongName} has run out.");
                    return;
                }
            }
            else
            {
                if (props.CurrentCharge < amount)
                {
                    if (sourceItem.TemplateIndex == (int)Armor.Boots || sourceItem.TemplateIndex == (int)Armor.Gauntlets || sourceItem.TemplateIndex == (int)Armor.Greaves)
                        DaggerfallUI.Instance.PopupMessage($"{sourceItem.LongName} have run out.");
                    else
                        DaggerfallUI.Instance.PopupMessage($"{sourceItem.LongName} has run out.");
                    return;
                }
            }
            
            // Create effect bundle settings from classic spell
            EffectBundleSettings bundleSettings;
            if (GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spell, BundleTypes.HeldMagicItem, out bundleSettings))
            {
                // Assign bundle
                EntityEffectBundle bundle = new EntityEffectBundle(bundleSettings, sourceEntity);
                bundle.FromEquippedItem = sourceItem;
                bundle.AddRuntimeFlags(BundleRuntimeFlags.ItemRecastEnabled);
                casterManager.AssignBundle(bundle, AssignBundleFlags.BypassSavingThrows);

                // Play cast sound on equip for player only
                if (casterManager.IsPlayerEntity)
                    casterManager.PlayCastSound(sourceEntity, casterManager.GetCastSoundID(bundle.Settings.ElementType), true);

                // Classic uses an item last "cast when held" effect spell cost to determine its durability loss on equip
                // Here, all effects are considered, as it seems more coherent to do so
                if (!recast)
                {
                    props.CurrentCharge -= amount;
                }
            }

            // Store equip time as last reroll time
            sourceItem.timeEffectsLastRerolled = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
        }
    }    
}
