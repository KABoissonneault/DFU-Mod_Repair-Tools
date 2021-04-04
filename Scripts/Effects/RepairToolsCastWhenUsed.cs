using DaggerfallConnect.Save;

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace RepairTools
{
    class RepairToolsCastWhenUsed : CastWhenUsed
    {
        const int chargeLossOnUse = 10;

        public override void SetProperties()
        {
            base.SetProperties();
            properties.DisableReflectiveEnumeration = true;
        }

        public override PayloadCallbackResults? EnchantmentPayloadCallback(EnchantmentPayloadFlags context, EnchantmentParam? param = null, DaggerfallEntityBehaviour sourceEntity = null, DaggerfallEntityBehaviour targetEntity = null, DaggerfallUnityItem sourceItem = null, int sourceDamage = 0)
        {
            // Validate
            if (context != EnchantmentPayloadFlags.Used || sourceEntity == null || param == null)
                return null;

            // Get caster effect manager
            EntityEffectManager effectManager = sourceEntity.GetComponent<EntityEffectManager>();
            if (!effectManager)
                return null;

            ItemProperties props = RepairTools.Instance.GetItemProperties(sourceItem);

            // Do not activate enchantment if it doesn't have enough  charge
            if (sourceItem != null && props.CurrentCharge < chargeLossOnUse)
            {
                if (sourceItem.TemplateIndex == (int)Armor.Boots || sourceItem.TemplateIndex == (int)Armor.Gauntlets || sourceItem.TemplateIndex == (int)Armor.Greaves)
                    DaggerfallUI.Instance.PopupMessage($"{sourceItem.LongName} have run out.");
                else
                    DaggerfallUI.Instance.PopupMessage($"{sourceItem.LongName} has run out.");
                
                return null;
            }

            props.CurrentCharge -= chargeLossOnUse;                

            // Cast when used enchantment prepares a new ready spell
            if (!string.IsNullOrEmpty(param.Value.CustomParam))
            {
                // TODO: Ready a custom spell bundle
            }
            else
            {
                // Ready a classic spell bundle
                SpellRecord.SpellRecordData spell;
                EffectBundleSettings bundleSettings;
                EntityEffectBundle bundle;
                if (GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(param.Value.ClassicParam, out spell))
                {
                    if (GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spell, BundleTypes.Spell, out bundleSettings))
                    {
                        // Self-cast spells are all assigned directly to self, "click to cast" spells are loaded to ready spell
                        // TODO: Support multiple ready spells so all loaded spells are launched on click
                        bundle = new EntityEffectBundle(bundleSettings, sourceEntity);
                        bundle.CastByItem = sourceItem;
                        if (bundle.Settings.TargetType == TargetTypes.CasterOnly)
                            effectManager.AssignBundle(bundle, AssignBundleFlags.BypassSavingThrows | AssignBundleFlags.BypassChance);
                        else
                            effectManager.SetReadySpell(bundle, true);
                    }
                }
            }

            return null;
        }
    }
}
