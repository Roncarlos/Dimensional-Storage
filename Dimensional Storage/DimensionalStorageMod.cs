using System.IO;
using BepInEx;
using Com.JiceeDev.DimensionalStorage.Patches;
using CommonAPI;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;


namespace Com.JiceeDev.DimensionalStorage
{


    [BepInPlugin("com.jiceedev.DimensionalStorage", "Dimensional Storage", "1.0.0")]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    public class DimensionalStorageMod : BaseUnityPlugin, IModCanSave 
    {
        
        public static DimensionalStorageSystem DimensionalStorageSystem = new DimensionalStorageSystem();
        
        
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
        
        public void Export(BinaryWriter  writer)
        {
            // Export the Dimensional Storage System
            DimensionalStorageSystem.Export(writer);
        }
        
        public void Import(BinaryReader reader)
        {
            // Import the Dimensional Storage System
            DimensionalStorageSystem.Import(reader);
        }

        public void IntoOtherSave()
        {
            return;
        }


    }



// Harmony Patch for UIStorageWindow:


// MechaForge Patch for TryAddTask

    //UIReplicatorWindowPatch OnOkButtonClickPrefix

    // public int TakeItem(int itemId, int count, out int inc) for StorageComponent
    // Only if type is MechaForgeStorageTryAddTaskStorageComponent



}