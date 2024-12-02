using System;
using System.Reflection;
using Com.JiceeDev.DimensionalStorage.Models;
using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    [HarmonyPatch]
    // public void TakeTailItems(ref int itemId, ref int count, out int inc, bool useBan = false)
    public static class StorageComponentTakeTailItemsPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(StorageComponent).GetMethod("TakeTailItems", new Type[]
            {
                typeof(int).MakeByRefType(),
                typeof(int).MakeByRefType(),
                typeof(int).MakeByRefType(),
                typeof(bool)
            });
        }
        
        // Prefix to cache item id
        [HarmonyPrefix]
        public static void TakeTailItemsPrefix(StorageComponent __instance, ref int itemId, ref int count, ref int inc, bool useBan)
        {
            // Show type
            // Debug.Log("DS - TakeTailItemsPrefix: " + __instance.GetType());
            // Check if it's in current inventory
            if (!(__instance is TempBuildStorageComponent) && !__instance.isPlayerInventory)
            {
                return;
            }
            
            var dimensionBonus = DimensionalStorageMod.TechManager.GetCachedDimensionalBonus();
            
            if(!dimensionBonus.CanBuildWithDimensionalStorage)
            {
                return;
            }


            // Debug.Log("DS - TakeTailItemsPrefix !");

            int numberOfItemInTempPackage = __instance.GetItemCount(itemId);
            
            if(count > numberOfItemInTempPackage)
            {
                DimensionalStorageMod.DimensionalStorageSystem.TransferToPlayer(itemId, count - numberOfItemInTempPackage, inc);
            }
            
        }

    }
    
}