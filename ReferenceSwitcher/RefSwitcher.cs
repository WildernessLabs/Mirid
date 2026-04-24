using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
                Console.WriteLine($"fileInfoToModify is null (packageId: {packageId})");
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
            XDocument doc;
            try { doc = XDocument.Load(fileInfo.FullName); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing {fileInfo.Name}: {ex.Message}");
                return new List<string>();
            }

            return doc.Descendants("ProjectReference")
                .Select(r => Path.GetFileName(r.Attribute("Include")?.Value ?? string.Empty))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        static List<string> GetListOfNugetReferencesInProject(FileInfo fileInfo)
        {
            XDocument doc;
            try { doc = XDocument.Load(fileInfo.FullName); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing {fileInfo.Name}: {ex.Message}");
                return new List<string>();
            }

            var nugets = doc.Descendants("PackageReference")
                .Select(r => r.Attribute("Include")?.Value ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            foreach (var n in nugets)
                Console.WriteLine($"Found package: {n}");

            return nugets;
        }

        public static Tuple<string, string>? GetNugetInfoFromFileInfo(FileInfo file)
        {
            XDocument doc;
            try { doc = XDocument.Load(file.FullName); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing {file.Name}: {ex.Message}");
                return null;
            }

            var packageId = doc.Descendants("PackageId").FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(packageId))
                return null;

            var version = doc.Descendants("Version").FirstOrDefault()?.Value ?? string.Empty;

            return new Tuple<string, string>(packageId, version);
        }

        static FileInfo? GetFileForPackageId(IEnumerable<FileInfo> fileInfos, string packageId)
        {
            foreach (var f in fileInfos)
            {
                XDocument doc;
                try { doc = XDocument.Load(f.FullName); }
                catch { continue; }

                if (doc.Descendants("PackageId").Any(e => e.Value == packageId))
                    return f;
            }

            return null;
        }
    }
}