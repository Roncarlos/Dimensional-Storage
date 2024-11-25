using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
using CommonAPI.Systems.ModLocalization;
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
            GlobalConfiguration.Init(new ConfigFile(Path.Combine(Paths.ConfigPath, "global_configuration.cfg"), true));
            TechManager.RegisterTechs();
        }

        public void IntoOtherSave()
        {
            return;
        }


    }

    public interface IConfigManager
    {
        void LoadConfig();
    }

    public interface ITechManager : IDisposable
    {
        void RegisterTechs();
        DimensionalBonus GetCurrentDimensionalBonus(int techID);
        DimensionalBonus GetAllDimensionalBonus();
        DimensionalBonus GetCachedDimensionalBonus();

    }

    public class TechManager : ITechManager, IDisposable
    {
        private static bool _isLoaded = false;
        private readonly static Dictionary<int, TechItem> _techItems = new Dictionary<int, TechItem>();

        private DimensionalBonus _currentBonus = null;

        public void RegisterTechs()
        {
            if (!_isLoaded)
            {
                LoadConfig();
                LoadTechs();
            }
            else
            {
                GameMain.history.onTechUnlocked += HistoryOnTechUnlocked;
            }
        }

        private void HistoryOnTechUnlocked(int arg1, int arg2, bool arg3)
        {
            _currentBonus = GetAllDimensionalBonus();
        }

        public DimensionalBonus GetCurrentDimensionalBonus(int techID)
        {
            if (!GameMain.history.techStates.TryGetValue(techID, out var techState))
            {
                return new DimensionalBonus();
            }

            if (!_techItems.TryGetValue(techID, out var techItem))
            {
                return new DimensionalBonus();
            }
            // return new DimensionalBonus();
            return techItem.Bonuses;
        }

        public DimensionalBonus GetAllDimensionalBonus()
        {
            var bonus = new DimensionalBonus();

            bonus.NumberOfDimensionalStorage += GlobalConfiguration.Tech.BaseNumberOfDimensionalStorage;
            bonus.CanBuildWithDimensionalStorage |= GlobalConfiguration.Tech.BaseCanBuildWithDimensionalStorage;
            bonus.CanReplicateWithDimensionalStorage |= GlobalConfiguration.Tech.BaseCanReplicateWithDimensionalStorage;

            foreach (var techItem in _techItems.Values)
            {
                if (!GameMain.history.techStates.TryGetValue(techItem.ID, out var techState) || !techState.unlocked) continue;

                bonus.NumberOfDimensionalStorage += techItem.Bonuses.NumberOfDimensionalStorage;
                bonus.CanBuildWithDimensionalStorage |= techItem.Bonuses.CanBuildWithDimensionalStorage;
                bonus.CanReplicateWithDimensionalStorage |= techItem.Bonuses.CanReplicateWithDimensionalStorage;
            }
            return bonus;
        }

        public DimensionalBonus GetCachedDimensionalBonus()
        {
            return _currentBonus ?? (_currentBonus = GetAllDimensionalBonus());
        }

        private static void LoadConfig()
        {

            string json = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "config", "tech",
                GlobalConfiguration.Tech.TechsPath));

            var techTree = StringSerializationAPI.Deserialize(typeof(TechTree), json) as TechTree;

            Debug.Log($"Loading tech tree from {GlobalConfiguration.Tech.TechsPath}, found {techTree.TechItems.Count} techs");
            foreach (var techItem in techTree.TechItems)
            {
                Debug.Log($"Registering tech {techItem.ID}: {techItem.IDLong} at position {techItem.Position}");
                Debug.Log("desc:" + techItem.Description);
                Debug.Log("conclusion:" + techItem.Conclusion);
                _techItems.Add(techItem.ID, techItem);
            }
            _isLoaded = true;

        }
        //Total amount of each jello is calculated like this: N = H*C/3600, where H - total hash count, C - items per minute of jello.
        private static void CalculateJellosRates()
        {
            foreach (var techItem in _techItems.Values)
            {
                for (int i = 0; i < techItem.Jellos.Length; i++)
                {
                    techItem.JellosRates[i] = techItem.HashNeeded * techItem.JellosRates[i] / 3600;
                }
            }
        }
        
        // Simple javascript function to calculate the amount of each jello needed for a tech
        // function calculateJellosRates(jellorate, hashneeded) {
        //     return hashneeded * jellorate / 3600;
        // }
        
        // Now from a target jello we want to calculate the amount of hash needed for a tech
        // function calculateHashNeeded(jellorate, jelloamount) {
        //     return jelloamount * 3600 / jellorate;
        // }
        
        // Now we want to calculate the amount of hash needed for a tech from a target jello and the amount of that jello we have
        // function calculateHashNeededFromJello(jellorate, jelloamount, jelloamountneeded) {
        //     return jelloamountneeded * 3600 / jellorate;
        // }
        

        private static void LoadTechs()
        {
            foreach (var techItem in _techItems.Values)
            {
                LocalizationModule.RegisterTranslation($"{techItem.IDLong}_name", techItem.Name);
                LocalizationModule.RegisterTranslation($"{techItem.IDLong}_desc", techItem.Description);
                LocalizationModule.RegisterTranslation($"{techItem.IDLong}_conc", techItem.Conclusion);

                ProtoRegistry.RegisterTech(
                    techItem.ID, $"{techItem.IDLong}_name", $"{techItem.IDLong}_desc", $"{techItem.IDLong}_conc",
                    techItem.IconPath, techItem.RequiredTechs, techItem.Jellos, techItem.JellosRates, techItem.HashNeeded, techItem.UnlockRecipes, techItem.Position);

            }
        }

        public void Dispose()
        {
            GameMain.history.onTechUnlocked -= HistoryOnTechUnlocked;
        }
    }

    [Serializable]
    public class TechItem
    {
        public int ID = 0;
        public string IDLong = "";
        public string Name = "";
        public string Description = "";
        public string Conclusion = "";
        public string IconPath = "";
        public int[] RequiredTechs = Array.Empty<int>();
        public int[] Jellos = Array.Empty<int>();
        public int[] JellosRates = Array.Empty<int>();
        public int HashNeeded = 0;
        public int[] UnlockRecipes = Array.Empty<int>();
        public Vector2 Position = new Vector2(0, 0);
        public DimensionalBonus Bonuses = new DimensionalBonus();
    }

    [Serializable]
    public class TechTree
    {
        public List<TechItem> TechItems;
    }

    [Serializable]
    public class DimensionalBonus
    {
        public int NumberOfDimensionalStorage = 0;
        public bool CanBuildWithDimensionalStorage = false;
        public bool CanReplicateWithDimensionalStorage = false;
    }



    // Harmony Patch for UIStorageWindow:


