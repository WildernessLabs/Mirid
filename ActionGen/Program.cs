namespace ActionGen
{
    class Program
    {
        //ToDo update to a command line arg
        public static string MCSourcePath = "../../../../../Meadow.Core/Source/";
        public static string MFSourcePath = "../../../../../Meadow.Foundation/Source/";
        public static string MFPeripheralsPath = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";

        static void Main(string[] args)
        {
            //load all Meadow Foundation Projects
            var projectFiles = GetCsProjFiles(MFSourcePath);

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

        static FileInfo[] GetCsProjFiles(string path)
        {
            return (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);
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
            string text = string.Empty;

            //load file
            if (File.Exists(projectFile.FullName))
            {
                text = File.ReadAllText(projectFile.FullName);

                if (string.IsNullOrWhiteSpace(text))
                {
                    return (0, string.Empty);
                }
            }

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            int count = 0;
            string packageId = string.Empty;

            foreach (var line in lines)
            {
                //does it reference another peripheral or a library
                if (line.Contains("ProjectReference") &&
                    line.Contains("Meadow.Foundation.Core") == false)
                {
                    count++;
                }
                else if (line.Contains("<PackageId>"))
                {
                    var index = line.IndexOf('>') + 1;
                    packageId = line.Substring(index, line.IndexOf('<', index) - index);
                }
            }

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