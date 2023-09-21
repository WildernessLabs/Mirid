using MeadowRepos;
using System.Text;

namespace Lectura
{
    internal class Program
    {
        static readonly string ROOT_DIRECTORY = @"h:\WL";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Contribuir - contributing.md writer");

            Repos.PopulateRepos();

            CreateContributionDocs();
        }

        static void CreateContributionDocs()
        {
            foreach (var repo in Repos.Repositories)
            {
                var path = Path.Combine(ROOT_DIRECTORY, repo.Key, repo.Value.SourceDirectory);

                WriteContributionDoc(repo.Value, path);
            }
        }

        static void WriteContributionDoc(GitRepo repo, string destinationFolder)
        {
            StringBuilder output = new();

            var rootPathIndex = destinationFolder.IndexOf(repo.SourceDirectory);
            var rootPath = destinationFolder.Substring(0, rootPathIndex);

            var fullPath = Path.Combine(rootPath, "Contributing.md");
            var repoPath = $"https://github.com/{repo.GitHubOrg}/{repo.Name}";

            output.AppendLine($"# Contribute to {repo.Name}");
            output.AppendLine();
            output.AppendLine($"**{repo.Name}** is an open-source project by [Wilderness Labs](https://www.wildernesslabs.co/) and we encourage community feedback and contributions.");
            output.AppendLine();

            output.AppendLine("## How to Contribute");
            output.AppendLine();
            output.AppendLine("- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)");
            output.AppendLine("- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)");
            output.AppendLine($"- Want to **contribute code?** Fork the [{repo.Name}]({repoPath}) repository and submit a pull request against the `develop` branch");
            output.AppendLine();

            output.AppendLine("## Pull Requests");
            output.AppendLine();
            output.AppendLine($"1. All PRs should target the `develop` branch on the {repo.Name} repository.");
            output.AppendLine("2. All new public or protected classes, methods, and properties need XML comment documentation.");
            output.AppendLine("3. Please try to follow the existing coding patterns and practices.");
            output.AppendLine();

            output.AppendLine("## Pull Request Steps");
            output.AppendLine();
            output.AppendLine("1. Fork the repository");
            output.AppendLine($"2. Clone your fork locally: `git clone {repoPath}`");
            output.AppendLine("3. Switch to the `develop` branch");
            output.AppendLine("4. Create a new branch: `git checkout -b feature/your-contribution`");
            output.AppendLine("5. Make your changes and commit: `git commit -m 'Added/Updated [feature/fix]`");
            output.AppendLine("6. Push to your fork: `git push origin feature/your-contribution`");
            output.AppendLine($"7. Open a pull request at [{repo.Name}/pulls]({repoPath}/pulls) targetting the `develop` branch");

            output.AppendLine("## Need Help?");
            output.AppendLine();
            output.AppendLine($"If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).");

            File.WriteAllText(fullPath, output.ToString());
            Console.WriteLine($"Wrote {fullPath}");
        }
    }
}