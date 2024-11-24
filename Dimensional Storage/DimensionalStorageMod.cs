using BepInEx;
using Com.JiceeDev.DimensionalStorage.Patches;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;


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
            Harmony.CreateAndPatchAll(typeof(MechaForgePatch));
            Harmony.CreateAndPatchAll(typeof(UIReplicatorWindowPatch));
            Harmony.CreateAndPatchAll(typeof(StorageComponentReplicatorCheckItemsPatch));
        }


    }



// Harmony Patch for UIStorageWindow:


// MechaForge Patch for TryAddTask

    //UIReplicatorWindowPatch OnOkButtonClickPrefix

    // public int TakeItem(int itemId, int count, out int inc) for StorageComponent
    // Only if type is MechaForgeStorageTryAddTaskStorageComponent



}