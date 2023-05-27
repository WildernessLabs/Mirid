using ExternalRefReaper;
using MeadowRepos;
using ReferenceSwitcher;
using System.Diagnostics;

namespace Lanzamiento
{
    internal class Program
    {
        static string ROOT_DIRECTORY = @"f:\Release";
        static string NUGET_DIRECTORY = @"f:\LocalNuget";
        static string VERSION = "0.98.1.1";

        static void Main(string[] args)
        {
            UpdateConsoleStatus("Hello Lanzamiento");

            ValidateDirectory(ROOT_DIRECTORY);
            ValidateDirectory(NUGET_DIRECTORY);

            UpdateConsoleStatus("Loading repo data");
            Repos.PopulateRepos();

            string branch = "develop";


            foreach (var repo in Repos.Repositories)
            {
                CloneRepo(ROOT_DIRECTORY, repo.Value.GitHubOrg, repo.Value.Name);
                SetLocalRepoBranch(ROOT_DIRECTORY, repo.Value.Name, branch);
                var path = Path.Combine(ROOT_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                repo.Value.ProjectFiles = RepoLoader.GetCsProjFiles(path, ProjectType.All);
            }

            foreach (var repo in Repos.Repositories)
            {
                Nugetize(ROOT_DIRECTORY, repo.Value);
                RemoveExternalReferences(ROOT_DIRECTORY, repo.Value);
            }

            string[] excludedProjects = { "Sample", "sample", "Test", "test", "Update", "Client", "client", "Demo", "Prototype", "ProKit", "HackKit", "Mobile", "mobile" };

            foreach (var repo in Repos.Repositories)
            {
                foreach (var project in repo.Value.ProjectFiles)
                {
                    if (excludedProjects.Any(project.DirectoryName.Contains))
                    {
                        continue;
                    }

                    BuildProject(project);
                }


                //BuildProject(ROOT_DIRECTORY, repo.Value);
            }
        }

        static void BuildProject(FileInfo project)
        {
            if (project.Exists == false)
            {
                UpdateConsoleMessage($"Could not find project {project}");
                return;
            }

            ExecuteCommand(ROOT_DIRECTORY, $"dotnet build -c Release {project.FullName} /p:Version={VERSION}");
            ExecuteCommand(ROOT_DIRECTORY, $"dotnet pack -c Release {project.FullName} /p:Version={VERSION} --output {NUGET_DIRECTORY}");
        }

        static void BuildProject(string directory, GitRepo repo)
        {
            //find the sln
            var path = Path.Combine(directory, repo.Name, repo.SourceDirectory);
            var projectFile = Directory.GetFiles(path, "*.csproj").FirstOrDefault();

            if (string.IsNullOrEmpty(projectFile))
            {
                UpdateConsoleMessage($"Could not find project for {repo.Name} in {directory}");
                return;
            }

            ExecuteCommand(directory, $"dotnet build --configuration Release {projectFile} /p:Version=${VERSION}");
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