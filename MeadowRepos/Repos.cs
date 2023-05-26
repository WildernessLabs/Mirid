﻿namespace MeadowRepos
{
    public static class Repos
    {
        public static Dictionary<string, GitRepo> Repositories { get; set; }

        static Repos()
        {
            Repositories = new Dictionary<string, GitRepo>();
        }

        public static void AddRepo(string name, string githubOrg, string? sourceDirectory, IEnumerable<string> depedencyRepos)
        {
            var repo = new GitRepo()
            {
                Name = name,
                GitHubOrg = githubOrg,
                SourceDirectory = sourceDirectory ?? "",
                DependencyRepoNames = depedencyRepos
            };

            Repositories?.Add(name, repo);
        }

        public static void PopulateSampleRepos()
        {
            string org = "WildernessLabs";

            AddRepo("Meadow.Core.Samples", org, "Meadow.Core.Samples/Source/", null);
            AddRepo("Meadow.Project.Samples", org, "Meadow.Project.Samples/Source/", null);
            AddRepo("Meadow.ProjectLab.Samples", org, "Meadow.ProjectLab.Samples/Source/", null);
        }

        public static void PopulateRepos()
        {
            string org = "WildernessLabs";

            AddRepo("Meadow.Units", org, "Source", new List<string>());
            AddRepo("MQTTnet", org, "Source/MQTTnet", new List<string>());
            AddRepo("Meadow.Logging", org, "Source", new List<string> { "Meadow.Units" });

            AddRepo("Meadow.Contracts", org, "Source", new List<string> { "Meadow.Units", "Meadow.Logging" });
            AddRepo("Meadow.Modbus", org, "src", new List<string> { "Meadow.Logging", "Meadow.Contracts" });

            AddRepo("Meadow.Core", org, "Source", new List<string> { "Meadow.Logging", "Meadow.Contracts", "Meadow.Core" });
            AddRepo("Meadow.Foundation", org, "Source", new List<string> { "Meadow.Contracts", "Meadow.Core" });

            AddRepo("Meadow.Foundation.Featherwings", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Meadow.Foundation.Grove", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Meadow.Foundation.mikroBus", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });

            AddRepo("Meadow.ProjectLab", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation", "Meadow.Modbus" });
            AddRepo("GNSS_Sensor_Tracker", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Clima", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Juego", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
        }
    }
}