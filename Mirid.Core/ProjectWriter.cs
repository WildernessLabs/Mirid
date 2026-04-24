using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Mirid
{
    public class ProjectWriter
    {
        public static bool AddOrReplaceReference(FileInfo project, string reference, string lineMatch)
        {
            if (project == null || !File.Exists(project.FullName)) return false;

            XDocument doc;
            XElement newElement;
            try
            {
                doc = XDocument.Load(project.FullName);
                newElement = XElement.Parse(reference.Trim());
            }
            catch (Exception ex) { Console.WriteLine($"Error loading {project.FullName}: {ex.Message}"); return false; }

            var matches = doc.Descendants()
                .Where(e => e.Attributes().Any(a => a.Value.Contains(lineMatch)))
                .ToList();

            if (matches.Any())
            {
                matches[0].ReplaceWith(newElement);
                for (int i = 1; i < matches.Count; i++)
                    matches[i].Remove();
            }
            else
            {
                var itemGroup = doc.Root.Elements("ItemGroup").FirstOrDefault();
                if (itemGroup == null)
                {
                    itemGroup = new XElement("ItemGroup");
                    doc.Root.Add(itemGroup);
                }
                itemGroup.AddFirst(newElement);
            }

            return SaveDocument(doc, project.FullName);
        }

        public static bool AddReference(FileInfo project, string reference)
        {
            if (project == null || !File.Exists(project.FullName)) return false;

            XDocument doc;
            XElement newElement;
            try
            {
                doc = XDocument.Load(project.FullName);
                newElement = XElement.Parse(reference.Trim());
            }
            catch (Exception ex) { Console.WriteLine($"Error loading {project.FullName}: {ex.Message}"); return false; }

            var includeValue = newElement.Attribute("Include")?.Value;
            if (includeValue != null &&
                doc.Descendants().Any(e => e.Attribute("Include")?.Value == includeValue))
            {
                return true; // already present
            }

            var itemGroup = doc.Root.Elements("ItemGroup").FirstOrDefault();
            if (itemGroup == null)
            {
                itemGroup = new XElement("ItemGroup");
                doc.Root.Add(itemGroup);
            }
            itemGroup.AddFirst(newElement);

            return SaveDocument(doc, project.FullName);
        }

        public static bool AddReference(FileInfo project, FileInfo reference)
        {
            if (project == null || reference == null) return false;

            var relativePath = Path.GetRelativePath(Path.GetDirectoryName(project.FullName), reference.FullName);
            return AddReference(project, $"<ProjectReference Include=\"{relativePath}\"/>");
        }

        public static bool AddNuget(FileInfo project, string packageName)
        {
            if (project == null || !File.Exists(project.FullName)) return false;

            XDocument doc;
            try { doc = XDocument.Load(project.FullName); }
            catch (Exception ex) { Console.WriteLine($"Error loading {project.FullName}: {ex.Message}"); return false; }

            var newElement = new XElement("PackageReference",
                new XAttribute("Include", packageName),
                new XAttribute("Version", "0.*"));

            // Prefer the ItemGroup that already has ProjectReferences
            var itemGroup = doc.Descendants("ProjectReference").FirstOrDefault()?.Parent
                         ?? doc.Root.Elements("ItemGroup").FirstOrDefault();

            if (itemGroup == null)
            {
                itemGroup = new XElement("ItemGroup");
                doc.Root.Add(itemGroup);
            }
            itemGroup.AddFirst(newElement);

            return SaveDocument(doc, project.FullName);
        }

        public static bool RemoveReference(FileInfo project, FileInfo reference)
        {
            if (project == null || reference == null) return false;

            XDocument doc;
            try { doc = XDocument.Load(project.FullName); }
            catch (Exception ex) { Console.WriteLine($"Error loading {project.FullName}: {ex.Message}"); return false; }

            var toRemove = doc.Descendants("ProjectReference")
                .Where(e => e.Attribute("Include")?.Value.Contains(reference.FullName) == true)
                .ToList();

            foreach (var el in toRemove) el.Remove();

            return SaveDocument(doc, project.FullName);
        }

        public static bool DeleteProperty(FileInfo file, string property)
        {
            if (file == null || !File.Exists(file.FullName)) return false;

            XDocument doc;
            try { doc = XDocument.Load(file.FullName); }
            catch (Exception ex) { Console.WriteLine($"Error loading {file.FullName}: {ex.Message}"); return false; }

            doc.Descendants(property).FirstOrDefault()?.Remove();

            return SaveDocument(doc, file.FullName);
        }

        public static bool AddUpdateProperty(FileInfo file, string property, string value)
        {
            if (file == null || !File.Exists(file.FullName)) return false;

            XDocument doc;
            try { doc = XDocument.Load(file.FullName); }
            catch (Exception ex) { Console.WriteLine($"Error loading {file.FullName}: {ex.Message}"); return false; }

            var propGroup = doc.Root.Elements("PropertyGroup").FirstOrDefault();
            if (propGroup == null) return false;

            // Remove duplicates, keep first
            var existing = propGroup.Elements(property).ToList();
            for (int i = 1; i < existing.Count; i++) existing[i].Remove();

            if (existing.Any())
            {
                if (existing[0].Value == value && existing.Count == 1)
                    return true; // already correct, nothing to write
                existing[0].Value = value;
            }
            else
            {
                propGroup.AddFirst(new XElement(property, value));
            }

            return SaveDocument(doc, file.FullName);
        }

        public static bool RemoveMeadowConfig(FileInfo file)
        {
            if (file == null || !File.Exists(file.FullName)) return false;

            XDocument doc;
            try { doc = XDocument.Load(file.FullName); }
            catch (Exception ex) { Console.WriteLine($"Error loading {file.FullName}: {ex.Message}"); return false; }

            var meadowConfigElement = doc.Descendants("None")
                .Where(e => e.Attribute("Update")?.Value == "meadow.config.yaml")
                .FirstOrDefault();

            if (meadowConfigElement != null)
            {
                var parent = meadowConfigElement.Parent;
                meadowConfigElement.Remove();

                // Clean up the ItemGroup if it's now empty
                if (parent != null && !parent.HasElements)
                    parent.Remove();

                return SaveDocument(doc, file.FullName);
            }

            return true;
        }

        static bool SaveDocument(XDocument doc, string path)
        {
            try
            {
                var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
                using var writer = XmlWriter.Create(path, settings);
                doc.Save(writer);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing {path}: {ex.Message}");
                return false;
            }
        }
    }
}
