using System;

namespace Com.JiceeDev.DimensionalStorage.Tech
{
    public interface ITechManager : IDisposable
    {
        void RegisterTechs();
        DimensionalBonus GetCurrentDimensionalBonus(int techID);
        DimensionalBonus GetAllDimensionalBonus();
        DimensionalBonus GetCachedDimensionalBonus();
        void ReloadCache();

    }
}