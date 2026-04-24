using MeadowRepos;
using Mirid;
using ReferenceSwitcher;

namespace Metafire;

internal class Program
{
    static string ROOT_DIRECTORY = @"h:\WL";
    static readonly string NUGET_VERSION = "2.2.0";

    static void Main(string[] args)
    {
        if (args.Length > 0)
            ROOT_DIRECTORY = args[0];
        else if (Environment.GetEnvironmentVariable("WL_ROOT") is string envRoot)
            ROOT_DIRECTORY = envRoot;
        else
        {
            Console.WriteLine("Usage: Metafire <wl-root-directory>");
            Console.WriteLine("  Or set WL_ROOT environment variable");
            return;
        }

        Console.Clear();
        Console.WriteLine("Meta all the projs!");

        Repos.PopulateRepos();

        UpdateProjectMetaData();
    }

    static void UpdateProjectMetaData()
    {
        foreach (var repo in Repos.Repositories)
        {
            var path = Path.Combine(ROOT_DIRECTORY, repo.Key, repo.Value.SourceDirectory);
            var projectFiles = RepoLoader.GetCsProjFiles(path, ProjectType.All);

            foreach (var proj in projectFiles)
            {

                if (proj.FullName.Contains("Meadow.Foundation")
                    && proj.FullName.Contains("Sample") == true)
                {
                    ProjectWriter.AddUpdateProperty(proj, "LangVersion", "12");
                    ProjectWriter.DeleteProperty(proj, "Version");
                    ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                    ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                }

                //make sure it's a Meadow.Foundation nuget driver package
                if (proj.FullName.Contains("Meadow.Foundation")
                    && proj.FullName.Contains("Sample") == false
                    && proj.FullName.Contains("Test") == false)
                {
                    //continue; //for now
                    ProjectWriter.AddOrReplaceReference(proj, $"    <None Include=\".\\Readme.md\" Pack=\"true\" PackagePath=\"\" />", "<None Include=\".\\Readme.md\"");
                    ProjectWriter.AddUpdateProperty(proj, "PackageReadmeFile", "Readme.md");
                }

                if (Repos.ExcludedProjects.Any(proj.DirectoryName.Contains))
                {
                    continue;
                }

                //    ProjectWriter.AddUpdateProperty(proj, "Nullable", "enable");
                ProjectWriter.AddUpdateProperty(proj, "LangVersion", "12");
                //    ProjectWriter.AddUpdateProperty(proj, "PackageLicenseExpression", "Apache-2.0");
                //    ProjectWriter.AddUpdateProperty(proj, "GenerateDocumentationFile", "true");
                //    ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                //    ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                //

                //ProjectWriter.AddUpdateProperty(proj, "Version", NUGET_VERSION);

                // ProjectWriter.AddReference(proj, $"    <None Include=\"..\\..\\..\\..\\icon.png\" Pack=\"true\" PackagePath=\"\"/>");
                //ProjectWriter.AddUpdateProperty(proj, "PackageProjectUrl", "http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/");
                //  ProjectWriter.AddUpdateProperty(proj, "PackageIcon", "icon.png");
                ProjectWriter.DeleteProperty(proj, "Version");
            }
        }
    }
}