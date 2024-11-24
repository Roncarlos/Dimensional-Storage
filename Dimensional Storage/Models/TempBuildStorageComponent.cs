using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Models
{
    public class TempBuildStorageComponent : StorageComponent
    {
        public TempBuildStorageComponent(int _size) : base(_size)
        {
            Debug.Log("DS - TempBuildStorageComponent constructor");
        }
    }
}