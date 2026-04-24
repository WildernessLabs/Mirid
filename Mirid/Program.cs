using Mirid.Models;
using Mirid.Output;
using Mirid.Outputs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirid
{
    class Program
    {
        static readonly string MFCoreGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Core/";
        static readonly string MFGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Peripherals/";
        static readonly string MFFrameworksGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Libraries_and_Frameworks/";
        static readonly string MFGroveGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation.Grove/tree/main/Source/";
        static readonly string MFFeatherGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation.FeatherWings/tree/main/Source/";
        static readonly string MFMikroBusGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation.MikroBus/tree/main/Source/";
        static readonly string MFCompositeGitHubUrl = "https://github.com/wildernesslabs/meadow.foundation.compositedevices/tree/main/Source/";

        static readonly Dictionary<string, MFDriverSet> driverSets = new();

        static readonly string CORE_PERIPHERALS = "Core Peripherals";
        static readonly string LIBRARIES_AND_FRAMEWORKS = "Libraries and Frameworks";
        static readonly string EXTERNAL_PERIPHERALS = "External Peripherals";
        static readonly string FEATHERWINGS = "FeatherWings";
        static readonly string SEEED_STUDIO_GROVE = "Seeed Studio Grove";
        static readonly string COMPOSITE_DEVICES = "Composite Devices";

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            var configPath = Path.Combine(AppContext.BaseDirectory, "mirid.config.json");
            var config = MiridConfig.Load(configPath);

            LoadDriverSets(config);

            UpdateProjectMetadata();

            UpdateDocs();

            WritePeripheralTables(driverSets.Values.ToList());
            //RunDriverReport();
        }

        static void UpdateProjectMetadata()
        {
            foreach (var driverSet in driverSets.Values)
            {
                driverSet.UpdateProjectMetadata();
            }
        }

        static void UpdateDocs()
        {
            if (driverSets.TryGetValue(CORE_PERIPHERALS, out var core)) UpdatePeripheralDocs(core);
            //if (driverSets.TryGetValue(LIBRARIES_AND_FRAMEWORKS, out var frameworks)) UpdatePeripheralDocs(frameworks);
            if (driverSets.TryGetValue(EXTERNAL_PERIPHERALS, out var external)) UpdatePeripheralDocs(external);
            if (driverSets.TryGetValue(FEATHERWINGS, out var feather)) UpdatePeripheralDocs(feather);
            if (driverSets.TryGetValue(SEEED_STUDIO_GROVE, out var grove)) UpdatePeripheralDocs(grove);
            if (driverSets.TryGetValue(COMPOSITE_DEVICES, out var composite)) UpdatePeripheralDocs(composite);
        }

        static void LoadDriverSets(MiridConfig config)
        {
            Console.WriteLine($"Load {CORE_PERIPHERALS} driver set");
            var coreDriverSet = new MFCoreDriverSet(
                name: CORE_PERIPHERALS,
                MFSourcePath: config.MFSourcePath,
                driverSourcePath: config.MFCorePeripheralsPath,
                docsOverridePath: config.MFDocsOverridePath,
                githubUrl: MFCoreGitHubUrl);
            Console.WriteLine($"Processed {coreDriverSet.DriverPackages.Count} packages with {GetDriverCount(coreDriverSet)} drivers");

            /*
            Console.WriteLine($"Load {LIBRARIES_AND_FRAMEWORKS} driver set");
            var frameworksDriverSet = new MFDriverSet(LIBRARIES_AND_FRAMEWORKS,
                config.MFSourcePath,
                config.MFFrameworksPath,
                config.MFDocsOverridePath,
                MFFrameworksGitHubUrl);
            Console.WriteLine($"Processed {frameworksDriverSet.DriverPackages.Count} packages with {GetDriverCount(frameworksDriverSet)} drivers");
            */

            Console.WriteLine($"Load {EXTERNAL_PERIPHERALS} driver set");
            var peripheralsDriverSet = new MFDriverSet(EXTERNAL_PERIPHERALS,
                config.MFSourcePath,
                config.MFPeripheralsPath,
                config.MFDocsOverridePath,
                MFGitHubUrl);
            Console.WriteLine($"Processed {peripheralsDriverSet.DriverPackages.Count} packages with {GetDriverCount(peripheralsDriverSet)} drivers");

            Console.WriteLine($"Load {SEEED_STUDIO_GROVE} driver set");
            var groveDriverSet = new MFDriverSet(SEEED_STUDIO_GROVE, config.MFSourcePath, config.MFGrovePath, config.MFGroveDocsOverridePath, MFGroveGitHubUrl);
            Console.WriteLine($"Processed {groveDriverSet.DriverPackages.Count} packages with {GetDriverCount(groveDriverSet)} drivers");

            Console.WriteLine($"Load {FEATHERWINGS} driver set");
            var featherDriverSet = new MFDriverSet(FEATHERWINGS, config.MFSourcePath, config.MFFeatherwingPath, config.MFFeatherwingDocsOverridePath, MFFeatherGitHubUrl);
            Console.WriteLine($"Processed {featherDriverSet.DriverPackages.Count} packages with {GetDriverCount(featherDriverSet)} drivers");

            Console.WriteLine($"Load {COMPOSITE_DEVICES} driver set");
            var compositeDriverSet = new MFDriverSet(COMPOSITE_DEVICES, config.MFSourcePath, config.MFCompositePath, config.MFCompositeDocsOverridePath, MFCompositeGitHubUrl);
            Console.WriteLine($"Processed {compositeDriverSet.DriverPackages.Count} packages packages with {GetDriverCount(compositeDriverSet)} drivers");

            //common location so we can turn em off and on ... order counts
            driverSets.Add(CORE_PERIPHERALS, coreDriverSet);
            //driverSets.Add(LIBRARIES_AND_FRAMEWORKS, frameworksDriverSet);
            driverSets.Add(EXTERNAL_PERIPHERALS, peripheralsDriverSet);
            driverSets.Add(SEEED_STUDIO_GROVE, groveDriverSet);
            driverSets.Add(FEATHERWINGS, featherDriverSet);
            driverSets.Add(COMPOSITE_DEVICES, compositeDriverSet);

            int total = 0;
            foreach (var d in driverSets.Values)
            {
                total += GetDriverCount(d);
            }

            Console.WriteLine($"Found {total} drivers");
        }

        static int GetDriverCount(MFDriverSet driverSet)
        {
            int count = 0;

            foreach (var p in driverSet.DriverPackages)
            {
                count += p.NumberOfDrivers;
            }
            return count;
        }

        static void WritePeripheralTables(List<MFDriverSet> driverSets)
        {
            Console.WriteLine("Write Peripheral Tables");

            //  PeripheralDocsOutput.WritePeripheralTablesSimple(docSet.DriverPackages);
            PeripheralDocsOutput.WritePeripheralTables(driverSets);
        }

        //ToDo - rework in the context of doc sets
        static void RunDriverReport(MFDriverSet docSet)
        {
            Console.Clear();
            Console.WriteLine("Driver Report");

            CsvOutput.WritePackagesCSV(docSet.DriverPackages, "AllPeripherals.csv");
            CsvOutput.WriteDriversCSV(docSet.DriverPackages, "AllDrivers.csv");

            CsvOutput.WritePackagesCSV(docSet.DriverPackages.Where(d => d.IsPublished == false).ToList(), "InProgressPeripherals.csv");

            return;
            //need to standardize the folder and naming convensions for the libs and frameworks first s

            //Libraries and frameworks
            var frameworkFiles = FileCrawler.GetAllProjectsInFolders(string.Empty);
            var frameworkProjectFiles = FileCrawler.GetDriverProjects(frameworkFiles);

            /*
            foreach (var file in frameworkProjectFiles)
            {
                frameworkNugets.Add(new MFPackage(file, MFDocsOverridePath));
            }

            CsvOutput.WritePackagesCSV(frameworkNugets, "AllFrameworks.csv");
            */
        }

        static void UpdatePeripheralDocs(MFDriverSet driverSet)
        {
            int count = 0;

            foreach (var package in driverSet.DriverPackages)
            {
                foreach (var driver in package.Drivers)
                {
                    count++;

                    if (driver.HasDocOverride == false)
                    {
                        Console.WriteLine($"No docs override for {driver.Name} .... creating file ....");
                        driver.CreateDocsOverride();
                    }

                    var relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, Path.GetDirectoryName(driver.FilePath));
                    //driver folder hack
                    if (Path.GetFileName(relativePath) == "Drivers") //treats the last folder name as a file
                    {
                        relativePath = Path.GetDirectoryName(relativePath);
                    }
                    if (Path.GetFileName(relativePath) == "Driver") //treats the last folder name as a file
                    {
                        relativePath = Path.GetDirectoryName(relativePath);
                    }

                    var uri = new Uri(driverSet.GitHubUrl);
                    var githubCodeUri = new Uri(uri, relativePath);

                    string githubDatasheetUrl = String.Empty;
                    if (package.HasDataSheet &&
                       !string.IsNullOrWhiteSpace(package.Assets.DatasheetPath))
                    {
                        relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, package.Assets.DatasheetPath);
                        uri = new Uri(driverSet.GitHubUrl);
                        var githubDatasheetUri = new Uri(uri, relativePath);
                        githubDatasheetUrl = $"{githubDatasheetUri}";
                    }

                    driver.UpdateDocHeader($"{githubCodeUri}", githubDatasheetUrl);

                    if (driver.HasSample)
                    {
                        relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, driver.SamplePath);
                        uri = new Uri(driverSet.GitHubUrl);
                        var sampleUri = new Uri(uri, relativePath);

                        driver.UpdateSnipSnop($"{sampleUri}");
                    }
                    else
                    {
                        Console.WriteLine($"No sample found for {driver.Name}");
                    }
                }
            }

            Console.WriteLine($"Found {count} drivers in {driverSet.SetName}");

            /*
             *
            var drivers = driverSet.DriverPackages.SelectMany(p => p.Drivers).ToList();

            foreach(var driver in drivers)
            {
                if(driver.HasSample)
                {
                    var relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, driver.SamplePath);
                    var uri = new Uri(driverSet.GitHubUrl);
                    var sampleUri = new Uri(uri, relativePath);

                    driver.UpdateSnipSnop($"{sampleUri}");
                }

              //generate source Uri
                var relativePath = Path.GetRelativePath(docSet.DriverSetSourcePath, driver.FilePath);
                relativePath = Path.GetDirectoryName(relativePath);
                var uri = new Uri(docSet.GitHubUrl);
                var sourceUri = new Uri(uri, relativePath);

                driver.UpdateDocHeader($"{driverSet.GitHubUrl}");
            }*/
        }
    }
}
