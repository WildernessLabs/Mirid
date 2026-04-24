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

        readonly FileInfo fileInfo;

        public MFPackageProject(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
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
            if (!File.Exists(fileInfo.FullName))
            {
                throw new FileNotFoundException($"Couldn't find driver project {fileInfo.FullName}");
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(fileInfo.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing project file {fileInfo.FullName}: {ex.Message}");
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
                PackageId = "Meadow.Foundation." + Path.GetFileNameWithoutExtension(fileInfo.Name);
            }
        }

        static string GetElement(XDocument doc, string elementName)
            => doc.Descendants(elementName).FirstOrDefault()?.Value ?? string.Empty;
    }
}
