using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFDriverSet
    {
        public string SetName { get; private set; }

        public string MeadowFoundationSourcePath { get; private set; }

        //root folder of drivers to crawl
        public string DriverSetSourcePath { get; private set; }


        //docfx API override folder for drivers
        public string DocsOverridePath { get; private set; }

        public string GitHubUrl { get; private set; }


        //list of driver packages - all-the-things: driver, sample, doc, etc.
        public List<MFPackage> DriverPackages { get; protected set; } = new List<MFPackage>();

        public MFDriverSet(string name,
            string MFSourcePath,
            string driverSourcePath,
            string docsOverridePath,
            string githubUrl)
        {
            SetName = name;
            MeadowFoundationSourcePath = MFSourcePath;
            DriverSetSourcePath = driverSourcePath;
            DocsOverridePath = docsOverridePath;
            GitHubUrl = githubUrl;

            //should auto-process here
            ReadPackageData(DriverSetSourcePath, DocsOverridePath);
        }

        static string FindIconPath(string startPath)
        {
            var dir = new DirectoryInfo(startPath);
            while (dir != null)
            {
                var icon = Path.Combine(dir.FullName, "icon.png");
                if (File.Exists(icon)) return icon;
                dir = dir.Parent;
            }
            return null;
        }

        protected virtual void ReadPackageData(string peripheralsPath, string docsOverridePath)
        {
            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(peripheralsPath, true);
            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            foreach (var file in driverProjectFiles)
            {
                DriverPackages.Add(new MFPackage(file, docsOverridePath));
            }

            DriverPackages = DriverPackages.OrderBy(x => x.PackageName).ToList();
        }

        //this entire method needs to be dynamic
        //pass in what to process and what to apply
        public void UpdateProjectMetadata()
        {
            Console.WriteLine("Update project metadata");

            //Drivers
            var projectFiles = FileCrawler.GetAllProjectsInFolders(DriverSetSourcePath);

            var driverProjectFiles = FileCrawler.GetDriverProjects(projectFiles);

            //to process samples
            //var driverSamples = FileCrawler.GetSampleProjects(projectFiles);

            var iconAbsPath = FindIconPath(DriverSetSourcePath);
            var repoUrl = GitHubUrl.Contains("/tree/")
                ? GitHubUrl.Substring(0, GitHubUrl.IndexOf("/tree/"))
                : GitHubUrl.TrimEnd('/');

            foreach (var proj in driverProjectFiles)
            {
                ProjectWriter.AddUpdateProperty(proj, "Nullable", "enable");
                ProjectWriter.AddUpdateProperty(proj, "LangVersion", "12.0");
                ProjectWriter.AddUpdateProperty(proj, "PackageLicenseExpression", "Apache-2.0");
                ProjectWriter.AddUpdateProperty(proj, "GenerateDocumentationFile", "true");
                ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "PackageProjectUrl", "http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/");
                ProjectWriter.AddUpdateProperty(proj, "PackageIcon", "icon.png");
                ProjectWriter.AddUpdateProperty(proj, "RepositoryUrl", repoUrl);
                ProjectWriter.DeleteProperty(proj, "PackageIconUrl");

                if (iconAbsPath != null)
                {
                    var iconRelPath = Path.GetRelativePath(proj.DirectoryName, iconAbsPath);
                    ProjectWriter.AddOrReplaceReference(proj, $"<None Include=\"{iconRelPath}\" Pack=\"true\" PackagePath=\"\"/>", "icon.png");
                }
            }
        }
    }
}
