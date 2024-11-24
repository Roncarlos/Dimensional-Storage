using System;
using System.Linq;
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
            if (!__instance.isPlayerInventory)
            {
                return;
            }

            try
            {
                __result += DimensionalStorageSystem.StorageContainers.Select(c => c.GetItemCount(itemId)).Sum();
            } catch (Exception ex)
            {
                Debug.Log("DS - GetItemCountPrefix error: " + ex.ToString());
            }

        }
        
    }
}