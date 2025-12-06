namespace Mirid
{
    public class ProjectValidator : IProjectValidator
    {
        private readonly IFileSystem _fileSystem;
        private static readonly IFileSystem _defaultFileSystem = new FileSystem();

        public ProjectValidator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        bool IProjectValidator.DoesProjectContainMatchingClass(FileInfo projectFile)
        {
            var driverName = projectFile.Name.Substring(0, projectFile.Name.IndexOf(".csproj"));
            driverName = driverName.Substring(driverName.LastIndexOf(".") + 1);

            var directory = projectFile.Directory;

            bool exists = _fileSystem.FileExists(Path.Combine(directory.FullName, driverName + ".cs"));

            if (exists == false)
            {
                exists = _fileSystem.FileExists(Path.Combine(directory.FullName, driverName + "Base.cs"));
            }
            if (exists == false)
            {
                exists = _fileSystem.FileExists(Path.Combine(directory.FullName, driverName + "Core.cs"));
            }
            return exists;
        }

        bool IProjectValidator.IsProjectInMatchingFolder(FileInfo projectFile)
        {
            return false;
        }
    }

    // Keep old class name for backwards compatibility
    public class Validations
    {
        private static readonly IFileSystem _defaultFileSystem = new FileSystem();

        public static bool DoesProjectContainMatchingClass(FileInfo projectFile)
        {
            return ((IProjectValidator)new ProjectValidator(_defaultFileSystem)).DoesProjectContainMatchingClass(projectFile);
        }

        public static bool IsProjectInMatchingFolder(FileInfo projectFile)
        {
            return ((IProjectValidator)new ProjectValidator(_defaultFileSystem)).IsProjectInMatchingFolder(projectFile);
        }
    }
}