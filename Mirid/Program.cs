using System;
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
        
        public static string MFPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        public static string MFDocsOverridePath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";
        public static string MFGitHubUrl = "https://github.com/WildernessLabs/Meadow.Foundation/tree/master/Source/Meadow.Foundation.Peripherals/";

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

            Console.WriteLine("Load Meadow.Foundation frameworks doc set");
            var frameworksDocSet = new MFDocSet("MFFrameworks",
                MFSourcePath,
                MFFrameworksPath,
                MFDocsOverridePath,
                MFFrameworksGitHubUrl);
            Console.WriteLine($"Processed {frameworksDocSet.DriverPackages.Count} packages");
            UpdatePeripheralDocs(frameworksDocSet, true);

            Console.WriteLine("Load Meadow.Foundation peripherals doc set");
            var peripheralsDocSet = new MFDocSet("MFPeripherals", 
                MFSourcePath, 
                MFPeripheralsPath, 
                MFDocsOverridePath, 
                MFGitHubUrl);
            Console.WriteLine($"Processed {peripheralsDocSet.DriverPackages.Count} packages");
            //UpdatePeripheralDocs(peripheralsDocSet, true);

            Console.WriteLine("Load Meadow.Foundation.Grove doc set");
            var groveDocSet = new MFDocSet("MFGrove", MFSourcePath, MFGrovePath, MFGroveDocsOverridePath, MFGroveGitHubUrl);
            Console.WriteLine($"Processed {groveDocSet.DriverPackages.Count} packages");
            //UpdatePeripheralDocs(groveDocSet, false);

            Console.WriteLine("Load Meadow.Foundation.Featherwing doc set");
            var featherDocSet = new MFDocSet("MFFeatherWing", MFSourcePath, MFFeatherwingPath, MFFeatherwingDocsOverridePath, MFFeatherGitHubUrl);
            Console.WriteLine($"Processed {featherDocSet.DriverPackages.Count} packages");
            //UpdatePeripheralDocs(featherDocSet, false);

            // WritePeripheralTables();
            // RunDriverReport();
        }


        static void WritePeripheralTables(MFDocSet docSet)
        {
            Console.Clear();
            Console.WriteLine("Driver Report");

            PeripheralDocsOutput.WritePeripheralTablesSimple(docSet.DriverPackages);
            PeripheralDocsOutput.WritePeripheralTables(docSet.DriverPackages);
        }

        //ToDo - rework in the context of doc sets
        static void RunDriverReport(MFDocSet docSet)
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

        static void UpdatePeripheralDocs(MFDocSet docSet, bool includeNamespaceInGitHubUrl = true)
        {
            var drivers = docSet.DriverPackages.SelectMany(p => p.Drivers).ToList();

            foreach(var driver in drivers)
            {
                driver.UpdateSnipSnop(docSet.GitHubUrl);
                driver.UpdateDocHeader(docSet.GitHubUrl, includeNamespaceInGitHubUrl);
            }
        }
    }
}