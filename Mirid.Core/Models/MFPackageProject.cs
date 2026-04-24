using System;
using System.IO;
using System.Xml.Linq;

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

        public FileInfo FileInfo { get; private set; }

        public MFPackageProject(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
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

        void ParseElements()
        {
            if (!File.Exists(FileInfo.FullName))
            {
                throw new FileNotFoundException($"Couldn't find driver project {FileInfo.FullName}");
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(FileInfo.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing project file {FileInfo.FullName}: {ex.Message}");
                return;
            }

            AssemblyName = GetElement(doc, "AssemblyName");
            CompanyName = GetElement(doc, "Company");
            Version = GetElement(doc, "Version");
            PackageId = GetElement(doc, "PackageId");
            Description = GetElement(doc, "Description");
            GeneratePackageOnBuild = GetElement(doc, "GeneratePackageOnBuild");
            Authors = GetElement(doc, "Authors");

            if (string.IsNullOrWhiteSpace(PackageId))
            {
                var stem = Path.GetFileNameWithoutExtension(FileInfo.Name);
                PackageId = stem.StartsWith("Meadow.Foundation.") ? stem : "Meadow.Foundation." + stem;
            }
        }

        static string GetElement(XDocument doc, string elementName)
            => doc.Descendants(elementName).FirstOrDefault()?.Value ?? string.Empty;
    }
}
