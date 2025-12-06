namespace Mirid
{
    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public void WriteAllLines(string path, string[] contents)
        {
            File.WriteAllLines(path, contents);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public FileInfo[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return (new DirectoryInfo(path)).GetFiles(searchPattern, searchOption);
        }

        public DirectoryInfo[] GetDirectories(string path)
        {
            return (new DirectoryInfo(path)).GetDirectories();
        }
    }
}
