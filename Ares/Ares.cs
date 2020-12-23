using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Ares
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(ResourcesAPI),
                          nameof(PlayerAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI),
                          nameof(NetworkingAPI), nameof(EffectAPI), nameof(EliteAPI))]
    public class Ares : BaseUnityPlugin
    {
        public const string ModVer = "0.0.1";
        public const string ModName = "Ares";
        public const string ModGuid = "com.KomradeSpectre.Ares";

        private void Awake()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ares.ares_assets"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@Ares", bundle);
                ResourcesAPI.AddProvider(provider);
            }
        }
    }
}
