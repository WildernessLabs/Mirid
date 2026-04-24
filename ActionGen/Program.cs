using MeadowRepos;
using ReferenceSwitcher;
using System.Xml.Linq;

namespace ActionGen
{
    class Program
    {
        //ToDo update to a command line arg
        public static string MCSourcePath = "../../../../../Meadow.Core/source/";
        public static string MFSourcePath = "../../../../../Meadow.Foundation/Source/";
        public static string MFPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";

        static void Main(string[] args)
        {
            //load all Meadow Foundation Projects
            var projectFiles = RepoLoader.GetCsProjFiles(MFSourcePath, ProjectType.All);

            //filter out samples (so only drivers) - confirmed 110 for RC2-1
            var drivers = GetDriverProjects(projectFiles);

            //count references to other projects
            var metadata = GetProjectsMetadata(drivers);

            //now split into two collections ... 0 refs and 1+ refs
            var level1 = new Dictionary<string, FileInfo>();
            var level2 = new Dictionary<string, FileInfo>();

            for (int i = 0; i < metadata.Length; i++)
            {
                if (metadata[i].refCount == 0)
                {
                    level1.Add(metadata[i].packageId, drivers[i]);
                }
                else
                {
                    level2.Add(metadata[i].packageId, drivers[i]);
                }
            }

            Console.WriteLine($"{level1.Count} drivers have 0 local refs");
            Console.WriteLine($"{level2.Count} drivers have 1 local refs");

            WorkflowWriter writer = new();

            writer.WriteWorkflow(level1, "nuget-level1.yml");
            writer.WriteWorkflow(level2, "nuget-level2.yml");
        }

        static FileInfo[] GetDriverProjects(FileInfo[] projectFiles)
        {
            var drivers = new List<FileInfo>();

            foreach (var project in projectFiles)
            {
                if (project.Directory.Name == "Driver")
                {
                    drivers.Add(project);
                }
            }

            return drivers.ToArray();
        }

        static (int refCount, string packageId) GetMetaData(FileInfo projectFile)
        {
            if (!File.Exists(projectFile.FullName))
                return (0, string.Empty);

            XDocument doc;
            try { doc = XDocument.Load(projectFile.FullName); }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing {projectFile.Name}: {ex.Message}");
                return (0, string.Empty);
            }

            int count = doc.Descendants("ProjectReference")
                .Count(r => r.Attribute("Include")?.Value?.Contains("Meadow.Foundation.Core") != true);

            var packageId = doc.Descendants("PackageId").FirstOrDefault()?.Value ?? string.Empty;

            return (count, packageId);
        }

        static (int refCount, string packageId)[] GetProjectsMetadata(FileInfo[] projectFiles)
        {
            var data = new (int refCount, string packageId)[projectFiles.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = GetMetaData(projectFiles[i]);
            }
            return data;
        }
    }
}