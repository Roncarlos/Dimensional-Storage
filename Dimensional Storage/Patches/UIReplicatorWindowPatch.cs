using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

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
            
            var dimensionBonus = DimensionalStorageMod.TechManager.GetCachedDimensionalBonus();
            
            if(!dimensionBonus.CanReplicateWithDimensionalStorage)
            {
                return true;
            }
            
            var multiplier = GetItemMultiplierForRecipe(__instance, selectedRecipe);
            
            Dictionary<int, int> requiredItems = new Dictionary<int, int>();
            
            TakeOutItemsRecursive(selectedRecipe, multiplier, requiredItems);

            return true;
        }

        static int GetItemMultiplierForRecipe(UIReplicatorWindow window, RecipeProto recipe)
        {
            return window.multipliers.TryGetValue(recipe.ID, out var multiplier) ? multiplier : 1;
        }

        static void TakeOutItemsRecursive(RecipeProto recipe, int multiplier, Dictionary<int, int> requiredItems)
        {
            for (int i = 0; i < recipe.Items.Length; i++)
            {
                var itemId = recipe.Items[i];
                var neededItemCount = recipe.ItemCounts[i] * multiplier;
                
                if (neededItemCount <= 0) continue;
                if (requiredItems.TryGetValue(itemId, out var existingCount))
                {
                    // If we already have this item in the required items, just add to the count
                    requiredItems[itemId] = existingCount + neededItemCount;
                }
                else
                {
                    // Otherwise, add it to the required items
                    requiredItems[itemId] = neededItemCount;
                }
                
                var playerItemCount = GetItemCountInPlayerInventory(itemId);
                
                if (playerItemCount >= neededItemCount)
                {
                    // If we have enough items in the player's inventory, we can skip this item
                    continue;
                }
                
                neededItemCount = requiredItems[itemId] - playerItemCount;
                
                // Add what we can from the Dimensional Storage
                var missingCount = DimensionalStorageMod.DimensionalStorageSystem.TransferToPlayer(itemId, neededItemCount, neededItemCount);
                if (missingCount <= 0) continue;
                var item = LDB.items.Select(itemId);
                var handcraftRecipe = item.handcraft;
                var handcraftCount = item.handcraftProductCount;
                if (handcraftCount <= 0)
                {
                    // If the item is not handcraftable, we can't do anything more
                    continue;
                }
 
                if (handcraftRecipe != null)
                {
                    TakeOutItemsRecursive(handcraftRecipe, Mathf.CeilToInt(missingCount / (float)handcraftCount), requiredItems);
                }
            }
        }

        static int GetItemCountInPlayerInventory(int itemId)
        {
            var player = GameMain.data.mainPlayer;
            if (player == null) return 0;

            var inventory = player.package;
            if (inventory == null) return 0;
            Utils.ShouldIgnoreNextItemCountCall = true;
            
            return inventory.GetItemCount(itemId);
        }
    }
}