using ExternalRefReaper;
using MeadowRepos;
using ReferenceSwitcher;
using System.Diagnostics;

namespace Lanzamiento
{
    internal class Program
    {
        static string ROOT_DIRECTORY = @"G:\Release1310";
        static string NUGET_DIRECTORY = @"G:\LocalNuget";
        static string VERSION = "1.3.1-beta";
        static string NUGET_TOKEN = "";

        static bool isPreRelease = true;
        static bool testBuild = true;
        static bool cloneRepos = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Lanzamiento");

            ValidateDirectory(ROOT_DIRECTORY);
            ValidateDirectory(NUGET_DIRECTORY);

            Repos.PopulateRepos();

            string sourceBranch = "develop";
            string targetBranch = $"v{VERSION}";

            foreach (var repo in Repos.Repositories)
            {
                if(cloneRepos)
                {
                    CloneRepo(ROOT_DIRECTORY, repo.Value.GitHubOrg, repo.Value.Name);
                    SetLocalRepoBranch(ROOT_DIRECTORY, repo.Value.Name, sourceBranch);
                }

                if (testBuild == false && isPreRelease == false)
                {
                    CreateNewBranch(ROOT_DIRECTORY, repo.Value.Name, targetBranch);
                    SetLocalRepoBranch(ROOT_DIRECTORY, repo.Value.Name, targetBranch);
                }
            }

            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                var repos = RepoLoader.GetCsProjFiles(path, ProjectType.All);
                repo.Value.ProjectFiles = RefSwitcher.SortProjectsByLocalDependencies(repos);
            }

            foreach (var repo in Repos.Repositories)
            {
                Nugetize(repo.Value, isPreRelease ? VERSION : null);
                RemoveExternalReferences(ROOT_DIRECTORY, repo.Value);

                Console.WriteLine($"Prepared {repo.Key} for publishing");
            }

            foreach (var repo in Repos.Repositories)
            {
                foreach (var project in repo.Value.ProjectFiles)
                {
                    if (Repos.ExcludedProjects.Any(project.DirectoryName.Contains))
                    {
                        continue;
                    }

                    BuildProject(project, ROOT_DIRECTORY, NUGET_DIRECTORY, VERSION);
                }
            }

            if (testBuild == true)
            {
                return;
            }

            PublishNugets(NUGET_DIRECTORY, VERSION);

            if (isPreRelease == true)
            {
                return;
            }

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
                UpdateConsoleMessage($"Could not find project {project} - OK for MQTTnet and Logging");
                return;
            }

            ExecuteCommand(rootDirectory, $"dotnet build -c Release {project.FullName} /p:Version={version}");
            ExecuteCommand(rootDirectory, $"dotnet pack -c Release {project.FullName} /p:Version={version} --output {nugetDirectory}");

            UpdateConsoleMessage($"Built {Path.GetFileNameWithoutExtension(project.Name)} nuget");
        }

        static void RemoveExternalReferences(string directory, GitRepo repo)
        {
            //find the sln
            var slnFile = Directory.GetFiles(Path.Combine(directory, repo.Name, repo.SourceDirectory), "*.sln").FirstOrDefault();

            if (string.IsNullOrEmpty(slnFile))
            {
                UpdateConsoleMessage($"Could not find solution for {repo.Name} - ok for Meadow.Logging and MQTTnet");
                return;
            }

            RefReaper.RemoveExternalRefs(slnFile);
        }

        static void Nugetize(GitRepo repo, string? version)
        {
            RefSwitcher.SwitchToPublishingMode(repo.ProjectFiles, GetDependencyProjects(repo.DependencyRepoNames), version);
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

            ExecuteCommand(fullPath, $"git checkout {branch}");
            Console.WriteLine($" and set branch to {branch}");
        }

        static void CloneRepo(string directory, string githubOrg, string githubRepo)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(fullPath))
            {
                ExecuteCommand(fullPath, $"git clean -dfx");
                ExecuteCommand(fullPath, $"git reset --hard");
                ExecuteCommand(fullPath, $"git pull");
                Console.Write($"Pulled {githubRepo}/{githubRepo}");
            }
            else
            {
                ExecuteCommand(ROOT_DIRECTORY, $"git clone https://www.github.com/{githubOrg}/{githubRepo}");
                Console.Write($"Cloned {githubRepo}/{githubRepo}");
            }
        }

        public static void ExecuteCommand(string directory, string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = directory
                };

                var process = new Process
                {
                    StartInfo = processInfo
                };

                process.Start();

                // Read the standard output asynchronously to avoid deadlocks.
                string output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                // Check for errors by examining the exit code.
                int exitCode = process.ExitCode;

                if (exitCode != 0)
                {
                    // Handle the error by throwing an exception.
                    throw new Exception($"Command exited with error code {exitCode}\nOutput:\n{output}");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during process execution.
                Console.WriteLine("Error executing the command:");
                Console.WriteLine(ex.Message);
                throw; // Re-throw the exception to propagate it further if needed.
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateConsoleMessage(e.Data);
        }

        static void ValidateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                UpdateConsoleStatus($"{directory} does not exist - creating");
                Directory.CreateDirectory(directory);
            }
            else
            {
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