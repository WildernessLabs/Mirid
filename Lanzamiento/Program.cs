using ExternalRefReaper;
using MeadowRepos;
using Mirid;
using ReferenceSwitcher;
using System.Diagnostics;
using System.Text.Json;

namespace Lanzamiento
{
    internal class Program
    {
        static string ROOT_DEV_DIRECTORY = @"G:\2500";
        static string NUGET_DIRECTORY = @"G:\LocalNuget";
        static string VERSION = "2.5.0";
        static string NUGET_TOKEN = "";
        static readonly bool testBuild = false;
        static readonly bool buildNugets = true;
        static readonly bool cloneRepos = true;
        static readonly bool tagRelease = true;
        static readonly bool publishNugets = true;
        static readonly bool pushVersionBranch = false;

        static void Main(string[] args)
        {
            if (args.Length >= 3)
            {
                VERSION = args[0];
                ROOT_DEV_DIRECTORY = args[1];
                NUGET_DIRECTORY = args[2];
            }
            else
            {
                Console.WriteLine("Usage: Lanzamiento <version> <root-dev-directory> <nuget-directory>");
                Console.WriteLine("  NuGet tokens are read from nuget-tokens.json next to the executable");
                Console.WriteLine("  Example: Lanzamiento 2.5.0 G:\\2500 G:\\LocalNuget");
                return;
            }

            var now = DateTime.Now;

            Console.WriteLine($"Hello Lanzamiento - {now}");

            ValidateDirectory(ROOT_DEV_DIRECTORY);
            ValidateDirectory(NUGET_DIRECTORY);

            Repos.PopulateRepos();

            Console.WriteLine($"Loaded {Repos.Repositories.Count} repos");

            if (publishNugets)
            {
                var org = Repos.Repositories.Values.First().GitHubOrg;
                NUGET_TOKEN = LoadNugetToken(org);

                if (string.IsNullOrEmpty(NUGET_TOKEN))
                {
                    Console.WriteLine($"Error: no NuGet token found for org '{org}'. Add it to nuget-tokens.json next to the executable.");
                    return;
                }
            }

            string sourceBranch = "develop";
            string targetBranch = $"v{VERSION}";

            foreach (var repo in Repos.Repositories)
            {
                if (cloneRepos)
                {
                    CloneRepo(ROOT_DEV_DIRECTORY, repo.Value.GitHubOrg, repo.Value.Name);
                    SetLocalRepoBranch(ROOT_DEV_DIRECTORY, repo.Value.Name, sourceBranch);
                }
            }

            var allProjects = new List<FileInfo>();

            //sort project dependancies 
            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DEV_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                var repos = RepoLoader.GetCsProjFiles(path, ProjectType.All);
                repo.Value.ProjectFiles = RefSwitcher.SortProjectsByLocalDependencies(repos);
                allProjects.AddRange(repo.Value.ProjectFiles);
            }

            var filterdProjects = RefSwitcher.RemoveNonPackageProjects(allProjects);
            var sortedProjects = RefSwitcher.SortProjectsByLocalDependencies(filterdProjects);

            var packagePropsPath = RefSwitcher.GenerateMeadowPackageProps(sortedProjects, VERSION, ROOT_DEV_DIRECTORY);

            RefSwitcher.UpdatePackageProps(packagePropsPath, ROOT_DEV_DIRECTORY);

            //update projects to swap local refs to nugets
            foreach (var repo in Repos.Repositories)
            {
                Nugetize(repo.Value, version: null); //package props .... no version
                if (repo.Value.DependencyRepoNames.Any())
                {
                    RemoveExternalReferences(ROOT_DEV_DIRECTORY, repo.Value);
                }
                UpdateProjectVersionMetaData();

                Console.WriteLine($"Prepared {repo.Key} for publishing");
            }

            foreach (var project in sortedProjects)
            {
                if (Repos.ExcludedProjects.Any(project.DirectoryName.Contains))
                {
                    continue;
                }

                if (buildNugets)
                {
                    //Console.WriteLine($"Building {project.Name}");
                    BuildProject(project, ROOT_DEV_DIRECTORY, NUGET_DIRECTORY, VERSION);
                }
            }

            if (publishNugets && !testBuild)
            {
                PublishNugets(NUGET_DIRECTORY, VERSION);

                foreach (var repo in Repos.Repositories)
                {
                    if (!testBuild && tagRelease)
                    {
                        TagBranch(ROOT_DEV_DIRECTORY, repo.Value.Name, VERSION);
                    }
                }
            }

            if (pushVersionBranch)
            {
                foreach (var repo in Repos.Repositories)
                {
                    if (repo.Key.Contains("MQTT"))
                    {   //since we'll never update and it doesn't have a main branch
                        continue;
                    }

                    if (!testBuild)
                    {
                        PushVersionBranch(ROOT_DEV_DIRECTORY, repo.Value.Name, VERSION);
                    }
                }
            }

