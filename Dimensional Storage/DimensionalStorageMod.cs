using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using Com.JiceeDev.DimensionalStorage.Patches;
using CommonAPI;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Com.JiceeDev.DimensionalStorage.Tech;
using FullSerializer;


namespace Com.JiceeDev.DimensionalStorage
{
    [BepInPlugin("com.jiceedev.DimensionalStorage", "Dimensional Storage", "1.0.0")]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry))]
    public class DimensionalStorageMod : BaseUnityPlugin, IModCanSave
    {

        public static DimensionalStorageSystem DimensionalStorageSystem = new DimensionalStorageSystem();
        public static ITechManager TechManager = new TechManager();

        void Awake()
        {
            Init();
            ApplyPatches();
        }

        private void ApplyPatches()
        {
            Harmony.CreateAndPatchAll(typeof(UIStorageWindowPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentGetItemCountPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentPatchAll));
            Harmony.CreateAndPatchAll(typeof(BuildToolPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentTakeTailItemsPatch));
            Harmony.CreateAndPatchAll(typeof(MechaForgePatch));
            Harmony.CreateAndPatchAll(typeof(UIReplicatorWindowPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentReplicatorCheckItemsPatch));
            Harmony.CreateAndPatchAll(typeof(GameHistoryDataPatch));
        }

        public void Export(BinaryWriter writer)
        {
            // Export the Dimensional Storage System
            DimensionalStorageSystem.Export(writer);
        }

        public void Import(BinaryReader reader)
        {
            TechManager?.Dispose();
            Init();
            // Import the Dimensional Storage System
            DimensionalStorageSystem.Import(reader);
        }

        private void Init()
        {
            Debug.Log("Dimensional Storage Mod Init");
            DimensionalStorageSystem = new DimensionalStorageSystem();
            TechManager = new TechManager();
            // Load the configuration
            GlobalConfiguration.Init(new ConfigFile(Path.Combine(Paths.ConfigPath, "dimensional_storage.global_configuration.cfg"), true));
            TechManager.RegisterTechs();
        }

        public void IntoOtherSave()
        {
            return;
        }


    }

}
