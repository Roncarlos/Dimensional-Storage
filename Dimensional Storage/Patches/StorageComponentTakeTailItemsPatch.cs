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

        private static int startItemId = 0;
        private static int startCount = 0;
        private static int startInc = 0;
        
        // Prefix to cache item id
        [HarmonyPrefix]
        public static void TakeTailItemsPrefix(StorageComponent __instance, ref int itemId, ref int count, ref int inc, bool useBan)
        {
            // Show type
            // Debug.Log("DS - TakeTailItemsPrefix: " + __instance.GetType());
            // Check if it's in current inventory
            if (!(__instance is TempBuildStorageComponent))
            {
                return;
            }

            // Debug.Log("DS - TakeTailItemsPrefix !");

            int numberOfItemInTempPackage = __instance.GetItemCount(itemId);
            
            if(count > numberOfItemInTempPackage)
            {
                DimensionalStorageSystem.TransferToPlayer(BuildToolPatch.player, itemId, count - numberOfItemInTempPackage, inc);
            }
            
        }
        
        
        [HarmonyPostfix]
        public static void TakeTailItemsPostfix(StorageComponent __instance, ref int itemId, ref int count, ref int inc, bool useBan)
        {
            return;
            // If not player inventory && not TempBuildStorageComponent
            if (!__instance.isPlayerInventory && !(__instance is TempBuildStorageComponent))
            {
                return;
            }
            
            // Debug.Log("DS - TakeTailItemsPostfix At Start: itemId=" + startItemId + " count=" + startCount + " inc=" + startInc);
            // Debug.Log("DS - TakeTailItemsPostfix: itemId=" + itemId + " count=" + count + " inc=" + inc + " useBan=" + useBan);
        
            // Show values
            Debug.Log("DS - TakeTailItemsPostfix: itemId=" + itemId + " count=" + count + " inc=" + inc + " useBan=" + useBan);
            Debug.Log("DS - TakeTailItemsPostfix At Start: itemId=" + startItemId + " count=" + startCount + " inc=" + startInc);

            try
            { 
                // If count == 0 it means that the item was not found
                if (count > 0)
                {
                    // No need to do anything
                    return;
                }

                int remaining = startCount;
            
                foreach (var storage in DimensionalStorageSystem.StorageContainers)
                {
                    int storageCount = storage.GetItemCount(startItemId);
                    int newItemId = startItemId;
                    if (storageCount <= 0)
                    {
                        continue;
                    }
                    //
                    if (storageCount >= remaining)
                    {
                        storage.TakeTailItems(ref newItemId, ref remaining, out int newInc, useBan);
                        break;
                    }
                    else
                    {
                        storage.TakeTailItems(ref newItemId, ref storageCount, out int newInc, useBan);
                        remaining -= storageCount;
                    }
            
                }
            
                if(remaining == 0)
                {
                    itemId = startItemId;
                    count = startCount;
                    inc = startInc;
                }
                else
                {
                    itemId = startItemId;
                    count = 0;
                    inc = startCount - remaining;
                }
            
            } catch (Exception ex)
            {
                Debug.Log("DS - TakeTailItemsPostfix error: " + ex.ToString());
            }
        
        }
    }
}