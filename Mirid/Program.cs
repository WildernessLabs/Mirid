﻿using System;
using System.Collections.Generic;
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
      
        static readonly Dictionary<string, MFDriverSet> driverSets = new Dictionary<string, MFDriverSet>();

        static readonly string CORE_PERIPHERALS = "Core Peripherals";
        static readonly string LIBRARIES_AND_FRAMEWORKS = "Libraries and Frameworks";
        static readonly string EXTERNAL_PERIPHERALS = "External Peripherals";
        static readonly string FEATHERWINGS = "FeatherWings";
        static readonly string SEEED_STUDIO_GROVE = "Seeed Studio Grove";

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            LoadDriverSets();

            // UpdateDocs(); 

            WritePeripheralTables(driverSets.Values.ToList());
            // RunDriverReport();
        }

        static void UpdateDocs()
        {
            UpdatePeripheralDocs(driverSets[CORE_PERIPHERALS]);
            UpdatePeripheralDocs(driverSets[LIBRARIES_AND_FRAMEWORKS]);
            UpdatePeripheralDocs(driverSets[EXTERNAL_PERIPHERALS]);
            UpdatePeripheralDocs(driverSets[FEATHERWINGS]);
            UpdatePeripheralDocs(driverSets[SEEED_STUDIO_GROVE]);
        }

        static void LoadDriverSets()
        { 
            Console.WriteLine($"Load {CORE_PERIPHERALS} driver set");
            var coreDriverSet = new MFCoreDriverSet(
                name: CORE_PERIPHERALS,
                MFSourcePath: MFSourcePath,
                driverSourcePath: MFCorePerihperalsPath,
                docsOverridePath: MFDocsOverridePath,
                githubUrl: MFCoreGitHubUrl);
            Console.WriteLine($"Processed {coreDriverSet.DriverPackages.Count} packages");
            

            Console.WriteLine($"Load {LIBRARIES_AND_FRAMEWORKS} driver set");
            var frameworksDriverSet = new MFDriverSet(LIBRARIES_AND_FRAMEWORKS,
                MFSourcePath,
                MFFrameworksPath,
                MFDocsOverridePath,
                MFFrameworksGitHubUrl);
            Console.WriteLine($"Processed {frameworksDriverSet.DriverPackages.Count} packages");


            Console.WriteLine($"Load {EXTERNAL_PERIPHERALS} driver set");
            var peripheralsDriverSet = new MFDriverSet(EXTERNAL_PERIPHERALS, 
                MFSourcePath, 
                MFPeripheralsPath, 
                MFDocsOverridePath, 
                MFGitHubUrl);
            Console.WriteLine($"Processed {peripheralsDriverSet.DriverPackages.Count} packages");


            Console.WriteLine($"Load {SEEED_STUDIO_GROVE} driver set");
            var groveDriverSet = new MFDriverSet(SEEED_STUDIO_GROVE, MFSourcePath, MFGrovePath, MFGroveDocsOverridePath, MFGroveGitHubUrl);
            Console.WriteLine($"Processed {groveDriverSet.DriverPackages.Count} packages");


            Console.WriteLine($"Load {FEATHERWINGS} driver set");
            var featherDriverSet = new MFDriverSet(FEATHERWINGS, MFSourcePath, MFFeatherwingPath, MFFeatherwingDocsOverridePath, MFFeatherGitHubUrl);
            Console.WriteLine($"Processed {featherDriverSet.DriverPackages.Count} packages");
  

            //common location so we can turn em off and on ... order counts
            driverSets.Add(CORE_PERIPHERALS, coreDriverSet);
            driverSets.Add(LIBRARIES_AND_FRAMEWORKS, frameworksDriverSet);
            driverSets.Add(EXTERNAL_PERIPHERALS, peripheralsDriverSet);
            driverSets.Add(SEEED_STUDIO_GROVE, groveDriverSet);
            driverSets.Add(FEATHERWINGS, featherDriverSet);
        }

        static void WritePeripheralTables(List<MFDriverSet> driverSets)
        {
            Console.Clear();
            Console.WriteLine("Driver Report");

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