using BepInEx.Configuration;
using CommonAPI;

namespace Com.JiceeDev.DimensionalStorage
{
    public static class GlobalConfiguration
    {
        public static class Tech
        {
            public const string SectionName = "Tech";
            public readonly static string[] DefaultTechsPaths = new string[]
            {
                "default.json"
            };
            public static string TechsPath { get; set; } = "default.json";
            public static int BaseNumberOfDimensionalStorage { get; set; } = 1;
            public static bool BaseCanBuildWithDimensionalStorage { get; set; } = false;

            public static bool BaseCanReplicateWithDimensionalStorage { get; set; } = false;
        }


        public static void Init(ConfigFile config)
        {
            Tech.TechsPath = config.Bind(Tech.SectionName, "Filename where the tech tree is stored", "default.json", new ConfigDescription(
                "Change The tech tree used, by default it uses the techs.json file in the config folder.\n" +
                "Some mods may require a different tech tree, so you can change it here.\n" +
                $"Already included tech trees: {string.Join(", ", Tech.DefaultTechsPaths)}")).Value;

            Tech.BaseNumberOfDimensionalStorage = config.Bind(Tech.SectionName, "Base number of Dimensional Storage", 1, new ConfigDescription(
                "The number of Dimensional Storage that the player starts with.")).Value;
            
            Tech.BaseCanBuildWithDimensionalStorage = config.Bind(Tech.SectionName, "Can build with Dimensional Storage", false, new ConfigDescription(
                "If true, the player can build with Dimensional Storage right from the start.")).Value;
            
            Tech.BaseCanReplicateWithDimensionalStorage = config.Bind(Tech.SectionName, "Can replicate with Dimensional Storage", false, new ConfigDescription(
                "If true, the player can replicate with Dimensional Storage right from the start.")).Value;

        }
    }
}