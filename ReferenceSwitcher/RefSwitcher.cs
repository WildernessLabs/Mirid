﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReferenceSwitcher
{
    public partial class RefSwitcher
    {
        public static string DirectoryPropsFileName = "Directory.Packages.props";
        public static string MeadowPropsFileName = "Meadow.Packages.props";

        public static void UpdatePackageProps(string meadowPropsPath, string rootFolder)
        {
            //find contracts package props in Meadow.Contracts sub folder
            var contractsFolder = Path.Combine(rootFolder, "Meadow.Contracts");

            //search for "Project.Packages.props" in contracts folder
            var contractsPackageProps = new List<FileInfo>();
            var files = Directory.GetFiles(contractsFolder, DirectoryPropsFileName, SearchOption.AllDirectories);

            var propsFile = files.FirstOrDefault();

            if (propsFile == null)
            {
                Console.WriteLine($"Could not find {DirectoryPropsFileName} in {contractsFolder}");
                return;
            }

            var lines = File.ReadAllLines(propsFile).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("</Project>"))
                {
                    lines.Insert(i, $"  <Import Project=\"{MeadowPropsFileName}\" />");
                    File.WriteAllText(propsFile, string.Join(Environment.NewLine, lines));
                    break;
                }
            }

            //now find every instance of Directory.Packages.props in rootFolder other than contractsPacakgeProps
            var allFiles = Directory.GetFiles(rootFolder, DirectoryPropsFileName, SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                if (file != propsFile)
                {
                    File.Copy(propsFile, file, true);
                }
                var path = Path.GetDirectoryName(file);

                File.Copy(meadowPropsPath, Path.Combine(path!, MeadowPropsFileName), true);
            }
        }

        public static string GenerateMeadowPackageProps(IEnumerable<FileInfo> projects, string nugetVersion, string folder)
        {
            //create a new file
            StringBuilder output = new();
            var fullPath = Path.Combine(folder, "Meadow.Packages.props");

            // Keep track of package names to detect duplicates
            HashSet<string> packageNames = new(StringComparer.OrdinalIgnoreCase);

            //write header
            output.AppendLine($"<Project>");
            output.AppendLine($"  <ItemGroup>");

            foreach (var proj in projects)
            {
                //crawl all projects ... add dependency with version
                var info = GetNugetInfoFromFileInfo(proj);

                if (info != null)
                {
                    var packageName = info.Item1;

                    if (!packageNames.Add(packageName))
                    {
                        throw new InvalidOperationException($"Duplicate package name detected: {packageName}");
                    }

                    output.AppendLine($"    <PackageVersion Include=\"{packageName}\" Version=\"{nugetVersion}\" />");
                }
            }

            //close it out
            output.AppendLine($"  </ItemGroup>");
            output.AppendLine($"</Project>");

            //write file
            File.WriteAllText(fullPath, output.ToString());

            return fullPath;
        }

        public static IEnumerable<FileInfo> RemoveNonPackageProjects(IEnumerable<FileInfo> projectsToUpdate)
        {
            var filteredProjects = new List<FileInfo>();
            foreach (var proj in projectsToUpdate)
            {
                if (proj.Name.Contains("Sample", StringComparison.OrdinalIgnoreCase) ||
                    proj.Name.Contains("Test", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                filteredProjects.Add(proj);

            }
            return filteredProjects;
        }

        public static IEnumerable<FileInfo> SortProjectsByLocalDependencies(IEnumerable<FileInfo> projectsToUpdate)
        {
            var unsortedList = new List<FileInfo>();
            var sortedList = new List<FileInfo>();

            unsortedList.AddRange(projectsToUpdate);

            foreach (var file in unsortedList)
            {
                var referencedProjects = GetListOfProjectReferencesInProject(file);
                //if it has no local dependencies, add it to the sorted list
                if (referencedProjects.Count == 0)
                {
                    sortedList.Add(file);
                }
            }

            //remove all items from unsorted list that are in the sorted list
            unsortedList.RemoveAll(f => sortedList.Contains(f));

            while (unsortedList.Count > 0)
            {
                //get the first project in the unsorted list
                var file = unsortedList[0];
                unsortedList.Remove(file);

                var referencedProjects = GetListOfProjectReferencesInProject(file);

                //check to see if all referenced projects are in the sorted list ... if not ... add it to the unsorted list and move on
                bool allRefsSorted = true;
                foreach (var p in referencedProjects)
                {
                    var refProjFileInfo = GetFileInfoForProjectName(p, projectsToUpdate);
                    if (refProjFileInfo == null)
                    {   //external ref, ignore
                        continue;
                    }
                    if (sortedList.Contains(refProjFileInfo) == false)
                    {
                        allRefsSorted = false;

                        break;
                    }
                }

                if (allRefsSorted)
                {
                    //all refs are sorted, add it to the sorted list
                    sortedList.Add(file);
                }
                else
                {
                    unsortedList.Add(file);
                }
            }

            return sortedList;
        }

        public static void SwitchToPublishingMode(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo> projectsToReference, string? version)
        {
            //loop over every project we want to update
            foreach (var f in projectsToUpdate)
            {
                //Console.WriteLine($"Found {f.Name}");

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
                    ReplaceLocalRefWithNugetRef(f, p, refProjFileInfo, version);
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

        public static void ReplaceLocalRefWithNugetRef(FileInfo fileInfoToModify, string fileName, FileInfo fileInfoToReference, string? version)
        {
            var lines = File.ReadAllLines(fileInfoToModify.FullName);

            var newLines = new List<string>();

            string newLine;
            bool skipUntilClose = false;

            foreach (var line in lines)
            {
                if (skipUntilClose)
                {
                    if (line.Contains("</ProjectReference>"))
                    {
                        skipUntilClose = false;
                    }
                    continue;
                }

                //skip the line we're removing
                if (line.Contains("ProjectReference") && line.Contains($"\\{fileName}\""))
                {
                    var nugetInfo = GetNugetInfoFromFileInfo(fileInfoToReference);

                    if (nugetInfo == null)  // if it's null it's missing meta data
                    {                       // which means it's not published
                        newLines.Add(line);
                    }
                    else
                    {
                        //Console.WriteLine($"Nuget: {nugetInfo.Item1} Version: {nugetInfo.Item2}");

                        if (version != null)
                        {
                            newLine = $"    <PackageReference Include=\"{nugetInfo.Item1}\" Version=\"{version}\" />";
                        }
                        else
                        {
                            newLine = $"    <PackageReference Include=\"{nugetInfo.Item1}\" />";
                        }

                        newLines.Add(newLine);

                        if (line.Contains("/>"))
                        {
                            skipUntilClose = false;
                        }
                        else
                        {
                            skipUntilClose = true;
                        }
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
                    {
                        break;
                    }

                    if (line.Contains("</ProjectReference"))
                    {
                        continue;
                    }

                    if (line.Contains("ProjectReference"))
                    {
                        int firstQuote = line.LastIndexOf("\\");
                        int secondQuote = line.IndexOf("\"", firstQuote + 1);

                        var projectName = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);

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

        public static Tuple<string, string>? GetNugetInfoFromFileInfo(FileInfo file)
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

        static FileInfo? GetFileForPackageId(IEnumerable<FileInfo> fileInfos, string packageId)
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