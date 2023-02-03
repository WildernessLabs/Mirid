using System.Collections.Generic;
using System.IO;

namespace ReferenceSwitcher
{
    public class RepoData
    {
        public Dictionary<string, string> Repos { get; private set; }

        string pathDepth = "../../../../../"; //hack for now

        public RepoData()
        {
            Repos = new Dictionary<string, string>();

            AddRepo("Meadow.Units", "Meadow.Units/Source/");
            AddRepo("Meadow.Modbus", "Meadow.Modbus/src/");
            AddRepo("MQTTnet", "MQTTnet/Source/MQTTnet/");
            AddRepo("Meadow.Logging", "Meadow.Logging/Source/");
            AddRepo("Meadow.Contracts", "Meadow.Contracts/Source/");
            AddRepo("Meadow.Core", "Meadow.Core/Source/");
            AddRepo("Meadow.Foundation", "Meadow.Foundation/Source/");
            AddRepo("Meadow.Foundation.Core", "Meadow.Foundation/Source/Meadow.Foundation.Core/");
            AddRepo("Meadow.Foundation.Featherwings", "Meadow.Foundation.Featherwings/Source/");
            AddRepo("Meadow.Foundation.Grove", "Meadow.Foundation.Grove/Source/");
            AddRepo("Meadow.Foundation.mikroBus", "Meadow.Foundation.mikroBus/Source/");
            AddRepo("Meadow.ProjectLab", "Meadow.ProjectLab/Source/");
        }

        public void AddRepo(string name, string path)
        {
            Repos.Add(name, Path.Combine(pathDepth, path));
        }
    }
}
