using ExternalRefReaper;
using MeadowRepos;
using ReferenceSwitcher;
using System.Diagnostics;

namespace Lanzamiento
{
    internal class Program
    {
        static readonly string ROOT_DEV_DIRECTORY = @"G:\1603Dev";
        static readonly string ROOT_MAIN_DIRECTORY = @"G:\1603Main";
        static readonly string NUGET_DIRECTORY = @"G:\LocalNuget";
        static readonly string VERSION = "1.6.0.5-beta";

        static readonly string TIMESTAMP = "2023-10-31 6:52:00";

        static readonly bool testBuild = false;
        static readonly bool buildNugets = true;
        static readonly bool cloneRepos = true;
        static readonly bool tagRelease = true;
        static readonly bool publishNugets = true;
        static readonly bool syncFolders = false;
        static readonly bool pushVersionBranch = false;

        static void Main(string[] args)
        {
            var now = DateTime.Now;

            Console.WriteLine($"Hello Lanzamiento - {now}");

            ValidateDirectory(ROOT_DEV_DIRECTORY);
            ValidateDirectory(ROOT_MAIN_DIRECTORY);
            ValidateDirectory(NUGET_DIRECTORY);

            Repos.PopulateRepos();

            Console.WriteLine($"Loaded {Repos.Repositories.Count} repos");

            string sourceBranch = "develop";
            string updateBranch = "main";
            string targetBranch = $"v{VERSION}";

            foreach (var repo in Repos.Repositories)
            {
                if (cloneRepos == true)
                {
                    CloneRepo(ROOT_DEV_DIRECTORY, repo.Value.GitHubOrg, repo.Value.Name);
                    SetLocalRepoBranch(ROOT_DEV_DIRECTORY, repo.Value.Name, sourceBranch);
                    //   var hash = GetCommitForTimeStamp(ROOT_DEV_DIRECTORY, repo.Value.Name, sourceBranch, $"{TIMESTAMP}");
                    //   SetHeadToCommit(ROOT_DEV_DIRECTORY, repo.Value.Name, hash);
                }
            }

            //sort project dependancies 
            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DEV_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                var repos = RepoLoader.GetCsProjFiles(path, ProjectType.All);
                repo.Value.ProjectFiles = RefSwitcher.SortProjectsByLocalDependencies(repos);
            }

            //update projects to swap local refs to nugets
            foreach (var repo in Repos.Repositories)
            {
                Nugetize(repo.Value, VERSION);
                RemoveExternalReferences(ROOT_DEV_DIRECTORY, repo.Value);

                Console.WriteLine($"Prepared {repo.Key} for publishing");
            }

            //build the projects
            foreach (var repo in Repos.Repositories)
            {
                foreach (var project in repo.Value.ProjectFiles)
                {
                    if (Repos.ExcludedProjects.Any(project.DirectoryName.Contains))
                    {
                        continue;
                    }

                    if (buildNugets)
                    {
                        BuildProject(project, ROOT_DEV_DIRECTORY, NUGET_DIRECTORY, VERSION);
                    }
                }
            }

            if (publishNugets == true && testBuild == false)
            {
                PublishNugets(NUGET_DIRECTORY, VERSION);

                foreach (var repo in Repos.Repositories)
                {
                    if (testBuild == false && tagRelease == true)
                    {
                        TagBranch(ROOT_DEV_DIRECTORY, repo.Value.Name, VERSION);
                    }
                }
            }

            if (syncFolders)
            {
                foreach (var repo in Repos.Repositories)
                {
                    if (repo.Key.Contains("MQTT"))
                    {   //since we'll never update and it doesn't have a main branch
                        continue;
                    }

                    CloneRepo(ROOT_MAIN_DIRECTORY, repo.Value.GitHubOrg, repo.Value.Name);
                    SetLocalRepoBranch(ROOT_MAIN_DIRECTORY, repo.Value.Name, updateBranch);
                    CreateNewBranch(ROOT_MAIN_DIRECTORY, repo.Value.Name, targetBranch);
                    SetLocalRepoBranch(ROOT_MAIN_DIRECTORY, repo.Value.Name, targetBranch);
                    SyncFolder(ROOT_DEV_DIRECTORY, ROOT_MAIN_DIRECTORY, repo.Value.Name);

                    if (pushVersionBranch && testBuild == false)
                    {
                        PushVersionBranch(ROOT_MAIN_DIRECTORY, repo.Value.Name, VERSION);
                    }
                }
            }

            Console.WriteLine($"Complete - took {DateTime.Now - now}");
        }

        static void SyncFolder(string sourceDirectory, string targetDirectory, string githubRepo)
        {
            var fullPathSource = Path.Combine(sourceDirectory, githubRepo);
            var fullPathTarget = Path.Combine(targetDirectory, githubRepo);

            FolderManager.CopyAndDeleteFiles(fullPathSource, fullPathTarget);
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
                if (0 == ExecuteCommand(directory, $"dotnet nuget push --api-key {NUGET_TOKEN} {file} -s https://api.nuget.org/v3/index.json", false))
                {
                    Console.WriteLine($"Published {file}");
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
            //find the sln
            var slnFile = Directory.GetFiles(Path.Combine(directory, repo.Name, repo.SourceDirectory), "*.sln").FirstOrDefault();

            if (string.IsNullOrEmpty(slnFile))
            {
                UpdateConsoleMessage($"*** Could not find solution for {repo.Name} to remove refs - ok for Meadow.Logging and MQTTnet");
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

            Console.Write($"Created new branch {branch} on {githubRepo}");
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

        static void TagBranch(string directory, string githubRepo, string version)
        {
            var fullPath = Path.Combine(directory, githubRepo);

            if (Directory.Exists(fullPath))
            {
                ExecuteCommand(fullPath, $"git tag {version}");
                ExecuteCommand(fullPath, $"git push origin --tags");
                Console.Write($"Tagged {githubRepo}/{githubRepo}");
            }
        }

        //git rev-list -n 1 --before="2023-10-31 17:24:22" develop

        static string GetCommitForTimeStamp(string directory, string githubRepo, string branch, string timeStampString)
        {
            var fullPath = Path.Combine(directory, githubRepo);
            ExecuteCommand(fullPath, $"git rev-list -n 1 --before=\"{timeStampString}\" {branch}", out string output);
            return output.Trim('\n');
        }

        static void SetHeadToCommit(string directory, string githubRepo, string commit)
        {
            var fullPath = Path.Combine(directory, githubRepo);
            ExecuteCommand(fullPath, $"git reset --hard {commit}");
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
                Console.WriteLine("Error executing the command:");
                Console.WriteLine(ex.Message);
                throw; // Re-throw the exception to propagate it further if needed.
            }

            return exitCode;
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