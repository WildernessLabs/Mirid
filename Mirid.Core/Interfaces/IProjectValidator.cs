namespace Mirid
{
    public interface IProjectValidator
    {
        bool DoesProjectContainMatchingClass(FileInfo projectFile);
        bool IsProjectInMatchingFolder(FileInfo projectFile);
    }
}
