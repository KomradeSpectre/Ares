using BepInEx.Configuration;
using R2API;
using RoR2;
using System;

namespace Ares.Elite_Equipment
{
    public abstract class AffixEquipmentBase<T> : AffixEquipmentBase where T : AffixEquipmentBase<T>
    {
        public static T instance { get; private set; }

        public AffixEquipmentBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting EquipmentBoilerplate/Equipment was instantiated twice ");
            instance = this as T;
        }
    }

    public abstract class AffixEquipmentBase
    {
        public abstract string AffixEquipmentName { get; }
        public abstract string AffixEquipmentLangTokenName { get; }
        public abstract string AffixEquipmentPickupDesc { get; }
        public abstract string AffixEquipmentFullDescription { get; }
        public abstract string AffixEquipmentLore { get; }

        public abstract string AffixEquipmentModelPath { get; }
        public abstract string AffixEquipmentIconPath { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = false;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = false;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;


        public EquipmentDef AffixEquipmentDef;
        public EquipmentIndex AffixEquipmentIndex;

        public abstract void Init(ConfigFile config);

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateLang()
        {
            LanguageAPI.Add("AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_NAME", AffixEquipmentName);
            LanguageAPI.Add("AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_PICKUP", AffixEquipmentPickupDesc);
            LanguageAPI.Add("AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_DESCRIPTION", AffixEquipmentFullDescription);
            LanguageAPI.Add("AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_LORE", AffixEquipmentLore);
        }

        protected void CreateEquipment()
        {
            AffixEquipmentDef = new RoR2.EquipmentDef()
            {
                name = "AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName,
                nameToken = "AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_NAME",
                pickupToken = "AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_PICKUP",
                descriptionToken = "AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "AFFIX_EQUIPMENT_" + AffixEquipmentLangTokenName + "_LORE",
                pickupModelPath = AffixEquipmentModelPath,
                pickupIconPath = AffixEquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };
            var itemDisplayRules = CreateItemDisplayRules();
            AffixEquipmentIndex = ItemAPI.Add(new CustomEquipment(AffixEquipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if (equipmentIndex == AffixEquipmentIndex)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public abstract void Hooks();
    }
}