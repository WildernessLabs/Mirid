﻿using MeadowRepos;
using Mirid.Models;
using ReferenceSwitcher;
using System.Text;

namespace Lectura
{
    internal class Program
    {
        static readonly string ROOT_DIRECTORY = @"h:\WL";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Lectura - readme writer");

            Repos.PopulateRepos();

            CreateReadmes();
        }

        static void CreateReadmes()
        {
            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
                var projectFiles = RepoLoader.GetCsProjFiles(path, ProjectType.All);

                foreach (var projectFile in projectFiles)
                {
                    //make sure it's a Meadow.Foundation nuget driver package
                    if (!projectFile.FullName.Contains("Meadow.Foundation"))
                    {
                        continue;
                    }

                    if (Repos.ExcludedProjects.Any(projectFile.DirectoryName.Contains))
                    {
                        continue;
                    }

                    //load project metadata
                    var packageProject = new MFPackageProject(projectFile);

                    //load sample 
                    var sample = LoadSample(projectFile, packageProject.AssemblyName);

                    //write readme
                    WriteReadme(repo.Value, packageProject, projectFile.DirectoryName, sample);
                }
            }
        }

        static string LoadSample(FileInfo projectFile, string name)
        {
            var parentFolder = projectFile.Directory.Parent;

            var samplesDirectory = parentFolder.GetDirectories("Sample*").FirstOrDefault();

            var folder = samplesDirectory?.GetDirectories(name + "_Sample").FirstOrDefault();

            if (folder == null)
            {   //go fuzzy
                folder = samplesDirectory?.GetDirectories("*_Sample").FirstOrDefault();
            }
            if (folder == null)
            {
                return string.Empty;
            }

            var sampleFile = folder.GetFiles("MeadowApp.cs").FirstOrDefault();
            if (sampleFile == null)
            {
                return string.Empty;
            }


            return GetSnipSnop(sampleFile);
        }

        const string SNIP = "//<!=SNIP=>";
        const string SNOP = "//<!=SNOP=>";

        static string GetSnipSnop(FileInfo sampleFile)
        {
            var text = File.ReadAllText(sampleFile.FullName);

            int snipIndex = text.IndexOf(SNIP);
            int snopIndex = text.IndexOf(SNOP);

            if (snipIndex == -1 || snopIndex == -1)
            {
                return string.Empty;
            }

            snipIndex += SNIP.Length;

            var rawText = text.Substring(snipIndex, snopIndex - snipIndex);

            var lines = rawText.Split("\r\n");

            int whiteSpaceCount = 0;
            bool isStart = true;
            bool lastLineIsBlank = false;

            var cleanlines = new List<string>();

            foreach (var line in lines)
            {
                if (isStart == true && string.IsNullOrEmpty(line))
                { continue; }

                isStart = false;

                if (whiteSpaceCount == 0)
                {
                    whiteSpaceCount = line.TakeWhile(c => char.IsWhiteSpace(c)).Count();
                }

                if (line.Length > whiteSpaceCount)
                {
                    cleanlines.Add(line.Substring(whiteSpaceCount));
                    lastLineIsBlank = false;
                }
                else
                {
                    if (lastLineIsBlank == false)
                        cleanlines.Add(line);
                    lastLineIsBlank = true;
                }
            }
            return string.Join("\n", cleanlines);
        }

        static void WriteReadme(GitRepo repo, MFPackageProject packageProject, string destinationFolder, string sample)
        {
            StringBuilder output = new();

            var fullPath = Path.Combine(destinationFolder, "Readme.md");
            var repoPath = $"https://github.com/{repo.GitHubOrg}/{repo.Name}";

            output.AppendLine($"# {packageProject.PackageId}");
            output.AppendLine();
            output.AppendLine($"**{packageProject.Description}**");
            output.AppendLine();

            output.AppendLine($"The **{packageProject.AssemblyName}** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).");
            output.AppendLine();
            output.AppendLine("The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.");
            output.AppendLine();
            output.AppendLine("For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).");
            output.AppendLine();
            output.AppendLine("To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).");
            output.AppendLine();

            if (string.IsNullOrWhiteSpace(sample) == false)
            {
                output.AppendLine("## Usage");
                output.AppendLine();
                output.AppendLine("```csharp");
                output.AppendLine(sample);
                output.AppendLine("```");
            }

            output.AppendLine("## How to Contribute");
            output.AppendLine();
            output.AppendLine("- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)");
            output.AppendLine("- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)");
            output.AppendLine($"- Want to **contribute code?** Fork the [{repo.Name}]({repoPath}) repository and submit a pull request against the `develop` branch");
            output.AppendLine();

            output.AppendLine();
            output.AppendLine("## Need Help?");
            output.AppendLine();
            output.AppendLine($"If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).");

            File.WriteAllText(fullPath, output.ToString());
        }
    }
}