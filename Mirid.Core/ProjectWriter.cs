namespace Mirid
{
    public class ProjectWriter
    {
        public static bool AddOrReplaceReference(FileInfo project, string reference, string lineMatch)
        {
            var lines = File.ReadAllLines(project.FullName).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(reference))
                {   //already have it
                    return true;
                }
                if (lines[i].Contains(lineMatch))
                {
                    lines[i] = reference;
                    File.WriteAllLines(project.FullName, lines.ToArray());
                    return true;
                }
            }

            //find references 
            int indexItemGroup = -1;
            int indexCloseProject = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("<ItemGroup>"))
                {
                    indexItemGroup = i;
                    break;
                }
                if (lines[i].Contains("</Project>"))
                {
                    indexCloseProject = i;
                }
            }

            if (indexItemGroup == -1)
            {
                lines.Insert(indexCloseProject, $"  </ItemGroup>");
                lines.Insert(indexCloseProject, $"  <ItemGroup>");
                indexItemGroup = indexCloseProject;
            }

            //insert 
            lines.Insert(indexItemGroup + 1, reference);

            File.WriteAllLines(project.FullName, lines.ToArray());

            return true;
        }

        public static bool AddReference(FileInfo project, string reference)
        {
            var lines = File.ReadAllLines(project.FullName).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(reference))
                {   //already have it
                    return true;
                }
            }

            //find references 
            int indexItemGroup = -1;
            int indexCloseProject = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("<ItemGroup>"))
                {
                    indexItemGroup = i;
                    break;
                }
                if (lines[i].Contains("</Project>"))
                {
                    indexCloseProject = i;
                }
            }

            if (indexItemGroup == -1)
            {
                lines.Insert(indexCloseProject, $"  </ItemGroup>");
                lines.Insert(indexCloseProject, $"  <ItemGroup>");
                indexItemGroup = indexCloseProject;
            }

            //insert 
            lines.Insert(indexItemGroup + 1, reference);

            File.WriteAllLines(project.FullName, lines.ToArray());

            return true;
        }

        public static bool AddReference(FileInfo project, FileInfo reference)
        {
            if (project == null || reference == null)
            {
                return false;
            }

            var relativePath = Path.GetRelativePath(Path.GetDirectoryName(project.FullName), reference.FullName);
            return AddReference(project, $"    <ProjectReference Include=\"{relativePath}\"/>");
        }

        public static bool AddNuget(FileInfo project, string packageName)
        {
            var lines = File.ReadAllLines(project.FullName).ToList();

            //find references 
            int indexItemGroup = -1;
            int indexCloseProject = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("<ProjectReference"))
                {
                    indexItemGroup = i - 1;
                }
                if (lines[i].Contains("</Project>"))
                {
                    indexCloseProject = i;
                }
            }

            if (indexItemGroup == -1)
            {
                lines.Insert(indexCloseProject, $"  </ItemGroup>");
                lines.Insert(indexCloseProject, $"  <ItemGroup>");
                indexItemGroup = indexCloseProject;
            }

            var reference = $"   <PackageReference Include=\"{packageName}\" Version=\"0.*\" />";

            //insert 
            lines.Insert(indexItemGroup + 1, reference);

            File.WriteAllLines(project.FullName, lines.ToArray());

            return true;
        }

        public static bool RemoveReference(FileInfo project, FileInfo reference)
        {
            if (project == null || reference == null)
            {
                return false;
            }

            var lines = File.ReadAllLines(project.FullName).ToList();

            //find references 
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(reference.FullName))
                {
                    lines.RemoveAt(i);
                }
            }

            File.WriteAllLines(project.FullName, lines.ToArray());

            return true;
        }

        public static bool DeleteProperty(FileInfo file, string property)
        {
            //load project
            var lines = File.ReadAllLines(file.FullName).ToList();

            //find property
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains($"<{property}>"))
                {
                    lines.RemoveAt(i);
                    break;
                }
            }

            File.WriteAllLines(file.FullName, lines.ToArray());

            return true;
        }

        public static bool AddUpdateProperty(FileInfo file, string property, string value)
        {
            //load project
            var lines = File.ReadAllLines(file.FullName).ToList();

            //find property
            int index = -1;
            int indexProperyGroup = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("<PropertyGroup>"))
                {
                    indexProperyGroup = i;
                }
                else if (lines[i].Contains($"<{property}>"))
                {
                    if (lines[i].Contains($">{value}<"))
                    {
                        return true;
                    }

                    index = i;
                    break;
                }
            }

            if (indexProperyGroup == -1)
            {
                return false;
            }

            if (index == -1)
            {
                lines.Insert(indexProperyGroup + 1, $"    <{property}>{value}</{property}>");
            }
            else
            {
                if (lines[index].Contains($"<{value}>") == false)
                {
                    lines[index] = $"    <{property}>{value}</{property}>";
                }
            }

            File.WriteAllLines(file.FullName, lines.ToArray());

            return true;
        }
    }
}