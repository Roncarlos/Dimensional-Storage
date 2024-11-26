using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Tech
{
    public class TechManager : ITechManager
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
}