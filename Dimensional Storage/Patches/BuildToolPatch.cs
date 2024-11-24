using System.Reflection;
using Com.JiceeDev.DimensionalStorage.Models;
using HarmonyLib;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    public class BuildToolPatch
    {
        private static FieldInfo tmpPackageField = typeof(BuildTool).GetField("tmpPackage", BindingFlags.NonPublic | BindingFlags.Instance);
        public static Player player;
        
        
        // Patch for _GameTick(long time) for class BuildTool
        [HarmonyPatch(typeof(BuildTool), "_GameTick")]
        [HarmonyPrefix]
        public static void _GameTickPrefix(BuildTool __instance, long time)
        {
            // There is a tmpPackage field in BuildTool
            // I want to check it's value and if it's null i want to create a new TempBuildStorageComponent
            var tmpPackage = (StorageComponent) tmpPackageField.GetValue(__instance);
            
            player = __instance.player;

            if (__instance.active && tmpPackage == null)
            {
                Debug.Log("DS - _GameTickPrefix: tmpPackage is null");
                var tempStorage = new TempBuildStorageComponent(__instance.player.package.size);
                tmpPackageField.SetValue(__instance, tempStorage);
            }
        }
    }
}