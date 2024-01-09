using MeadowRepos;
using Mirid;
using ReferenceSwitcher;

namespace Metafire;

internal class Program
{
    static readonly string ROOT_DIRECTORY = @"h:\WL";
    static readonly string NUGET_VERSION = "1.7.0";

    static void Main(string[] args)
    {
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

                //make sure it's a Meadow.Foundation nuget driver package
                if (proj.FullName.Contains("Meadow.Foundation") && proj.FullName.Contains("Sample") == false)
                {
                    //continue; //for now
                    ProjectWriter.AddOrReplaceReference(proj, $"    <None Include=\".\\Readme.md\" Pack=\"true\" PackagePath=\"\" />", "<None Include=\".\\Readme.md\"");
                    ProjectWriter.AddUpdateProperty(proj, "PackageReadmeFile", "Readme.md");
                }

                if (Repos.ExcludedProjects.Any(proj.DirectoryName.Contains))
                {
                    continue;
                }

                ProjectWriter.AddUpdateProperty(proj, "Nullable", "enable");
                ProjectWriter.AddUpdateProperty(proj, "LangVersion", "10.0");
                ProjectWriter.AddUpdateProperty(proj, "PackageLicenseExpression", "Apache-2.0");
                ProjectWriter.AddUpdateProperty(proj, "GenerateDocumentationFile", "true");
                ProjectWriter.AddUpdateProperty(proj, "Authors", "Wilderness Labs, Inc");
                ProjectWriter.AddUpdateProperty(proj, "Company", "Wilderness Labs, Inc");
                //

                ProjectWriter.AddUpdateProperty(proj, "Version", NUGET_VERSION);

                // ProjectWriter.AddReference(proj, $"    <None Include=\"..\\..\\..\\..\\icon.png\" Pack=\"true\" PackagePath=\"\"/>");
                //ProjectWriter.AddUpdateProperty(proj, "PackageProjectUrl", "http://developer.wildernesslabs.co/Meadow/Meadow.Foundation/");
                //  ProjectWriter.AddUpdateProperty(proj, "PackageIcon", "icon.png");
                //  ProjectWriter.DeleteProperty(proj, "PackageIconUrl");
            }
        }
    }
}