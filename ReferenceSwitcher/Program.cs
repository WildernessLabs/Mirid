using System;
using System.Collections.Generic;
using System.IO;

namespace ReferenceSwitcher
{
    enum Projects
    {
        All,
        Drivers,
        Samples
    }

    class Program
    {
        //ToDo update to a command line arg
        public static string MeadowContractsSourcePath = "../../../../../Meadow.Contracts/Source/";
        public static string MeadowModbusSourcePath = "../../../../../Meadow.Modbus/src/";
        public static string MeadowLoggingSourcePath = "../../../../../Meadow.Logging/Source/";
        public static string MeadowUnitsSourcePath = "../../../../../Meadow.Units/Source/";
        public static string MeadowMQTTSourcePath = "../../../../../MQTTnet/Source/MQTTnet/";

        public static string MeadowCoreSourcePath = "../../../../../Meadow.Core/Source/";
        public static string MeadowFoundationCoreSourcePath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Core";
        public static string MeadowFoundationSourcePath = "../../../../../Meadow.Foundation/Source/";
        public static string MeadowFoundationPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";

        public static string MeadowFoundationGrovePath = "../../../../../Meadow.Foundation.Grove/Source/";
        public static string MeadowFoundationFeatherwingPath = "../../../../../Meadow.Foundation.Featherwings/Source/";
        public static string MeadowFoundationMikroBusPath = "../../../../../Meadow.Foundation.mikroBUS/Source/";

        public static string MeadowProjectLabPath = "../../../../../Meadow.ProjectLab/Source/";


        // zero dependancy nugets
        static FileInfo[]? MeadowMQTTProjects;
        static FileInfo[]? MeadowUnitsProjects;
        static FileInfo[]? MeadowLoggingProjects;
        
        static FileInfo[]? MeadowContractsProjects;

        static FileInfo[]? MeadowModbusProjects;
        static FileInfo[]? MeadowCoreProjects;

        static FileInfo[]? MeadowFoundationDriverProjects;
        static FileInfo[]? MeadowFoundationSampleProjects;
        static FileInfo[]? MeadowFoundationCoreProjects;
        static FileInfo[]? MeadowFoundationGroveProjects;
        static FileInfo[]? MeadowFoundationMikroBusProjects;
        static FileInfo[]? MeadowFoundationFeatherwingsProjects;

        static FileInfo[]? MeadowProjectLabProjects;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Meadow developers!");

            LoadProjects();


            //toggle methods below for various repos

            //SwitchMeadowContracts(publish: true);

            //SwitchMeadowModbus(publish: true);

            //SwitchMeadowCore(publish: true);
            

            //SwitchMeadowFoundationCore(publish: true);

            SwitchMeadowFoundation(publish: false);

            //SwitchMeadowFoundationGrove(publish: true);

            //SwitchMeadowFoundationFeatherwings(publish: true);

            //SwitchMeadowFoundationMikroBus(publish: true);

            //SwitchMeadowProjectLab(publish: false);
        }

        static void LoadProjects()
        {
            MeadowMQTTProjects = GetCsProjFiles(MeadowMQTTSourcePath);

            MeadowUnitsProjects = GetCsProjFiles(MeadowUnitsSourcePath);
            MeadowLoggingProjects = GetCsProjFiles(MeadowLoggingSourcePath);
            MeadowContractsProjects = GetCsProjFiles(MeadowContractsSourcePath);
            MeadowModbusProjects = GetCsProjFiles(MeadowModbusSourcePath);

            MeadowCoreProjects = GetCsProjFiles(MeadowCoreSourcePath);

            MeadowFoundationDriverProjects = GetCsProjFiles(MeadowFoundationSourcePath, Projects.Drivers);
            MeadowFoundationSampleProjects = GetCsProjFiles(MeadowFoundationSourcePath, Projects.Samples);

            MeadowFoundationCoreProjects = GetCsProjFiles(MeadowFoundationCoreSourcePath, Projects.Drivers);

            MeadowFoundationGroveProjects = GetCsProjFiles(MeadowFoundationGrovePath, Projects.All);
            MeadowFoundationMikroBusProjects = GetCsProjFiles(MeadowFoundationMikroBusPath, Projects.All);
            MeadowFoundationFeatherwingsProjects = GetCsProjFiles(MeadowFoundationFeatherwingPath, Projects.All);

            MeadowProjectLabProjects = GetCsProjFiles(MeadowProjectLabPath, Projects.All);
        }

        static void SwitchRepo(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo>[] projectsToReference, bool publish)
        {
            foreach(var collection in projectsToReference)
            {
                if (publish)
                {
                    SwitchToPublishingMode(projectsToUpdate, collection);
                }
                else
                {
                    SwitchToDeveloperMode(projectsToUpdate, collection);
                }
            }
        }

