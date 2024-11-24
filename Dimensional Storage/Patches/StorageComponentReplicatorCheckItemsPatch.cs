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
            
            int numberOfItemsInStorage = __instance.GetItemCount(itemId);
            // Add what we can from the Dimensional Storage
            numberOfItemsInStorage += DimensionalStorageSystem.GetItemCount(itemId);
            
            Debug.Log("DS - TakeItemPostfix: itemId=" + itemId + " count=" + count + " numberOfItemsInStorage=" + numberOfItemsInStorage);
            Debug.Log("DS - TakeItemPostfix: numberOfItemsInStorage=" + numberOfItemsInStorage);
            
            
            __result = numberOfItemsInStorage - count;
        }
    }
}