// MechaForge Patch for TryAddTask

    //UIReplicatorWindowPatch OnOkButtonClickPrefix

    // public int TakeItem(int itemId, int count, out int inc) for StorageComponent
    // Only if type is MechaForgeStorageTryAddTaskStorageComponent



}

// Example of json file in config/tech/default.json
// {
//     "TechItems": [
//         {
//             "ID": 1901,
//             "IDLong": "dimensional_storage_basic",
//             "Name": "Dimensional Storage - Basic",
//             "Description": "Unlock the basic Dimensional Storage",
//             "Conclusion": "You have
//             unlocked the basic Dimensional Storage",
//             "IconPath": "assets/dimensional_storage.png",
//             "RequiredTechs": [],
//             "Jellos": [6001,6002,6003,6004,6005,6006],
//             "JellosRates": [1,1,1,1,1,1],
//             "HashNeeded": 10000,
//             "UnlockRecipes": [],
//             "Position": [0,0],
//             "Bonuses": {
//                 "NumberOfDimensionalStorage": 1,
//                 "CanBuildWithDimensionalStorage": false,
//                 "CanReplicateWithDimensionalStorage": false
//             }
//         },
//         {
//             "ID": 1902,
//             "IDLong": "dimensional_storage_basic",
//             "Name": "Dimensional Storage - Basic",
//             "Description": "Unlock the basic Dimensional Storage",
//             "Conclusion": "You have
//             unlocked the basic Dimensional Storage",
//             "IconPath": "assets/dimensional_storage.png",
//             "RequiredTechs": [],
//             "Jellos": [6001,6002,6003,6004,6005,6006],
//             "JellosRates": [1,1,1,1,1,1],
//             "HashNeeded": 10000,
//             "UnlockRecipes": [],
//             "Position": [2,2],
//             "Bonuses": {
//                 "NumberOfDimensionalStorage": 1,
//                 "CanBuildWithDimensionalStorage": false,
//                 "CanReplicateWithDimensionalStorage": false
//             }
//         }
//     ]
// }