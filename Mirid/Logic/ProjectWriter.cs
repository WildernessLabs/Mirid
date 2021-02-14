using System.IO;
using System.Linq;

namespace Mirid
{
    class ProjectWriter
    {
        public static bool AddUpdateProperty(FileInfo file, string property, string value)
        {
            //load project
            var lines = File.ReadAllLines(file.FullName).ToList();

            //find property
            int index = -1;
            int indexProperyGroup = -1;
            for(int i = 0; i < lines.Count; i++)
            {
                if(lines[i].Contains("<PropertyGroup>"))
                {
                    indexProperyGroup = i;
                }
                else if(lines[i].Contains($"<{property}>"))
                {
                    if(lines[i].Contains($">{value}<"))
                    {
                        return true;
                    }

                    index = i;
                    break;
                }
            }

            if(indexProperyGroup == -1)
            {
                return false;
            }

            if(index == -1)
            {
                lines.Insert(indexProperyGroup + 1, $"    <{property}>{value}</{property}>");
            }
            else
            {
                if(lines[index].Contains($"<{value}>") == false)
                {
                    lines[index] = $"    <{property}>{value}</{property}>";
                }
            }

            File.WriteAllLines(file.FullName, lines.ToArray());

            return true;
        }
    }

    
}
