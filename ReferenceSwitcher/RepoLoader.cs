using System.Collections.Generic;
using System.IO;
using static ReferenceSwitcher.RefSwitcher;

namespace ReferenceSwitcher
{
    public class RepoLoader
    {
        public Repo LoadRepo(string name, string path)
        {
            var repo = new Repo()
            {
                Name = name,
                Path = path,
                ProjectFiles = GetCsProjFiles(path),
            };
            return repo;
        }

        public FileInfo[] GetCsProjFiles(string path, Projects projectsType = Projects.Drivers)
        {
            var files = (new DirectoryInfo(path)).GetFiles("*.csproj", SearchOption.AllDirectories);

            var filteredFiles = new List<FileInfo>();

            foreach (var file in files)
            {
                if (projectsType == Projects.Drivers &&
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
    }
}
