using System;
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
        public static string MFFullPath = "../../../../../Meadow.Foundation/Source/";
        public static string MFPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        public static string MFFrameworksPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Libraries_and_Frameworks";
        public static string MFDocsOverridePath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";

        public static string MCFullPath = "../../../../../Meadow.Core/Source/";

        static List<MFPackage> driverNugets = new List<MFPackage>();
        static List<MFPackage> frameworkNugets = new List<MFPackage>();

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            //  UpdatePeripheralDocs();

            WritePeripheralTables();

            RunDriverReport();
        //    UpdateProjects();

         //   UpdateSamples();
        }

        static void UpdateSamples()
        {
            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(MFFullPath);

            var driverSamples = FileCrawler.GetSampleProjects(projectFiles);

            var projectMCF7 = new FileInfo(Path.Combine(MCFullPath, "Meadow.F7/Meadow.F7.csproj"));
            

            foreach (var proj in driverSamples)
            {
                //   ProjectWriter.AddReference(proj, projectMCF7);
                ProjectWriter.RemoveReference(proj, projectMCF7);
                ProjectWriter.AddNuget(proj, "Meadow.F7");
                ProjectWriter.AddUpdateProperty(proj, "CopyLocalLockFileAssemblies", "true");
                ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "RepositoryUrl", "https://github.com/WildernessLabs/Meadow.Foundation");
            }
        }

        static void UpdateProjects()
        {
            Console.WriteLine("Update project metadata");

            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(MFPeripheralsPath);

            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            foreach(var proj in driverProjectFiles)
            {
                ProjectWriter.AddReference(proj, $"    <None Include=\"..\\..\\..\\..\\icon.png\" Pack=\"true\" PackagePath=\"\"/>");
                ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "PackageProjectUrl", "http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/");
                ProjectWriter.AddUpdateProperty(proj, "PackageIcon", "icon.png");
                ProjectWriter.DeleteProperty(proj, "PackageIconUrl");
            }

            projectFiles = FileCrawler.GetAllProjectsInFolders(MFFrameworksPath);

            var driverFrameworkFiles = FileCrawler.GetDriverProjects(projectFiles);

            foreach (var proj in driverFrameworkFiles)
            {
                ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "PackageProjectUrl", "http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/");
                ProjectWriter.AddUpdateProperty(proj, "PackageIconUrl", "https://github.com/WildernessLabs/Meadow.Foundation/blob/master/Source/icon.png?raw=true");
                ProjectWriter.AddUpdateProperty(proj, "RepositoryUrl", "https://github.com/WildernessLabs/Meadow.Foundation");
            }
        }

        static void ReadPackageData()
        {
            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(MFPeripheralsPath);
            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            foreach (var file in driverProjectFiles)
            {
                driverNugets.Add(new MFPackage(file));
            }

            driverNugets = driverNugets.OrderBy(x => x.PackageName).ToList();
        }

        static void WritePeripheralTables()
        {
            Console.Clear();
            Console.WriteLine("Driver Report");

            ReadPackageData();

            PeripheralDocsOutput.WritePeripheralTablesSimple(driverNugets);
            PeripheralDocsOutput.WritePeripheralTables(driverNugets);
        }

        static void RunDriverReport()
        { 
            Console.Clear();
            Console.WriteLine("Driver Report");

            ReadPackageData();

            CsvOutput.WritePackagesCSV(driverNugets, "AllPeripherals.csv");
            CsvOutput.WriteDriversCSV(driverNugets, "AllDrivers.csv");

            CsvOutput.WritePackagesCSV(driverNugets.Where(d => d.IsPublished == false).ToList(), "InProgressPeripherals.csv");

            return;
            //need to standardize the folder and naming convensions for the libs and frameworks first s

            //Libraries and frameworks
            var frameworkFiles = FileCrawler.GetAllProjectsInFolders(MFFrameworksPath);
            var frameworkProjectFiles = FileCrawler.GetDriverProjects(frameworkFiles);

            foreach (var file in frameworkProjectFiles)
            {
                frameworkNugets.Add(new MFPackage(file));
            }

            CsvOutput.WritePackagesCSV(frameworkNugets, "AllFrameworks.csv");
        }

        static void UpdatePeripheralDocs()
        {
            ReadPackageData();

            //loop over all drivers .... so we need to extract every driver 
            var drivers = new List<MFDriver>();

            foreach (var package in driverNugets)
            {
                foreach (var d in package.Drivers)
                {
                    drivers.Add(d);
                }
            }

            foreach(var driver in drivers)
            {
                driver.UpdateSnipSnop();
                driver.UpdateDocHeader();
            }
        }
    }
}