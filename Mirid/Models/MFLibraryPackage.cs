using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFLibraryPackage : MFPackage
    {
        static readonly string[] ignorePatterns =
        {
            "Enum", "Base", "Config", "Helper", "Utilities", "Utility",
            "EventArgs", "Converter", "Attribute", "Constants", "Extension",
            "Collection", "Serializer", "Provider", "Builder", "Theme",
            "Characters", "Page", "Handler", "Args", "Strategy", "Info"
        };

        public MFLibraryPackage(FileInfo driverProjectFile, string docsOverridePath)
            : base(driverProjectFile, docsOverridePath)
        {
        }

        protected override void LoadDriverResources(FileInfo driverProjectFile, string docsOverridePath)
        {
            if (!File.Exists(driverProjectFile.FullName))
                throw new FileNotFoundException($"Driver project not found {driverProjectFile.FullName}");

            NugetProject = new MFPackageProject(driverProjectFile);

            var parentDir = driverProjectFile.Directory.Parent;
            Assets = new MFDriverAssets(parentDir);

            var files = driverProjectFile.Directory
                .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                .ToList();

            foreach (var file in files)
            {
                var stem = Path.GetFileNameWithoutExtension(file.Name);

                if (stem.StartsWith("I")) continue;
                if (stem.Contains('.')) continue; // partial class
                if (ignorePatterns.Any(p => stem.Contains(p))) continue;

                var driverCode = new MFDriverCode(file);
                var sample = Assets.GetSampleForName(stem + "_Sample");
                Drivers.Add(new MFDriver(this, driverCode, sample, docsOverridePath));
            }
        }
    }
}
