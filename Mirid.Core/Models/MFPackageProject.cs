using System.IO;

namespace Mirid.Models
{
    public class MFPackageProject
    {
        public string AssemblyName { get; private set; }
        public string CompanyName { get; private set; }
        public string PackageId { get; private set; }
        public string Description { get; private set; }
        public string GeneratePackageOnBuild { get; private set; }
        public string Version { get; private set; }
        public string Authors { get; private set; }

        //private
        string projectText;
        FileInfo fileInfo;

        public MFPackageProject(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            LoadDriverText(fileInfo.FullName);
            ParseElements();
        }

        public bool IsMetadataComplete()
        {
            if (string.IsNullOrWhiteSpace(AssemblyName)) { return false; }
            if (string.IsNullOrWhiteSpace(CompanyName)) { return false; }
            if (string.IsNullOrWhiteSpace(Description)) { return false; }
            if (string.IsNullOrWhiteSpace(PackageId)) { return false; }
            if (string.IsNullOrWhiteSpace(Authors)) { return false; }
            if (string.IsNullOrWhiteSpace(Version)) { return false; }

            return true;
        }

        //could do this on demand but I'm not really worried about memory
        void ParseElements()
        {
            AssemblyName = GetElement("AssemblyName");
            CompanyName = GetElement("Company");
            Version = GetElement("Version");
            PackageId = GetElement("PackageId");
            Description = GetElement("Description");
            GeneratePackageOnBuild = GetElement("GeneratePackageOnBuild");
            Authors = GetElement("Authors");

            if (string.IsNullOrWhiteSpace(PackageId))
            {
                //parse the project name
                PackageId = "Meadow.Foundation." + Path.GetFileNameWithoutExtension(fileInfo.Name);
            }
        }

        void LoadDriverText(string path)
        {
            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException($"Couldn't find driver project {path}");
            }

            try
            {
                projectText = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading project file {path}: {ex.Message}");
                projectText = string.Empty;
            }
        }

        string GetElement(string element)
        {
            int index = projectText.IndexOf(element);

            if (index == -1)
            {
                return string.Empty;
            }

            int start = projectText.IndexOf(">", index) + 1;
            int end = projectText.IndexOf("<", start);

            if (start <= 0 || end < 0 || end <= start)
            {
                return string.Empty;
            }

            return projectText.Substring(start, end - start);
        }
    }
}