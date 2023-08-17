using ReferenceSwitcher;

namespace MeadowRepos
{
    public class RepoLoader
    {
        string pathHack = "../../../../../";

        public GitRepo LoadRepo(string name, string path, ProjectType projectType = ProjectType.All)
        {
            var repo = new GitRepo()
            {
                Name = name,
                SourceDirectory = path,
                ProjectFiles = GetCsProjFiles(Path.Combine(pathHack, path), projectType),
            };
            return repo;
        }

        public static FileInfo[] GetCsProjFiles(string path, ProjectType projectsType = ProjectType.Drivers)
        {
            var files = (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);

            var filteredFiles = new List<FileInfo>();

            foreach (var file in files)
            {
                if (projectsType == ProjectType.Drivers &&
                    (file.DirectoryName.Contains("Sample") || file.DirectoryName.Contains("sample")))
                {
                    continue;
                }
                if (projectsType == ProjectType.Samples &&
                    (!file.DirectoryName.Contains("Sample") && !file.DirectoryName.Contains("sample")))
                {
                    continue;
                }
                filteredFiles.Add(file);
            }

            return filteredFiles.ToArray();
        }
    }
}