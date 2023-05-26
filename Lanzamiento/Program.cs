using MeadowRepos;
using ReferenceSwitcher;
using System.Diagnostics;

namespace Lanzamiento
{
    internal class Program
    {
        static string rootDirectory = @"f:\Release";
        static string nugetDirectory = @"f:\LocalNuget";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Lanzamiento");

            ValidateDirectory(rootDirectory);
            ValidateDirectory(nugetDirectory);

            Console.WriteLine("Loading repo data");
            Repos.PopulateRepos();

            string branch = "develop";

            foreach (var repo in Repos.Repositories)
            {
                CloneRepo(rootDirectory, repo.Value.GitHubOrg, repo.Value.Name);
                SetLocalRepoBranch(rootDirectory, repo.Value.Name, branch);
                var path = Path.Combine(rootDirectory, repo.Key, repo.Value.SourceDirectory);
                repo.Value.ProjectFiles = RepoLoader.GetCsProjFiles(path, ProjectType.All);
            }

            foreach (var repo in Repos.Repositories)
            {
                Nugetize(rootDirectory, repo.Value);
            }
        }

        static void Nugetize(string directory, GitRepo repo)
        {
            RefSwitcher.SwitchToPublishingMode(repo.ProjectFiles, GetDependencyProjects(repo.DependencyRepoNames));
        }

        static IEnumerable<FileInfo> GetDependencyProjects(IEnumerable<string> dependencyNames)
        {
            var projects = new List<FileInfo>();

            foreach (var dependency in dependencyNames)
            {   //will throw if doesn't exist ....
                projects.AddRange(Repos.Repositories[dependency].ProjectFiles);
            }
            return projects;
        }

        static void SetLocalRepoBranch(string directory, string githubRepo, string branch)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(Path.Combine(directory, githubRepo)) == false)
            {
                throw new Exception($"{Path.Combine(directory, githubRepo)} doesn't exist, cannot set branch: {branch}");
            }

            Console.WriteLine($"Changing {githubRepo} branch to {branch}");
            ExecuteCommand(fullPath, $"git checkout {branch}");
        }

        static void CloneRepo(string directory, string githubOrg, string githubRepo)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(fullPath))
            {
                Console.WriteLine($"Getting the latest in {githubRepo}/{githubRepo}");
                ExecuteCommand(fullPath, $"git clean -dfx");
                ExecuteCommand(fullPath, $"git pull");
            }
            else
            {
                Console.WriteLine($"Cloning {githubRepo}/{githubRepo}");
                ExecuteCommand(rootDirectory, $"git clone https://www.github.com/{githubOrg}/{githubRepo}");
            }
        }

        public static void ExecuteCommand(string directory, string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                WorkingDirectory = directory
            };

            var process = Process.Start(processInfo);
            process?.WaitForExit();
        }

        static void ValidateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Console.WriteLine($"{directory} does not exist");
                Console.WriteLine($"Creating {directory}");
                Directory.CreateDirectory(directory);
            }
            else
            {
                Console.WriteLine($"{directory} found");
            }
        }
    }
}