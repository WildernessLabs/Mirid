namespace Mirid
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        string[] ReadAllLines(string path);
        void WriteAllLines(string path, string[] contents);
        string ReadAllText(string path);
        void WriteAllText(string path, string content);
        FileInfo[] GetFiles(string path, string searchPattern, SearchOption searchOption);
        DirectoryInfo[] GetDirectories(string path);
    }
}
