using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mirid
{
    public class MiridConfig
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public string MFSourcePath { get; set; } = "../../../../../Meadow.Foundation/Source/";
        public string MFCorePeripheralsPath { get; set; } = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Core";
        public string MFPeripheralsPath { get; set; } = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        public string MFDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation";
        public string MFFrameworksPath { get; set; } = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Libraries_and_Frameworks";
        public string MFGrovePath { get; set; } = "../../../../../Meadow.Foundation.Grove/Source/";
        public string MFGroveDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.Grove";
        public string MFFeatherwingPath { get; set; } = "../../../../../Meadow.Foundation.Featherwings/Source/";
        public string MFFeatherwingDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.Featherwings";
        public string MFMikroBusPath { get; set; } = "../../../../../Meadow.Foundation.mikroBUS/Source/";
        public string MFMikroBusDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.MikroBus";
        public string MFCompositePath { get; set; } = "../../../../../Meadow.Foundation.CompositeDevices/Source/";
        public string MFCompositeDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.CompositeDevices";

        public static MiridConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                var defaults = new MiridConfig();
                File.WriteAllText(configPath, JsonSerializer.Serialize(defaults, SerializerOptions));
                Console.WriteLine($"Config file not found — created template at {configPath}");
                Console.WriteLine("Edit it to match your local repo paths, then re-run.");
                Environment.Exit(0);
            }

            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<MiridConfig>(json, SerializerOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize config from {configPath}");
        }
    }
}
