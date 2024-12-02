using System;
using System.Linq;
using Com.JiceeDev.DimensionalStorage.Models;
using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    public class StorageComponentPatchAll
    {
        // Prefix patch for method with one int parameter
        [HarmonyPatch(typeof(StorageComponent), "GetItemCount", new Type[]
        {
            typeof(int)
        })]
        [HarmonyPostfix]
        public static void GetItemCountPostfix(ref int __result, StorageComponent __instance, int itemId)
        {
            bool shouldBeHandled = __instance.isPlayerInventory || __instance is MechaForgeStorageTryAddTaskStorageComponent;

            if (!shouldBeHandled)
            {
                return;
            }

            var dimensionBonus = DimensionalStorageMod.TechManager.GetCachedDimensionalBonus();

            if (__instance.isPlayerInventory && !dimensionBonus.CanBuildWithDimensionalStorage)
            {
                return;
            }

            if (__instance is MechaForgeStorageTryAddTaskStorageComponent && !dimensionBonus.CanReplicateWithDimensionalStorage)
            {
                return;
            }

            try
            {
                int dimensionalStorageItemCount = DimensionalStorageMod.DimensionalStorageSystem.GetItemCount(itemId);
                __result += dimensionalStorageItemCount;
            }
            catch (Exception ex)
            {
                Debug.Log("DS - GetItemCountPrefix error: " + ex.ToString());
            }

        }



    }
}