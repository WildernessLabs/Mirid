namespace Mirid
{
    public class Validations
    {
        public static bool DoesProjectContainMatchingClass(FileInfo projectFile)
        {
            var driverName = Path.GetFileNameWithoutExtension(projectFile.Name);
            driverName = driverName.Substring(driverName.LastIndexOf(".") + 1);

            var directory = projectFile.Directory;

            bool exists = File.Exists(Path.Combine(directory.FullName, driverName + ".cs"));

            if (exists == false)
            {
                exists = File.Exists(Path.Combine(directory.FullName, driverName + "Base.cs"));
            }
            if (exists == false)
            {
                exists = File.Exists(Path.Combine(directory.FullName, driverName + "Core.cs"));
            }
            return exists;
        }

    }
}