        static void SwitchMeadowModbus(bool publish)
        {
            SwitchRepo(MeadowModbusProjects,
                new IEnumerable<FileInfo>[] { MeadowLoggingProjects, MeadowContractsProjects },
                publish);
        }

        static void SwitchMeadowContracts(bool publish)
        {
            SwitchRepo(MeadowModbusProjects,
                new IEnumerable<FileInfo>[] { MeadowLoggingProjects, MeadowUnitsProjects },
                publish);
        }

        static void SwitchMeadowCore(bool publish)
        {
            SwitchRepo(MeadowCoreProjects,
                new IEnumerable<FileInfo>[] { MeadowMQTTProjects, MeadowContractsProjects },
                publish);
        }

        static void SwitchMeadowFoundationCore(bool publish)
        {
            SwitchRepo(MeadowFoundationCoreProjects,
                new IEnumerable<FileInfo>[] { MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundation(bool publish)
        {
            SwitchRepo(MeadowFoundationDriverProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, 
                                              MeadowFoundationCoreProjects, 
                                              MeadowModbusProjects },
                publish);

            SwitchRepo(MeadowFoundationSampleProjects,
                new IEnumerable<FileInfo>[] { MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundationFeatherwings(bool publish)
        {
            SwitchRepo(MeadowFoundationFeatherwingsProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundationMikroBus(bool publish)
        {
            SwitchRepo(MeadowFoundationMikroBusProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, MeadowCoreProjects },
                publish);
        }

        static void SwitchMeadowFoundationGrove(bool publish)
        {
            SwitchRepo(MeadowFoundationGroveProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, MeadowCoreProjects }, 
                publish);
        }

        static void SwitchMeadowProjectLab(bool publish)
        {
            SwitchRepo(MeadowProjectLabProjects,
                new IEnumerable<FileInfo>[] { MeadowFoundationDriverProjects, 
                                              MeadowCoreProjects, 
                                              MeadowModbusProjects},
                publish);
        }

        static void SwitchToPublishingMode(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo> projectsToReference)
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

        static void SwitchToDeveloperMode(IEnumerable<FileInfo> projectsToUpdate, IEnumerable<FileInfo> projectsToReference)
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

        static FileInfo GetFileInfoForProjectName(string projectName, IEnumerable<FileInfo> files)
        {
            foreach (var f in files)
            {
                var name = Path.GetFileName(f.FullName);

                if (Path.GetFileName(f.FullName) == projectName)
                {
                    return f;
                }
            }

            return null;
        }

        static void ReplaceLocalRefWithNugetRef(FileInfo fileInfoToModify, string fileName, FileInfo fileInfoToReference)
        {
            var lines = File.ReadAllLines(fileInfoToModify.FullName);

            var newLines = new List<string>();

            Console.WriteLine($"ReplaceLocalRef: {fileName}");

            foreach (var line in lines)
            {
                //skip the line we're removing
                if (line.Contains("ProjectReference") && line.Contains(fileName))
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

        static void ReplaceNugetRefWithLocalRef(FileInfo fileInfoToModify, string packageId, FileInfo fileInfoToReference)
        {
            var lines = File.ReadAllLines(fileInfoToModify.FullName);

            var newLines = new List<string>();

            foreach (var line in lines)
            {
                //skip the line we're removing
                if (line.Contains("PackageReference") && line.Contains(packageId))
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

        static FileInfo[] GetCsProjFiles(string path, Projects projectsType = Projects.Drivers)
        {
            var files = (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);

            var filteredFiles = new List<FileInfo>();

            foreach(var file in files) 
            {
                if(projectsType == Projects.Drivers &&
                    (file.DirectoryName.Contains("Sample") || file.DirectoryName.Contains("sample")))
                {
                    continue;
                }
                if (projectsType == Projects.Samples &&
                    (!file.DirectoryName.Contains("Sample") && !file.DirectoryName.Contains("sample")))
                {
                    continue;
                }
                filteredFiles.Add(file);
            }

            return filteredFiles.ToArray();
        }

        static Tuple<string, string> GetNugetInfoFromFileInfo(FileInfo file)
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

                    packageId = line.Substring(startIndex, endIndex - startIndex);
                    isPublished = true;
                }

                if (line.Contains("<Version>"))
                {
                    var startIndex = line.IndexOf(">") + 1;
                    var endIndex = line.LastIndexOf("<");

                    version = line.Substring(startIndex, endIndex - startIndex);
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

                        if (line.Contains("PackageId") && line.Contains(packageId))
                        {
                            return f;
                        }
                    }
                }
            }

            return null;
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

                        if(firstQuote == -1 || secondQuote == -1)
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
    }
}