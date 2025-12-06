namespace Mirid
{
    public class FileCrawler : IFileCrawler
    {
        private readonly IFileSystem _fileSystem;
        private static readonly IFileSystem _defaultFileSystem = new FileSystem();

        public FileCrawler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        FileInfo[] IFileCrawler.GetAllProjectsInFolders(string path, bool filter)
        {
            //check if path exists first
            if (_fileSystem.DirectoryExists(path))
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

        FileInfo IFileCrawler.GetFileInfo(string path)
        {
            return new FileInfo(path);
        }

        FileInfo[] GetCsProjFiles(string path)
        {
            return _fileSystem.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
        }

        List<FileInfo> IFileCrawler.GetSampleProjects(FileInfo[] projects)
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

        List<FileInfo> IFileCrawler.GetDriverProjects(FileInfo[] projects)
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

        // Static methods for backwards compatibility
        public static FileInfo[] GetAllProjectsInFolders(string path, bool filter = true)
        {
            return ((IFileCrawler)new FileCrawler(_defaultFileSystem)).GetAllProjectsInFolders(path, filter);
        }

        public static FileInfo GetFileInfo(string path)
        {
            return ((IFileCrawler)new FileCrawler(_defaultFileSystem)).GetFileInfo(path);
        }

        public static List<FileInfo> GetSampleProjects(FileInfo[] projects)
        {
            return ((IFileCrawler)new FileCrawler(_defaultFileSystem)).GetSampleProjects(projects);
        }

        public static List<FileInfo> GetDriverProjects(FileInfo[] projects)
        {
            return ((IFileCrawler)new FileCrawler(_defaultFileSystem)).GetDriverProjects(projects);
        }
    }
}
