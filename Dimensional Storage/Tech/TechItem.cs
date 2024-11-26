using System;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage.Tech
{
    [Serializable]
    public class TechItem
    {
        public int ID = 0;
        public string IDLong = "";
        public string Name = "";
        public string Description = "";
        public string Conclusion = "";
        public string IconPath = "";
        public int[] RequiredTechs = Array.Empty<int>();
        public int[] Jellos = Array.Empty<int>();
        public int[] JellosRates = Array.Empty<int>();
        public int HashNeeded = 0;
        public int[] UnlockRecipes = Array.Empty<int>();
        public Vector2 Position = new Vector2(0, 0);
        public DimensionalBonus Bonuses = new DimensionalBonus();
    }
}