using System;
using System.Reflection;
using Com.JiceeDev.DimensionalStorage.Models;
using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    public static class StorageComponentReplicatorCheckItemsPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(StorageComponent).GetMethod("TakeItem", new Type[]
            {
                typeof(int),
                typeof(int),
                typeof(int).MakeByRefType()
            });
        }

        [HarmonyPostfix]
        public static void TakeItemPostfix(StorageComponent __instance, ref int __result, int itemId, int count, ref int inc)
        {
            if (!(__instance is MechaForgeStorageTryAddTaskStorageComponent))
            {
                return;
            }
            
            var dimensionBonus = DimensionalStorageMod.TechManager.GetCachedDimensionalBonus();
            
            if( !dimensionBonus.CanReplicateWithDimensionalStorage)
            {
                return;
            }
            
            int numberOfItemsInStorage = __instance.GetItemCount(itemId);
            // Add what we can from the Dimensional Storage
            numberOfItemsInStorage += DimensionalStorageMod.DimensionalStorageSystem.GetItemCount(itemId);
            
            
            __result = numberOfItemsInStorage - count;
        }
    }
}