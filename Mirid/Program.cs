using System;
using System.IO;
using System.Linq;
using Mirid.Models;
using Mirid.Output;
using Mirid.Outputs;

namespace Mirid
{
    class Program
    {
        public static string MCFullPath = "../../../../../Meadow.Core/Source/";
        public static string MFSourcePath = "../../../../../Meadow.Foundation/Source/";

        public static string MFCorePerihperalsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Core";
        public static string MFCoreGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Core/";

        public static string MFPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        public static string MFDocsOverridePath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";
        public static string MFGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Peripherals/";

        public static string MFFrameworksPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Libraries_and_Frameworks";
        public static string MFFrameworksGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Libraries_and_Frameworks/";

        public static string MFGrovePath = "../../../../../Meadow.Foundation.Grove/Source/";
        public static string MFGroveDocsOverridePath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation.Grove";
        public static string MFGroveGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation.Grove/tree/main/Source/";

        public static string MFFeatherwingPath = "../../../../../Meadow.Foundation.Featherwings/Source/";
        public static string MFFeatherwingDocsOverridePath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation.Featherwings";
        public static string MFFeatherGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation.FeatherWings/tree/main/Source/";

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            Console.WriteLine("Load Meadow.Foundation.Core peripherals driver set");
            var coreDriverSet = new MFCoreDriverSet(
                name: "M.F.Core Peripherals",
                MFSourcePath: MFSourcePath,
                driverSourcePath: MFCorePerihperalsPath,
                docsOverridePath: MFDocsOverridePath,
                githubUrl: MFCoreGitHubUrl);
            Console.WriteLine($"Processed {coreDriverSet.DriverPackages.Count} packages");
            UpdatePeripheralDocs(coreDriverSet);

            Console.WriteLine("Load Meadow.Foundation frameworks driver set");
            var frameworksDriverSet = new MFDriverSet("M.F. Frameworks",
                MFSourcePath,
                MFFrameworksPath,
                MFDocsOverridePath,
                MFFrameworksGitHubUrl);
            Console.WriteLine($"Processed {frameworksDriverSet.DriverPackages.Count} packages");
            UpdatePeripheralDocs(frameworksDriverSet);

            Console.WriteLine("Load Meadow.Foundation peripherals driver set");
            var peripheralsDriverSet = new MFDriverSet("M.F. Peripherals", 
                MFSourcePath, 
                MFPeripheralsPath, 
                MFDocsOverridePath, 
                MFGitHubUrl);
            Console.WriteLine($"Processed {peripheralsDriverSet.DriverPackages.Count} packages");
            UpdatePeripheralDocs(peripheralsDriverSet);

            Console.WriteLine("Load Meadow.Foundation.Grove driver set");
            var groveDriverSet = new MFDriverSet("M.F. Grove", MFSourcePath, MFGrovePath, MFGroveDocsOverridePath, MFGroveGitHubUrl);
            Console.WriteLine($"Processed {groveDriverSet.DriverPackages.Count} packages");
            UpdatePeripheralDocs(groveDriverSet);

            Console.WriteLine("Load Meadow.Foundation.Featherwing doc set");
            var featherDriverSet = new MFDriverSet("M.F. FeatherWings", MFSourcePath, MFFeatherwingPath, MFFeatherwingDocsOverridePath, MFFeatherGitHubUrl);
            Console.WriteLine($"Processed {featherDriverSet.DriverPackages.Count} packages");
            UpdatePeripheralDocs(featherDriverSet);

            // WritePeripheralTables();
            // RunDriverReport();
        }

        static void WritePeripheralTables(MFDriverSet docSet)
        {
            Console.Clear();
            Console.WriteLine("Driver Report");

            PeripheralDocsOutput.WritePeripheralTablesSimple(docSet.DriverPackages);
            PeripheralDocsOutput.WritePeripheralTables(docSet.DriverPackages);
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
            var frameworkFiles = FileCrawler.GetAllProjectsInFolders(MFFrameworksPath);
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

            foreach(var package in driverSet.DriverPackages)
            {
                foreach (var driver in package.Drivers)
                {
                    count++;

                    if (driver.HasDocOverride == false)
                    {
                        Console.WriteLine($"No docs override for {driver.Name}");
                        continue;
                    }

                    var relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, Path.GetDirectoryName(driver.FilePath));
                    //driver folder hack
                    if(Path.GetFileName(relativePath) == "Driver") //treats the last folder name as a file
                    {
                        relativePath = Path.GetDirectoryName(relativePath);
                    }

                    var uri = new Uri(driverSet.GitHubUrl);
                    var projUri = new Uri(uri, relativePath);

                    driver.UpdateDocHeader($"{projUri}");

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

            Console.WriteLine($"Found {count} drivers in {driverSet.DocSetName}");

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