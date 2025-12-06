namespace Mirid
{
    public interface IProjectWriter
    {
        bool AddOrReplaceReference(FileInfo project, string reference, string lineMatch);
        bool AddReference(FileInfo project, string reference);
        bool AddReference(FileInfo project, FileInfo reference);
        bool AddNuget(FileInfo project, string packageName);
        bool RemoveReference(FileInfo project, FileInfo reference);
        bool DeleteProperty(FileInfo file, string property);
        bool AddUpdateProperty(FileInfo file, string property, string value);
        bool RemoveMeadowConfig(FileInfo file);
    }
}
