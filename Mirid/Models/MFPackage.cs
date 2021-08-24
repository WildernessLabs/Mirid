using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFPackage
    {
        [Ignore]
        public string SimpleName => PackageName.Split(".").LastOrDefault();

        //indexes for writing CSV
        [Index(0)]
        public string PackageName => NugetProject.PackageId;
        [Index(1)]
        public int NumberOfDrivers => Drivers.Count();
        [Index(2)]
        public bool IsTested => NugetProject?.GeneratePackageOnBuild == "true";
        [Index(3)]
        public bool HasCompleteMetaData => NugetProject?.IsMetadataComplete() ?? false;
        [Index(4)]
        public bool HasDataSheet => Assets?.HasDataSheet ?? false;
        [Index(5)]
        public int NumberOfSamples => Assets?.NumberOfSamples ?? 0;
        [Index(6)]
        public bool HasTestSuite => false;
        [Index(7)]
        public bool HasDocOverride => Documentation?.HasOverride ?? false;
        [Index(8)]
        public bool HasFritzing => Documentation?.HasFritzing ?? false;
        [Index(9)]
        public bool HasCodeExample => Documentation?.HasCodeExample ?? false;
        [Index(10)]
        public bool HasWiringExample => Documentation?.HasWiringExample ?? false;
        [Index(11)]
        public bool HasPurchasing => Documentation?.HasPurchasing ?? false;
        [Index(12)]
        public bool HasSnipSnop => Drivers[0]?.HasSnipSnop ?? false;

        [Ignore]
        public MFPackageProject NugetProject { get; private set; }
        [Ignore]
        public List<MFDriver> Drivers { get; private set; } = new List<MFDriver>();
        [Ignore]
        public MFDriverAssets Assets { get; private set; }
        [Ignore]
        public MFDriverDocumentation Documentation { get; private set; }


        [Ignore]
        public string Namespace => Drivers.First().Namespace;

        [Ignore] //ToDo LINQ expression from driver list - count of drivers that have samples 
        public List<string> Samples { get; private set; } = new List<string>();

        [Ignore]
        public string Description => NugetProject?.Description ?? string.Empty;


        public MFPackage(FileInfo driverProjectFile)
        {
            if (File.Exists(driverProjectFile.FullName) == false)
            {
                throw new FileNotFoundException($"Driver project not found {driverProjectFile.FullName}");
            }

            //load nuget project (for metadata)
            NugetProject = new MFPackageProject(driverProjectFile);

            //Load assets
            var parentDir = driverProjectFile.Directory.Parent.Parent;
            Assets = new MFDriverAssets(parentDir);

            //load driver code
            var driverDir = driverProjectFile.Directory.GetDirectories("Drivers").FirstOrDefault();

            if (driverDir != null)
            {
                var files = driverDir.GetFiles();
                var filesSorted = files.OrderBy(f => f.Name);

                foreach (var file in filesSorted)
                {   //trickery - removing the extension hacks off the end of the package name 
                    var name = Path.GetFileNameWithoutExtension(driverProjectFile.Name) + "." + Path.GetFileNameWithoutExtension(file.Name) + "_Sample";
                    Drivers.Add(new MFDriver(file.FullName, Assets.GetSampleForName(name)));
                }
            }
            else
            {
                var fileName = GetSimpleName(driverProjectFile.Name) + ".cs";
                var file = Path.Combine(driverProjectFile.DirectoryName, fileName);

                var fullName = Path.GetFileNameWithoutExtension(driverProjectFile.Name);
                Drivers.Add(new MFDriver(file, Assets.GetSampleForName(fullName + "_Sample")));
            }

            //Load documentation
            //ToDo
            //Documentation = new MFDriverDocumentation(this, Program.MFDocsOverridePath);

            
        }

        static string GetSimpleName(string Name)
        {
            var nameChunks = Name.Split('.');

            return nameChunks[nameChunks.Length - 2];
        }
    }
}
