using DaggerfallConnect.Save;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace RepairTools
{
    class RepairToolsCastWhenStrikes : CastWhenStrikes
    {
        // Items lose 10 durability points for every spell cast on strike
        // http://en.uesp.net/wiki/Daggerfall:Magical_Items#Durability_of_Magical_Items
        const int durabilityLossOnStrike = 10;

        public override void SetProperties()
        {
            base.SetProperties();
            properties.DisableReflectiveEnumeration = true;
        }

        public override PayloadCallbackResults? EnchantmentPayloadCallback(EnchantmentPayloadFlags context, EnchantmentParam? param = null, DaggerfallEntityBehaviour sourceEntity = null, DaggerfallEntityBehaviour targetEntity = null, DaggerfallUnityItem sourceItem = null, int sourceDamage = 0)
        {
            // Validate
            if (context != EnchantmentPayloadFlags.Strikes || targetEntity == null || param == null || sourceDamage == 0)
                return null;

            // Get target effect manager
            EntityEffectManager effectManager = targetEntity.GetComponent<EntityEffectManager>();
            if (!effectManager)
                return null;

            // Handle the item charge
            ItemProperties props = RepairTools.Instance.GetItemProperties(sourceItem);
            if(props.CurrentCharge < durabilityLossOnStrike)
                return null;

            props.CurrentCharge -= durabilityLossOnStrike;

            // Cast when strikes enchantment prepares a new ready spell
            if (!string.IsNullOrEmpty(param.Value.CustomParam))
            {
                // TODO: Ready a custom spell bundle
                return null;
            }

            // Ready a classic spell bundle
            SpellRecord.SpellRecordData spell;
            if (GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(param.Value.ClassicParam, out spell))
            {
                EffectBundleSettings bundleSettings;
                if (GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spell, BundleTypes.Spell, out bundleSettings))
                {
                    EntityEffectBundle bundle = new EntityEffectBundle(bundleSettings, sourceEntity);
                    effectManager.AssignBundle(bundle, AssignBundleFlags.ShowNonPlayerFailures);
                }
            }
            
            return null;
        }

    }
}
