using System;
using System.Collections.Generic;
using System.Linq;
using Mirid.Models;
using Mirid.Output;
using Mirid.Outputs;

namespace Mirid
{
    class Program
    {
        public static string MFPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
        public static string MFFrameworksPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Libraries_and_Frameworks";
        public static string MFDocsOverridePath = "../../../../../Documentation/docfx/api-override/Meadow.Foundation";

        static List<MFDriver> drivers = new List<MFDriver>();
        static List<MFDriver> frameworks = new List<MFDriver>();

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Hello Mirid!");

            RunDriverReport();
            //UpdateProjects();
        }

        static void UpdateProjects()
        {
            Console.WriteLine("Add Author to projects");

            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(MFPeripheralsPath);

            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            foreach(var proj in driverProjectFiles)
            {
                ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "PackageProjectUrl", "http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/");
                ProjectWriter.AddUpdateProperty(proj, "PackageIconUrl", "https://github.com/WildernessLabs/Meadow.Foundation/blob/master/Source/icon.png?raw=true");
                ProjectWriter.AddUpdateProperty(proj, "RepositoryUrl", "https://github.com/WildernessLabs/Meadow.Foundation");
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

        static void RunDriverReport()
        { 
            Console.Clear();
            Console.WriteLine("Driver Report");

            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(MFPeripheralsPath);

            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);
            
            foreach(var file in driverProjectFiles)
            {
                drivers.Add(new MFDriver(file));
            }

            drivers = drivers.OrderBy(x => x.PackageName).ToList();

            CsvOutput.WriteCSV(drivers, "AllPeripherals.csv");
            CsvOutput.WriteCSV(drivers.Where(d => d.IsTested == false).ToList(), "InProgressPeripherals.csv");
            PeripheralDocsOutput.WritePeripheralTablesSimple(drivers);
            PeripheralDocsOutput.WritePeripheralTables(drivers);

            return;
            //need to standardize the folder and naming convensions for the libs and frameworks first s

            //Libraries and frameworks
            var frameworkFiles = FileCrawler.GetAllProjectsInFolders(MFFrameworksPath);
            var frameworkProjectFiles = FileCrawler.GetDriverProjects(frameworkFiles);

            foreach (var file in frameworkProjectFiles)
            {
                frameworks.Add(new MFDriver(file));
            }

            CsvOutput.WriteCSV(frameworks, "AllFrameworks.csv");
        }
    }
}