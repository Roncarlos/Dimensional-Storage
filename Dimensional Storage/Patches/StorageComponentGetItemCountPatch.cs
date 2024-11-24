using System;
using System.Reflection;
using Com.JiceeDev.DimensionalStorage.Models;
using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    [HarmonyPatch]
    public static class StorageComponentGetItemCountPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(StorageComponent).GetMethod("GetItemCount", new Type[]
            {
                typeof(int),
                typeof(int).MakeByRefType()
            });
        }

        [HarmonyPostfix]
        public static void GetItemCountPostfix(ref int __result, StorageComponent __instance, int itemId, ref int inc)
        {
            if (!__instance.isPlayerInventory && !(__instance is MechaForgeStorageTryAddTaskStorageComponent))
            {
                return;
            }

            try
            {
                foreach (var storage in DimensionalStorageSystem.StorageContainers)
                {
                    __result += storage.GetItemCount(itemId, out int count2);
                    inc += count2;
                }
            } catch (Exception ex)
            {
                Debug.Log("DS - GetItemCountPrefix error: " + ex.ToString());
            }

        }
    }
}