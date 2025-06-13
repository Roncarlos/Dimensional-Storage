using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage
{
    public interface ISaveComponent
    {
        void Export(BinaryWriter writer);
        void Import(BinaryReader reader);
    }
    
    
    public class DimensionalStorageSystem : ISaveComponent
    {
        public HashSet<StorageComponent> StorageContainers { get; private set; } = new HashSet<StorageComponent>();

        private bool CanAddMoreStorageContainers()
        {
            return StorageContainers.Count < DimensionalStorageMod.TechManager.GetCachedDimensionalBonus().NumberOfDimensionalStorage;
        }
        
        public int GetRemainingLinks()
        {
            return DimensionalStorageMod.TechManager.GetCachedDimensionalBonus().NumberOfDimensionalStorage - StorageContainers.Count;
        }
        
        public bool AddStorageContainer(StorageComponent storage)
        {
            return CanAddMoreStorageContainers() && StorageContainers.Add(storage);
        }


        public void RemoveStorageContainer(StorageComponent storage)
        {
            StorageContainers.Remove(storage);
        }

        public int GetItemCount(int itemId)
        {
            return StorageContainers.Select(c => c.GetItemCount(itemId)).Sum();
        }

        public void TransferTo(StorageComponent other, int itemId, int count)
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

        /// <summary>
        /// Transfers items to the player, taking from all available storage containers.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        /// <param name="itemInc"></param>
        /// <returns>
        /// The number of items that could not be transferred to the player.
        /// </returns>
        public int TransferToPlayer(int itemId, int count, int itemInc)
        {
            return TransferToPlayer(GameMain.data.mainPlayer, itemId, count, itemInc);
        }

        
        /// <summary>
        /// Transfers items to the player, taking from all available storage containers.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        /// <param name="itemInc"></param>
        /// <returns>
        /// The number of items that could not be transferred to the player.
        /// </returns>
        public int TransferToPlayer(Player player, int itemId, int count, int itemInc)
        {
            int maximumItemsAvailable = StorageContainers.Select(c => c.GetItemCount(itemId)).Sum();
            
            var missingCount =  maximumItemsAvailable < count ? count - maximumItemsAvailable : 0;
            
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
            
            return missingCount;
        }

        public void Export(BinaryWriter writer)
        {
            writer.Write(StorageContainers.Count);
            foreach (var storage in StorageContainers)
            {
                writer.Write(storage.id);
            }
            
        }

        public void Import(BinaryReader reader)
        {
            StorageContainers.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                var storage = GameMain.data.factories.First(c => c.factoryStorage.storagePool[id] != null).factoryStorage.storagePool[id];
                if(storage == null)
                {
                    continue;
                }
                StorageContainers.Add(storage);
            }
        }
    }
}