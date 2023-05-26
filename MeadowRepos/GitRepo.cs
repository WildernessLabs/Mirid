namespace MeadowRepos
{
    public class GitRepo
    {
        /// <summary>
        /// The repo name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// The GitHub organization
        /// </summary>
        public string GitHubOrg { get; set; } = "WildernessLabs";

        /// <summary>
        /// The sub directory containting the source code
        /// </summary>
        public string SourceDirectory { get; set; } = String.Empty;

        /// <summary>
        /// The names of dependancy repos for nugetizing
        /// </summary>
        public IEnumerable<string> DependencyRepoNames { get; set; } = new List<string>();

        /// <summary>
        /// The project files for nugetizing
        /// </summary>
        public IEnumerable<FileInfo> ProjectFiles { get; set; }
    }
}