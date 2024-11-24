using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.JiceeDev.DimensionalStorage
{
    public static class DimensionalStorageSystem
    {
        public static HashSet<StorageComponent> StorageContainers { get; private set; } = new HashSet<StorageComponent>();
        public static Player Player { get; } = GameMain.mainPlayer;

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

        public static void TransferToPlayer(int itemId, int count, int itemInc)
        {
            TransferToPlayer(Player, itemId, count, itemInc);
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