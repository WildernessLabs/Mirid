using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    //ToDo re-eval this class name - DriverSet? ... 
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
        public bool IsPublished => NugetProject?.GeneratePackageOnBuild == "true";
        [Index(3)]
        public bool HasCompleteMetaData => NugetProject?.IsMetadataComplete() ?? false;
        [Index(4)]
        public bool HasDataSheet => Assets?.HasDataSheet ?? false;
        [Index(5)]
        public int NumberOfSamples => Assets?.NumberOfSamples ?? 0;
        [Index(6)]
        public bool HasTestSuite => false;


        [Ignore]
        public MFPackageProject NugetProject { get; protected set; }
        [Ignore]
        public List<MFDriver> Drivers { get; protected set; } = new List<MFDriver>();
        [Ignore]
        public MFDriverAssets Assets { get; protected set; }
        [Ignore]
        public MFDriverDocumentation Documentation { get; protected set; }


        [Ignore]
        public string Namespace => Drivers.First().Namespace;

        [Ignore] //ToDo LINQ expression from driver list - count of drivers that have samples 
        public List<string> Samples { get; protected set; } = new List<string>();

        [Ignore]
        public string Description => NugetProject?.Description ?? string.Empty;


        public MFPackage(FileInfo driverProjectFile, string docsOverridePath)
        {
            LoadDriverResouces(driverProjectFile, docsOverridePath);
        }

        protected virtual void LoadDriverResouces(FileInfo driverProjectFile, string docsOverridePath)
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
            var driverDir = driverProjectFile.Directory.GetDirectories("Drivers").FirstOrDefault();

            //if the package contains multiple drivers
            if (driverDir != null)
            {
                var files = driverDir.GetFiles();
                var filesSorted = files.OrderBy(f => f.Name);

                foreach (var file in filesSorted)
                {   //trickery - removing the extension hacks off the end of the package name 
                    var name = Path.GetFileNameWithoutExtension(file.Name) + "_Sample";
                    Drivers.Add(new MFDriver(this, file.FullName, Assets.GetSampleForName(name), docsOverridePath));
                }
            }
            //if the package only contains one driver
            else
            {
                var fileName = GetSimpleName(driverProjectFile.Name) + ".cs";
                var file = Path.Combine(driverProjectFile.DirectoryName, fileName);

                var fullName = Path.GetFileNameWithoutExtension(driverProjectFile.Name);
                Drivers.Add(new MFDriver(this, 
                                         file, 
                                         Assets.GetSampleForName(Path.GetFileNameWithoutExtension(file) + "_Sample"),
                                         docsOverridePath));
            }
        }

        protected static string GetSimpleName(string Name)
        {
            var nameChunks = Name.Split('.');

            return nameChunks[nameChunks.Length - 2];
        }
    }
}