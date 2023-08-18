namespace Mirid
{
    public static class FileCrawler
    {
        public static FileInfo[] GetAllProjectsInFolders(string path, bool filter = true)
        {
            //check if path exists first
            if (Directory.Exists(path))
            {
                var files = GetCsProjFiles(path);

                if (filter)
                {
                    files = files.Where(f => !f.FullName.Contains("Test"))
                                 .Where(f => !f.FullName.Contains("Utilities"))
                                 .ToArray();
                }
                return files;
            }
            else
            {
                return Array.Empty<FileInfo>();
            }
        }

        public static FileInfo GetFileInfo(string path)
        {
            return new FileInfo(path);
        }

        static FileInfo[] GetCsProjFiles(string path)
        {
            return (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);
        }

        public static List<FileInfo> GetSampleProjects(FileInfo[] projects)
        {
            var samples = new List<FileInfo>();

            foreach (var file in projects)
            {
                if (file.Name.Contains("Sample"))
                {
                    samples.Add(file);
                }
            }

            return samples;
        }

        public static List<FileInfo> GetDriverProjects(FileInfo[] projects)
        {
            var drivers = new List<FileInfo>();

            foreach (var file in projects)
            {
                if (file.Name.Contains("Sample") == false)
                {
                    drivers.Add(file);
                }
            }

            return drivers;
        }

    }
}
