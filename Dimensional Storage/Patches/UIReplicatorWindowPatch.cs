using System.Reflection;
using HarmonyLib;

namespace Com.JiceeDev.DimensionalStorage.Patches
{
    public static class UIReplicatorWindowPatch
    {

        private static FieldInfo selectedRecipeField = typeof(UIReplicatorWindow).GetField("selectedRecipe", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(UIReplicatorWindow), "OnOkButtonClick")]
        [HarmonyPrefix]
        public static bool OnOkButtonClickPrefix(UIReplicatorWindow __instance)
        {
            var selectedRecipe = (RecipeProto)selectedRecipeField.GetValue(__instance);

            if (selectedRecipe == null || GameMain.isFullscreenPaused)
            {
                return true;
            }

            for (int i = 0; i < selectedRecipe.Items.Length; i++)
            {
                int itemId = selectedRecipe.Items[i];
                int itemCount = selectedRecipe.ItemCounts[i];

                // Add what we can from the Dimensional Storage
                DimensionalStorageSystem.TransferToPlayer(itemId, itemCount, itemCount);
            }

            return true;
        }
        
    }
}