            Console.WriteLine($"Complete - took {DateTime.Now - now}");
        }

        static void UpdateProjectVersionMetaData()
        {
            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DEV_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                var projectFiles = RepoLoader.GetCsProjFiles(path, ProjectType.All);

                foreach (var proj in projectFiles)
                {
                    ProjectWriter.AddUpdateProperty(proj, "Version", $"{VERSION}");
                }
            }
        }

        static void PushVersionBranch(string directory, string githubRepo, string version)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(Path.Combine(directory, githubRepo)) == false)
            {
                throw new Exception($"{Path.Combine(directory, githubRepo)} doesn't exist, cannot push");
            }

            try
            {
                ExecuteCommand(fullPath, $"git add -A", false);
                ExecuteCommand(fullPath, $"git commit -m \"Release {version}\"", false);
                ExecuteCommand(fullPath, $"git push --set-upstream origin v{version}");
                Console.WriteLine($"Pushed {version} branch to {githubRepo}");
            }
            catch
            {
                Console.WriteLine($"Failed to push {version} branch to {githubRepo}");
            }
        }

        static void PublishNugets(string directory, string version)
        {
            var files = Directory.GetFiles(directory, $"*{version}*.nupkg");

            foreach (var file in files)
            {
                var command = $"dotnet nuget push --api-key {NUGET_TOKEN} {file} -s https://api.nuget.org/v3/index.json";

                for (int i = 0; i < 5; i++)
                {
                    if (0 == ExecuteCommand(directory, command, false))
                    {
                        Console.WriteLine($"Published {file}");
                        break;
                    }
                    Thread.Sleep(500);
                }
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
            var searchPath = Path.Combine(directory, repo.Name, repo.SourceDirectory);
            var slnFile = Directory.GetFiles(searchPath, "*.sln").FirstOrDefault()
                       ?? Directory.GetFiles(searchPath, "*.slnx").FirstOrDefault();

            if (string.IsNullOrEmpty(slnFile))
            {
                UpdateConsoleMessage($"*** Could not find solution for {repo.Name} to remove refs - ok for Meadow.Logging, Meadow.Units and MQTTnet");
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

        static void TagBranch(string directory, string githubRepo, string version)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(fullPath))
            {
                ExecuteCommand(fullPath, $"git tag {version}");
                ExecuteCommand(fullPath, $"git push origin --tags");
                Console.WriteLine($"Tagged {githubRepo}/{githubRepo}");
            }
        }

        static void CloneRepo(string directory, string githubOrg, string githubRepo)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(fullPath))
            {
                ExecuteCommand(fullPath, $"git clean -dfx");
                ExecuteCommand(fullPath, $"git reset --hard");
                ExecuteCommand(fullPath, $"git pull");
                Console.Write($"Pulled {githubOrg}/{githubRepo}");
            }
            else
            {
                ExecuteCommand(directory, $"git clone https://www.github.com/{githubOrg}/{githubRepo}");
                Console.Write($"Cloned {githubOrg}/{githubRepo}");
            }
        }

        public static int ExecuteCommand(string directory, string command, bool throwOnError = true)
        {
            return ExecuteCommand(directory, command, out _, throwOnError);
        }

        public static int ExecuteCommand(string directory, string command, out string output, bool throwOnError = true)
        {
            int exitCode;

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
                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                // Check for errors by examining the exit code.
                exitCode = process.ExitCode;

                if (exitCode != 0)
                {
                    if (throwOnError)
                    {
                        // Handle the error by throwing an exception.
                        throw new Exception($"Command exited with error code {exitCode}\nOutput:\n{output}");
                    }
                    else
                    {
                        Console.WriteLine($"*** ExecuteCommand failed: {command}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during process execution.
                Console.WriteLine($"Error executing the command: {command}");
                Console.WriteLine(ex.Message);
                throw; // Re-throw the exception to propagate it further if needed.
            }

            return exitCode;
        }

        static string LoadNugetToken(string org)
        {
            var tokensPath = Path.Combine(AppContext.BaseDirectory, "nuget-tokens.json");

            if (!File.Exists(tokensPath))
            {
                Console.WriteLine($"nuget-tokens.json not found at {tokensPath}");
                return "";
            }

            var tokens = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(tokensPath));
            return tokens?.GetValueOrDefault(org) ?? "";
        }

        static void ValidateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                UpdateConsoleStatus($"{directory} does not exist - creating");
                Directory.CreateDirectory(directory);
            }
        }

        static void UpdateConsoleStatus(string status)
        {
            Console.WriteLine(status.PadRight(80));
        }

        static void UpdateConsoleMessage(string message)
        {
            Console.WriteLine(message.PadRight(80));
        }
    }
}