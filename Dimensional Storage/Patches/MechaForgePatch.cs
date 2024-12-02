using System.Reflection;
using Com.JiceeDev.DimensionalStorage.Models;
using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    public static class MechaForgePatch
    {
        private static FieldInfo _test_storage = typeof(MechaForge).GetField("_test_storage", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(MechaForge), "TryAddTask")]
        [HarmonyPrefix]
        public static void TryAddTaskPrefix(MechaForge __instance)
        {
            InitStorage(__instance);
        }
        
        [HarmonyPatch(typeof(MechaForge), "TryTaskWithTestPackage")]
        [HarmonyPrefix]
        public static void TryTaskWithTestPackagePrefix(MechaForge __instance)
        {
            InitStorage(__instance);
        }

        private static void InitStorage(MechaForge __instance)
        {
            var currentStorage = (StorageComponent)_test_storage.GetValue(__instance);
            if (currentStorage is MechaForgeStorageTryAddTaskStorageComponent)
            {
                return;
            }
            Debug.Log("DS - InitStorage for MechaForge !");
            // Create a MechaForgeStorageTryAddTaskStorageComponent with size 0
            currentStorage = new MechaForgeStorageTryAddTaskStorageComponent(0);
            // Set field value
            _test_storage.SetValue(__instance, currentStorage);
        }
        
        // PredictTaskCount(int recipeId, int maxShowing = 99, bool predictBottleneckItems = false)
        [HarmonyPatch(typeof(MechaForge), "PredictTaskCount")]
        [HarmonyPrefix]
        public static void PredictTaskCountPostfix(MechaForge __instance, ref int __result, int recipeId, int maxShowing, bool predictBottleneckItems)
        {
            // Debug.Log("DS - PredictTaskCountPostfix: Result: " + __result + " RecipeId: " + recipeId + " MaxShowing: " + maxShowing + " PredictBottleneckItems: " + predictBottleneckItems);
        }

    }
}