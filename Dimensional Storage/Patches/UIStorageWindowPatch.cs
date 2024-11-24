using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    public class UIStorageWindowPatch
    {
        public static UIButton DimensionalStorageButton = null;

        public static UIStorageWindow StorageWindow;

        [HarmonyPatch(typeof(UIStorageWindow), "_OnCreate")]
        [HarmonyPostfix]
        public static void _OnCreatePostfix(ref UIStorageWindow __instance)
        {

            StorageWindow = __instance;

            // Add a new button to the UIStorageWindow
            if (DimensionalStorageButton != null)
            {
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
            var storageComponent = GetStorageComponent();

            if (storageComponent == null)
            {
                return;
            }

            if (DimensionalStorageMod.DimensionalStorageSystem.StorageContainers.Contains(storageComponent))
            {
                // Remove the storage container from the Dimensional Storage System
                DimensionalStorageMod.DimensionalStorageSystem.RemoveStorageContainer(storageComponent);
            }
            else
            {
                DimensionalStorageMod.DimensionalStorageSystem.AddStorageContainer(storageComponent);
            }
            
            bool isLinked = DimensionalStorageMod.DimensionalStorageSystem.StorageContainers.Contains(storageComponent);

            UpdateUI(isLinked);

        }

        // On open
        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyPostfix]
        public static void OnStorageIdChangePostfix(ref UIStorageWindow __instance)
        {
            var storageComponent = GetStorageComponent();
            if (storageComponent == null)
            {
                UpdateUI(false);
                return;
            }

            bool isLinked = DimensionalStorageMod.DimensionalStorageSystem.StorageContainers.Contains(storageComponent);
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
}