namespace Mirid
{
    public interface IFileCrawler
    {
        FileInfo[] GetAllProjectsInFolders(string path, bool filter = true);
        FileInfo GetFileInfo(string path);
        List<FileInfo> GetSampleProjects(FileInfo[] projects);
        List<FileInfo> GetDriverProjects(FileInfo[] projects);
    }
}
