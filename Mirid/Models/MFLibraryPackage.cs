using System.Collections.Generic;
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
                .EnumerateFiles("*.cs", SearchOption.TopDirectoryOnly)
                .ToList();

            var candidates = new List<MFDriver>();

            foreach (var file in files)
            {
                var stem = Path.GetFileNameWithoutExtension(file.Name);

                if (stem.StartsWith("I")) continue;
                if (stem.Contains('.')) continue; // partial class
                if (ignorePatterns.Any(p => stem.Contains(p))) continue;

                var driverCode = new MFDriverCode(file);
                var sample = Assets.GetSampleForName(stem + "_Sample");
                candidates.Add(new MFDriver(this, driverCode, sample, docsOverridePath));
            }

            if (candidates.Count == 0) return;

            // Each L&F package = one row in the table. Pick the primary class (matching
            // the package leaf name), falling back to the first candidate alphabetically.
            var leafName = PackageName.Split('.').Last();
            var primary = candidates.FirstOrDefault(d => d.Name == leafName) ?? candidates[0];
            Drivers.Add(primary);
        }
    }
}
