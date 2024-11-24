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
            if (!__instance.isPlayerInventory && !(__instance is MechaForgeStorageTryAddTaskStorageComponent))
            {
                return;
            }

            try
            {
                __result += DimensionalStorageSystem.GetItemCount(itemId);
            } catch (Exception ex)
            {
                Debug.Log("DS - GetItemCountPrefix error: " + ex.ToString());
            }

        }
        
      
        
    }
}