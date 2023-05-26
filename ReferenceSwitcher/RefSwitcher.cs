using System;
using System.Collections.Generic;
using System.IO;

namespace ReferenceSwitcher
{
    public partial class RefSwitcher
    {
        public static void SwitchToPublishingMode(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo> projectsToReference)
        {
            Console.WriteLine("Developer mode");

            //loop over every project we want to update
            foreach (var f in projectsToUpdate)
            {
                Console.WriteLine($"Found {f.Name}");

                //find all of the local refs in that project
                var referencedProjects = GetListOfProjectReferencesInProject(f);

                foreach (var p in referencedProjects)
                {
                    var refProjFileInfo = GetFileInfoForProjectName(p, projectsToReference);

                    if (refProjFileInfo == null)
                    {   //referenced project outside of foundation (probably core)
                        continue;
                    }

                    //time to change the file
                    ReplaceLocalRefWithNugetRef(f, p, refProjFileInfo);
                }
            }
        }

        public static void SwitchToDeveloperMode(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo> projectsToReference)
        {
            foreach (var f in projectsToUpdate)
            {
                Console.WriteLine($"Found {f.Name}");
                var packageIds = GetListOfNugetReferencesInProject(f);

                foreach (var id in packageIds)
                {
                    //get the csproj file info that maps to the referenced nuget package
                    var nugetProj = GetFileForPackageId(projectsToReference, id);

                    if (nugetProj == null)
                    {   //likely means it's referencng and external nuget package
                        continue;
                    }

                    ReplaceNugetRefWithLocalRef(f, id, nugetProj);
                }
            }
        }

        public static void ReplaceNugetRefWithLocalRef(FileInfo fileInfoToModify, string packageId, FileInfo fileInfoToReference)
        {
            if (fileInfoToModify == null)
            {
                Console.WriteLine($"{fileInfoToModify} is null");
                return;
            }

            var lines = File.ReadAllLines(fileInfoToModify.FullName);

            var newLines = new List<string>();

            foreach (var line in lines)
            {
                //skip the line we're removing
                if (line.Contains("PackageReference") && line.Contains($"\"{packageId}\""))
                {
                    var path = Path.GetRelativePath(fileInfoToModify.DirectoryName, fileInfoToReference.DirectoryName);

                    path = Path.Combine(path, Path.GetFileName(fileInfoToReference.FullName));

                    string newLine = $"    <ProjectReference Include=\"{path}";

                    newLine = newLine.Replace("/", "\\") + "\" />";
                    newLines.Add(newLine);
                }
                else
                {
                    newLines.Add(line);
                }
            }

            File.WriteAllLines(fileInfoToModify.FullName, newLines.ToArray());
        }

        public static FileInfo GetFileInfoForProjectName(string projectName, IEnumerable<FileInfo> files)
        {
            foreach (var f in files)
            {
                if (Path.GetFileName(f.FullName) == projectName)
                {
                    return f;
                }
            }
            return null;
        }

        public static void ReplaceLocalRefWithNugetRef(FileInfo fileInfoToModify, string fileName, FileInfo fileInfoToReference)
        {
            var lines = File.ReadAllLines(fileInfoToModify.FullName);

            var newLines = new List<string>();

            Console.WriteLine($"ReplaceLocalRef: {fileName}");

            foreach (var line in lines)
            {
                //skip the line we're removing
                if (line.Contains("ProjectReference") && line.Contains($"\\{fileName}\""))
                {
                    var nugetInfo = GetNugetInfoFromFileInfo(fileInfoToReference);

                    if (nugetInfo == null)   //if it's null it's missing meta data
                    {                       //which means it's not published
                        newLines.Add(line);
                    }
                    else
                    {
                        Console.WriteLine($"Nuget: {nugetInfo.Item1} Version: {nugetInfo.Item2}");

                        string newLine = $"    <PackageReference Include=\"{nugetInfo.Item1}\" Version=\"0.*\" />";

                        newLines.Add(newLine);
                    }
                }
                else
                {
                    newLines.Add(line);
                }
            }

            File.WriteAllLines(fileInfoToModify.FullName, newLines.ToArray());
        }

        static List<string> GetListOfProjectReferencesInProject(FileInfo fileInfo)
        {
            var projects = new List<string>();

            using (var sr = fileInfo.OpenText())
            {
                string line;

                while (true)
                {
                    line = sr.ReadLine();

                    if (line == null)
                        break;

                    if (line.Contains("ProjectReference"))
                    {
                        int firstQuote = line.LastIndexOf("\\");
                        int secondQuote = line.IndexOf("\"", firstQuote + 1);

                        var projectName = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);

                        //  var projectName = Path.GetFileNameWithoutExtension(projPath);

                        Console.WriteLine($"Found project ref: {projectName}");

                        projects.Add(projectName);
                    }
                }
            }

            return projects;
        }

        static List<string> GetListOfNugetReferencesInProject(FileInfo fileInfo)
        {
            var nugets = new List<string>();

            using (var sr = fileInfo.OpenText())
            {
                string line;

                while (true)
                {
                    line = sr.ReadLine();

                    if (line == null)
                        break;

                    if (line.Contains("PackageReference"))
                    {
                        int firstQuote = line.IndexOf("\"");
                        int secondQuote = line.IndexOf("\"", firstQuote + 1);

                        if (firstQuote == -1 || secondQuote == -1)
                        {
                            Console.WriteLine("malformed xml in " + fileInfo.Name);
                            break;
                        }

                        var packageName = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);

                        Console.WriteLine($"Found package: {packageName}");

                        nugets.Add(packageName);
                    }
                }
            }

            return nugets;
        }

        public static Tuple<string, string> GetNugetInfoFromFileInfo(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName);

            string packageId = string.Empty;
            string version = string.Empty;

            //we'll check for metadata that verifies if it's published
            bool isPublished = false;

            foreach (var line in lines)
            {
                if (line.Contains("PackageId"))
                {
                    var startIndex = line.IndexOf(">") + 1;
                    var endIndex = line.LastIndexOf("<");

                    if (endIndex < startIndex) continue;

                    packageId = line[startIndex..endIndex];
                    isPublished = true;
                }

                if (line.Contains("<Version>"))
                {
                    var startIndex = line.IndexOf(">") + 1;
                    var endIndex = line.LastIndexOf("<");

                    version = line[startIndex..endIndex];
                }
            }

            if (isPublished)
            {
                return new Tuple<string, string>(packageId, version);
            }
            return null;
        }

        static FileInfo GetFileForPackageId(IEnumerable<FileInfo> fileInfos, string packageId)
        {
            foreach (var f in fileInfos)
            {
                using (var sr = f.OpenText())
                {
                    string line;

                    while (true)
                    {
                        line = sr.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        if (line.Contains("PackageId") && line.Contains($">{packageId}<"))
                        {
                            return f;
                        }
                    }
                }
            }

            return null;
        }
    }
}