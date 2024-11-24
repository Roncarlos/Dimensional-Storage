using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Com.JiceeDev.DimensionalStorage.Patches;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;


namespace Com.JiceeDev.DimensionalStorage
{


    [BepInPlugin("com.jiceedev.DimensionalStorage", "Dimensional Storage", "1.0.0")]
    public class DimensionalStorageMod : BaseUnityPlugin
    {
        void Awake()
        {
            // Harmony Patch for UIStorageWindow
            Harmony.CreateAndPatchAll(typeof(UIStorageWindowPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentGetItemCountPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentPatchAll));
            Harmony.CreateAndPatchAll(typeof(BuildToolPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentTakeTailItemsPatch));
        }


    }



// Harmony Patch for UIStorageWindow:


    public class UIStorageWindowPatch
    {
        public static UIButton DimensionalStorageButton = null;

        public static UIStorageWindow StorageWindow;

        [HarmonyPatch(typeof(UIStorageWindow), "_OnCreate")]
        [HarmonyPostfix]
        public static void _OnCreatePostfix(ref UIStorageWindow __instance)
        {

            Debug.Log("DS - UIStorageWindow _OnCreatePostfix");
            StorageWindow = __instance;

            // Add a new button to the UIStorageWindow
            if (DimensionalStorageButton != null)
            {
                Debug.Log("DS - Button already exists");
                return;
            }

            // Create a new button
            UIButton button = UnityEngine.Object.Instantiate<UIButton>(__instance.filterButton0, __instance.filterButton0.transform.parent);

            // Set the button's position
            button.transform.localPosition = new Vector3(button.transform.localPosition.x + 120, button.transform.localPosition.y, 0f);
            button.tips.tipTitle = "Dimensional Storage (Inactive)";
            button.tips.tipText = "When active, the container will be linked to the Dimensional Storage";

            button.onClick += ButtonOnClick;


            // __instance.filterButton0.tips.tipTitle = "Dimensional Storage";
            // __instance.filterButton0.tips.tipText = "Open the Dimensional Storage";
            // Set the button's text
            // button.input.text = "Dimensional Storage";

            DimensionalStorageButton = button;


        }

        private static void ButtonOnClick(int obj)
        {
            Debug.Log("DS - ButtonOnClick");
            var storageComponent = GetStorageComponent();

            if (storageComponent == null)
            {
                Debug.Log("DS - StorageComponent is null");
                return;
            }

            bool isLinked = DimensionalStorageSystem.StorageContainers.Contains(storageComponent);

            if (DimensionalStorageSystem.StorageContainers.Contains(storageComponent))
            {
                // Remove the storage container from the Dimensional Storage System
                DimensionalStorageSystem.RemoveStorageContainer(storageComponent);
            }
            else
            {
                DimensionalStorageSystem.AddStorageContainer(storageComponent);
            }

            UpdateUI(isLinked);

        }

        // On open
        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyPostfix]
        public static void OnStorageIdChangePostfix(ref UIStorageWindow __instance)
        {
            Debug.Log("DS - OnStorageIdChangePostfix");
            var storageComponent = GetStorageComponent();

            if (storageComponent == null)
            {
                Debug.Log("DS - StorageComponent is null");
                return;
            }

            bool isLinked = DimensionalStorageSystem.StorageContainers.Contains(storageComponent);
            UpdateUI(isLinked);
        }



        private static StorageComponent GetStorageComponent()
        {
            try
            {
                return StorageWindow.factoryStorage.storagePool[StorageWindow.storageId];
            } catch
            {
                return null;
            }
        }

        private static void UpdateUI(bool isLinked)
        {
            DimensionalStorageButton.button.image.color = isLinked ? Color.green : Color.red;
            DimensionalStorageButton.tips.tipTitle = isLinked ? "Dimensional Storage (Active)" : "Dimensional Storage (Inactive)";
        }
    }

    public static class DimensionalStorageSystem
    {
        public static HashSet<StorageComponent> StorageContainers { get; private set; } = new HashSet<StorageComponent>();

        public static void AddStorageContainer(StorageComponent storage)
        {
            StorageContainers.Add(storage);
        }

        public static void RemoveStorageContainer(StorageComponent storage)
        {
            StorageContainers.Remove(storage);
        }
        
        public static int GetItemCount(int itemId)
        {
            return StorageContainers.Select(c => c.GetItemCount(itemId)).Sum();
        }
        
        public static void TransferTo(StorageComponent other, int itemId, int count)
        {
            int maximumItemsAvailable = StorageContainers.Select(c => c.GetItemCount(itemId)).Sum();
            
            count = Math.Min(count, maximumItemsAvailable);
            
            foreach (var storage in StorageContainers)
            {
                int storageCount = storage.GetItemCount(itemId);
                if (storageCount <= 0)
                {
                    continue;
                }
                //
                if (storageCount >= count)
                {
                    storage.TakeItem(itemId, count, out int _);
                    other.AddItem(itemId, count, 0, 1, 0, out _);
                    break;
                }
                else
                {
                    storage.TakeItem(itemId, storageCount, out int newInc);
                    other.AddItem(itemId, storageCount, 0, 1, 0, out _);
                    count -= storageCount;
                }
            }
        }
        
        public static void TransferToPlayer(Player player, int itemId, int count, int itemInc)
        {
            int maximumItemsAvailable = StorageContainers.Select(c => c.GetItemCount(itemId)).Sum();
            
            count = Math.Min(count, maximumItemsAvailable);
            itemInc = Math.Min(itemInc, count);

            player.TryAddItemToPackage(itemId, count, itemInc, true);
            
            foreach (var storage in StorageContainers)
            {
                int storageCount = storage.GetItemCount(itemId);
                if (storageCount <= 0)
                {
                    continue;
                }
                //
                if (storageCount >= count)
                {
                    storage.TakeItem(itemId, count, out int _);
                    break;
                }
                else
                {
                    storage.TakeItem(itemId, storageCount, out int newInc);
                    count -= storageCount;
                }
            }
        }


    }


}