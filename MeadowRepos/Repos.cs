namespace MeadowRepos
{
    public static class Repos
    {
        public static Dictionary<string, GitRepo> Repositories { get; set; }

        public static readonly string[] ExcludedProjects = { "Simulated", "Sample", "sample", "Test", "test", "Utilities", "Update", "client", "Demo", "Prototype", "ProKit", "HackKit", "Mobile", "mobile" };

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

            AddRepo("Meadow.Units", org, "Source\\Meadow.Units", new List<string>());
            AddRepo("MQTTnet", org, "Source\\MQTTnet", new List<string>());
            AddRepo("Meadow.Logging", org, "Source", new List<string>());

            AddRepo("Meadow.Contracts", org, "Source\\Meadow.Contracts", new List<string> { "Meadow.Units", "Meadow.Logging" });
            AddRepo("Meadow.Modbus", org, "src", new List<string> { "Meadow.Contracts" });
            AddRepo("Maple", org, "Source", new List<string> { "Meadow.Logging" });
            AddRepo("Meadow.TensorFlow", org, "Source", new List<string> { "Meadow.Logging" });

            AddRepo("Meadow.Core", org, "source", new List<string> { "Meadow.Contracts", "Meadow.Logging", "Meadow.Core", "Meadow.Modbus", "MQTTnet", "Meadow.Foundation" });
            AddRepo("Meadow.Foundation", org, "Source", new List<string> { "Meadow.Core", "Meadow.Contracts", "Meadow.Foundation", "Meadow.Modbus" });

            AddRepo("Meadow.Foundation.MBus", org, "Source", new List<string> { "Meadow.Core", "Meadow.Contracts", "Meadow.Logging" });
            AddRepo("Meadow.Foundation.CompositeDevices", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Meadow.Foundation.Featherwings", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Meadow.Foundation.Grove", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Meadow.Foundation.mikroBus", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation", "Meadow.ProjectLab", "Meadow.Foundation.MBus" });

            AddRepo("Meadow.ProjectLab", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation", "Meadow.Modbus" });
            AddRepo("GNSS_Sensor_Tracker", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
            AddRepo("Clima", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation", "Maple" });
            AddRepo("Juego", org, "Source", new List<string> { "Meadow.Core", "Meadow.Foundation" });
        }
    }
}