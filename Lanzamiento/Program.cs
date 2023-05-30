using ExternalRefReaper;
using MeadowRepos;
using ReferenceSwitcher;
using System.Diagnostics;

namespace Lanzamiento
{
    internal class Program
    {
        static string ROOT_DIRECTORY = @"F:\Release";
        static string NUGET_DIRECTORY = @"F:\LocalNuget";
        static string VERSION = "1.0.0.1";
        static string NUGET_TOKEN = "";

        static void Main(string[] args)
        {
            UpdateConsoleStatus("Hello Lanzamiento");

            ValidateDirectory(ROOT_DIRECTORY);
            ValidateDirectory(NUGET_DIRECTORY);

            UpdateConsoleStatus("Loading repo data");
            Repos.PopulateRepos();

            string sourceBranch = "develop";
            string targetBranch = $"v{VERSION}";

            foreach (var repo in Repos.Repositories)
            {
                CloneRepo(ROOT_DIRECTORY, repo.Value.GitHubOrg, repo.Value.Name);
                SetLocalRepoBranch(ROOT_DIRECTORY, repo.Value.Name, sourceBranch);
                CreateNewBranch(ROOT_DIRECTORY, repo.Value.Name, targetBranch);
                SetLocalRepoBranch(ROOT_DIRECTORY, repo.Value.Name, targetBranch);
            }

            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                var repos = RepoLoader.GetCsProjFiles(path, ProjectType.All);
                repo.Value.ProjectFiles = RefSwitcher.SortProjectsByLocalDependencies(repos);
            }

            foreach (var repo in Repos.Repositories)
            {
                Nugetize(repo.Value);
                RemoveExternalReferences(ROOT_DIRECTORY, repo.Value);
            }

            string[] excludedProjects = { "Simulated", "Sample", "sample", "Test", "test", "Utilities", "Update", "client", "Demo", "Prototype", "ProKit", "HackKit", "Mobile", "mobile" };

            foreach (var repo in Repos.Repositories)
            {
                foreach (var project in repo.Value.ProjectFiles)
                {
                    if (excludedProjects.Any(project.DirectoryName.Contains))
                    {
                        continue;
                    }

                    BuildProject(project, ROOT_DIRECTORY, NUGET_DIRECTORY, VERSION);
                }
            }

            PublishNugets(NUGET_DIRECTORY, VERSION);

            foreach (var repo in Repos.Repositories)
            {
                PushVersionBranch(ROOT_DIRECTORY, repo.Value.Name, VERSION);
            }
        }

        static void PushVersionBranch(string directory, string githubRepo, string version)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(Path.Combine(directory, githubRepo)) == false)
            {
                throw new Exception($"{Path.Combine(directory, githubRepo)} doesn't exist, cannot push");
            }

            //UpdateConsoleStatus($"Creating new branch {CreateNewBranch} on  {githubRepo}");
            ExecuteCommand(fullPath, $"git add -A");
            ExecuteCommand(fullPath, $"git commit -m \"Release {version}\"");
            ExecuteCommand(fullPath, $"git push --set-upstream origin v{version}");
        }

        static void PublishNugets(string directory, string version)
        {
            var files = Directory.GetFiles(directory, $"*{version}*");

            foreach (var file in files)
            {
                ExecuteCommand(directory, $"dotnet nuget push --api-key {NUGET_TOKEN} {file} -s https://api.nuget.org/v3/index.json");
            }
        }

        static void BuildProject(FileInfo project, string rootDirectory, string nugetDirectory, string version)
        {
            if (project.Exists == false)
            {
                UpdateConsoleMessage($"Could not find project {project}");
                return;
            }

            ExecuteCommand(rootDirectory, $"dotnet build -c Release {project.FullName} /p:Version={version}");
            ExecuteCommand(rootDirectory, $"dotnet pack -c Release {project.FullName} /p:Version={version} --output {nugetDirectory}");
        }

        static void RemoveExternalReferences(string directory, GitRepo repo)
        {
            //find the sln
            var slnFile = Directory.GetFiles(Path.Combine(directory, repo.Name, repo.SourceDirectory), "*.sln").FirstOrDefault();

            if (string.IsNullOrEmpty(slnFile))
            {
                UpdateConsoleMessage($"Could not find solution (sln) for {repo.Name} in {directory}");
                return;
            }

            RefReaper.RemoveExternalRefs(slnFile);
        }

        static void Nugetize(GitRepo repo)
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

        static void CreateNewBranch(string directory, string githubRepo, string branch)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(Path.Combine(directory, githubRepo)) == false)
            {
                throw new Exception($"{Path.Combine(directory, githubRepo)} doesn't exist, cannot set branch: {branch}");
            }

            UpdateConsoleStatus($"Creating new branch {CreateNewBranch} on  {githubRepo}");
            ExecuteCommand(fullPath, $"git branch {branch}");
        }

        static void SetLocalRepoBranch(string directory, string githubRepo, string branch)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(Path.Combine(directory, githubRepo)) == false)
            {
                throw new Exception($"{Path.Combine(directory, githubRepo)} doesn't exist, cannot set branch: {branch}");
            }

            UpdateConsoleStatus($"Changing {githubRepo} branch to {branch}");
            ExecuteCommand(fullPath, $"git checkout {branch}");
        }

        static void CloneRepo(string directory, string githubOrg, string githubRepo)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(fullPath))
            {
                UpdateConsoleStatus($"Getting the latest in {githubRepo}/{githubRepo}");
                ExecuteCommand(fullPath, $"git clean -dfx");
                ExecuteCommand(fullPath, $"git reset --hard");
                ExecuteCommand(fullPath, $"git pull");
            }
            else
            {
                UpdateConsoleStatus($"Cloning {githubRepo}/{githubRepo}");
                ExecuteCommand(ROOT_DIRECTORY, $"git clone https://www.github.com/{githubOrg}/{githubRepo}");
            }
        }

        public static void ExecuteCommand(string directory, string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                // RedirectStandardOutput = true,
                WorkingDirectory = directory
            };

            var process = Process.Start(processInfo);

            process.OutputDataReceived += Process_OutputDataReceived;

            process?.WaitForExit();

            var exitCode = process.ExitCode;
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateConsoleMessage(e.Data);
        }

        static void ValidateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                UpdateConsoleStatus($"{directory} does not exist");
                UpdateConsoleStatus($"Creating {directory}");
                Directory.CreateDirectory(directory);
            }
            else
            {
                UpdateConsoleMessage($"{directory} found");
            }
        }

        static void UpdateConsoleStatus(string status)
        {
            //    Console.CursorTop = 1;
            //    Console.CursorLeft = 0;
            Console.WriteLine(status.PadRight(80));
        }

        static void UpdateConsoleMessage(string message)
        {
            //    Console.CursorTop = 3;
            //    Console.CursorLeft = 0;
            Console.WriteLine(message.PadRight(80));
        }
    }
}