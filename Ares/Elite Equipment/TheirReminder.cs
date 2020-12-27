using Ares.Utils;
using BepInEx.Configuration;
using EliteSpawningOverhaul;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Ares.Elite_Equipment
{
    public class TheirReminder : AffixEquipmentBase<TheirReminder>
    {
        public ConfigEntry<float> DurationOfLightningStormBuff;
        public ConfigEntry<float> LightningStrikeCooldown;
        public ConfigEntry<int> AmountOfLightningStrikesPerBarrage;

        public override string AffixEquipmentName => "Their Reminder";

        public override string AffixEquipmentLangTokenName => "THEIR_REMINDER";

        public override string AffixEquipmentPickupDesc => "Become an aspect of the storm.";

        public override string AffixEquipmentFullDescription => "";

        public override string AffixEquipmentLore => "";

        public override string AffixEquipmentModelPath => "@Aetherium:Assets/Models/Prefabs/Elite Equipment/TheirReminder/TheirReminder.prefab";

        public override string AffixEquipmentIconPath => "";

        public static BuffIndex EliteBuffIndex;
        public static BuffDef EliteBuffDef;

        public static BuffIndex LightningStormBuffIndex;

        public static EliteIndex EliteIndex;

        public static EliteAffixCard EliteAffixCard;

        public static GameObject HyperchargedProjectile;
        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateProjectile();
            CreateEquipment();
            CreateElite();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            DurationOfLightningStormBuff = config.Bind<float>(AffixEquipmentName, "Duration of Lightning Storm Buff", 5f, "Duration of the Lightning Storm Buff upon activation of the affix item.");
            LightningStrikeCooldown = config.Bind<float>(AffixEquipmentName, "Duration Between Strikes on Lightning Storm Buff", 1f, "Duration between the strikes while Lightning Storm Buff is active.");
            AmountOfLightningStrikesPerBarrage = config.Bind<int>(AffixEquipmentName, "Amount of Lightning Strikes per Barrage", 16, "How many lightning strikes should be in each strike period of the Lightning Storm Buff?");
        }

        public void CreateBuff()
        {
            EliteBuffDef = new RoR2.BuffDef
            {
                name = "Affix_Hypercharged",
                buffColor = new Color32(255, 255, 255, byte.MaxValue),
                iconPath = "@Ares:Assets/Textures/Icons/Buff/TheirReminderBuffIcon.png",
                canStack = false,
            };

            var LightningStormBuffDef = new BuffDef
            {
                name = "Hypercharged Lightning Storm",
                buffColor = new Color32(255, 255, 255, byte.MaxValue),
                iconPath = "@Ares:Assets/Textures/Icons/Buff/LightningStormBuffIcon.png",
                canStack = false,
            };
            LightningStormBuffIndex = BuffAPI.Add(new CustomBuff(LightningStormBuffDef));

        }

        public void CreateProjectile()
        {
            HyperchargedProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/LightningStake"), "HyperchargedProjectile", true);

            var controller = HyperchargedProjectile.GetComponent<ProjectileController>();
            controller.startSound = "Play_titanboss_shift_shoot";

            var impactExplosion = HyperchargedProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.lifetime = 0.5f;
            impactExplosion.impactEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact");
            impactExplosion.blastRadius = 7f;
            impactExplosion.bonusBlastForce = new Vector3(0, 750, 0);

            // register it for networking
            if (HyperchargedProjectile) PrefabAPI.RegisterNetworkPrefab(HyperchargedProjectile);

            // add it to the projectile catalog or it won't work in multiplayer 
            RoR2.ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(HyperchargedProjectile);
            };
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[] { });
        }

        public void CreateElite()
        {
            var eliteDef = new RoR2.EliteDef
            {
                name = "Hypercharged",
                modifierToken = "ARES_HYPERCHARGED",
                color = Color.white,
                eliteEquipmentIndex = AffixEquipmentIndex
            };
            EliteIndex = EliteAPI.Add(new CustomElite(eliteDef, 2));
            LanguageAPI.Add(eliteDef.modifierToken, eliteDef.name + " {0}");

            EliteBuffDef.eliteIndex = EliteIndex;
            EliteBuffIndex = BuffAPI.Add(new CustomBuff(EliteBuffDef));
            AffixEquipmentDef.passiveBuff = EliteBuffIndex;

            EliteAffixCard = new EliteAffixCard
            {
                spawnWeight = 0.5f,
                costMultiplier = 30.0f,
                damageBoostCoeff = 2.0f,
                healthBoostCoeff = 4.5f,
                eliteOnlyScaling = 0.5f,
                eliteType = EliteIndex
            };
            EsoLib.Cards.Add(EliteAffixCard);
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += ManageLightningStrikes;
        }

        private void ManageLightningStrikes(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if (self.HasBuff(LightningStormBuffIndex))
            {
                var lightningTracker = self.GetComponent<LightningTracker>();
                if (!lightningTracker) { lightningTracker = self.gameObject.AddComponent<LightningTracker>(); }

                if(lightningTracker.LightningCooldown > 0)
                {
                    lightningTracker.LightningCooldown -= Time.fixedDeltaTime;
                }
                if(lightningTracker.LightningCooldown <= 0)
                {
                    for (int i = 1; i <= AmountOfLightningStrikesPerBarrage.Value; i++)
                    {
                        var newProjectileInfo = new FireProjectileInfo
                        {
                            owner = self.gameObject,
                            projectilePrefab = HyperchargedProjectile,
                            speedOverride = 150.0f,
                            damage = self.damage,
                            damageTypeOverride = null,
                            damageColorIndex = DamageColorIndex.Default,
                            procChainMask = default
                        };
                        var theta = (Math.PI * 2) / AmountOfLightningStrikesPerBarrage.Value;
                        var angle = theta * i;
                        var radius = 20 + random.RangeFloat(-15, 15);
                        var positionChosen = new Vector3((float)(radius * Math.Cos(angle) + self.corePosition.x), self.corePosition.y + 1, (float)(radius * Math.Sin(angle) + self.corePosition.z));
                        var raycastedChosen = MiscUtils.RaycastToFloor(positionChosen, 1000f);
                        if (raycastedChosen != null)
                        {
                            positionChosen = raycastedChosen.Value + new Vector3(0, 0.5f, 0);
                        }
                        newProjectileInfo.position = positionChosen;
                        newProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(positionChosen + Vector3.down);
                        ProjectileManager.instance.FireProjectile(newProjectileInfo);
                    }
                    lightningTracker.LightningCooldown = LightningStrikeCooldown.Value;
                }
            }
            orig(self);
        }

        protected override bool ActivateEquipment(RoR2.EquipmentSlot slot)
        {
            if (!slot.characterBody) { return false; }
            var body = slot.characterBody;

            if (NetworkServer.active)
            {
                body.AddTimedBuffAuthority(LightningStormBuffIndex, DurationOfLightningStormBuff.Value);
            }
            return true;
        }

        public class LightningTracker : MonoBehaviour
        {
            public float LightningCooldown;
        }
    }
}
