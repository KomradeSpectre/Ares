using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Ares.Elite_Equipment
{
    public abstract class AffixBase<T> : AffixBase where T : AffixBase<T>
    {
        public static T instance { get; private set; }

        public AffixBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EquipmentBoilerplate/Equipment was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class AffixBase
    {
        // Equipment Generation Settings


        // Strings
        public string BaseTypeNamePrefix = "AFFIX_";
        public abstract string AffixEquipmentName { get; }
        public abstract string AffixBuffName { get; }
        public abstract string AffixLangTokenName { get; }
        public abstract string AffixPickupDesc { get; }
        public abstract string AffixFullDescription { get; }
        public abstract string AffixLore { get; }

        public abstract string AffixModelPath { get; }
        public abstract string AffixIconPath { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = true;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = true;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public EquipmentIndex AffixIndex;

        public abstract void Init(ConfigFile config);

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateLang()
        {
            LanguageAPI.Add(BaseTypeNamePrefix + AffixLangTokenName + "_NAME", AffixName);
            LanguageAPI.Add(BaseTypeNamePrefix + AffixLangTokenName + "_PICKUP", AffixPickupDesc);
            LanguageAPI.Add(BaseTypeNamePrefix + AffixLangTokenName + "_DESCRIPTION", AffixFullDescription);
            LanguageAPI.Add(BaseTypeNamePrefix + AffixLangTokenName + "_LORE", AffixLore);
        }

        protected void CreateAffix()
        {
            EquipmentDef affixDef = new RoR2.EquipmentDef()
            {
                name = BaseTypeNamePrefix + AffixLangTokenName,
                nameToken = BaseTypeNamePrefix + AffixLangTokenName + "_NAME",
                pickupToken = BaseTypeNamePrefix + AffixLangTokenName + "_PICKUP",
                descriptionToken = BaseTypeNamePrefix + AffixLangTokenName + "_DESCRIPTION",
                loreToken = BaseTypeNamePrefix + AffixLangTokenName + "_LORE",
                pickupModelPath = AffixModelPath,
                pickupIconPath = AffixIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };
            var itemDisplayRules = CreateItemDisplayRules();
            AffixIndex = ItemAPI.Add(new CustomEquipment(affixDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if (equipmentIndex == AffixIndex)
            {
                return ActivateAffix(self);
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }

        protected abstract bool ActivateAffix(EquipmentSlot slot);

        public abstract void Hooks();
    }
}
