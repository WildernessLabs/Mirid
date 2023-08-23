using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    //custom implimentation for M.F. Core
    public class MFCorePackage : MFPackage
    {
        // namespaces we care about 

        readonly string[] coreDriverNamespaces =
        {
            "Meadow.Foundation.Audio",
            "Meadow.Foundation.Controllers",
            "Meadow.Foundation.Generators",
            "Meadow.Foundation.Leds",
            "Meadow.Foundation.Motors",
            "Meadow.Foundation.Relays",
            "Meadow.Foundation.Sensors"
        };

        readonly string[] ignoreFileList =
        {
            "Enum",
            "Calibration",
            "Configuration",
            "Base",
            "Helper",
            "TypicalForwardVoltage",
            "TwoBitGray"
        };

        public MFCorePackage(FileInfo driverProjectFile, string docsOverridePath) 
            : base(driverProjectFile, docsOverridePath)
        {
        }

        protected override void LoadDriverResouces(FileInfo driverProjectFile, string docsOverridePath)
        {
            if (File.Exists(driverProjectFile.FullName) == false)
            {
                throw new FileNotFoundException($"Driver project not found {driverProjectFile.FullName}");
            }

            //load nuget project (for metadata)
            NugetProject = new MFPackageProject(driverProjectFile);

            //Load assets
            var parentDir = driverProjectFile.Directory.Parent;
            Assets = new MFDriverAssets(parentDir);

            //load driver code
            var files = driverProjectFile.Directory.EnumerateFiles("*.cs", SearchOption.AllDirectories).ToList();

            var filesSorted = new List<FileInfo>();

            foreach (var f in files)
            {
                //hacky
                if (f.Name.StartsWith("I")) continue;
                if( ignoreFileList.Any(f.Name.Contains)) continue;

                var driver = new MFDriverCode(f);

                if(coreDriverNamespaces.Any(driver.Namespace.Contains))
                {
                    Drivers.Add(new MFDriver(this, driver, GetSampleForDriver(driverProjectFile, driver), docsOverridePath));
                }
            }
        }

        MFDriverSample GetSampleForDriver(FileInfo driverProjectFile, MFDriverCode driver)
        {
            var directories = driverProjectFile.Directory.Parent.GetDirectories("Meadow.Foundation.Core.Samples").FirstOrDefault().GetDirectories();
            
            var directory = directories.FirstOrDefault(f => f.Name.Contains(driver.Name));

            if (directory == null || directory.Name.Length == 0)
            {
                Console.WriteLine($"No sample found for {driver.Name}");
                return null;
            }

            return new MFDriverSample(directory);
        }
    }
}