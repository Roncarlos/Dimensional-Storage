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
        public static bool TryAddTaskPrefix(MechaForge __instance)
        {
            InitStorage(__instance);
            return true;
        }
        
        [HarmonyPatch(typeof(MechaForge), "TryTaskWithTestPackage")]
        [HarmonyPrefix]
        public static bool TryTaskWithTestPackagePrefix(MechaForge __instance)
        {
            InitStorage(__instance);
            return true;
        }

        private static void InitStorage(MechaForge __instance)
        {
            var currentStorage = (StorageComponent)_test_storage.GetValue(__instance);
            if (currentStorage != null) return;
            // Create a MechaForgeStorageTryAddTaskStorageComponent with size 0
            currentStorage = new MechaForgeStorageTryAddTaskStorageComponent(0);
            // Set field value
            _test_storage.SetValue(__instance, currentStorage);
            Debug.Log("DS - TryAddTaskPrefix: currentStorage is null so created a new one");


        }

    }
}