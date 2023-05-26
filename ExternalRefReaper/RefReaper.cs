namespace ExternalRefReaper
{
    public class RefReaper
    {
        public static void RemoveExternalRefs(string solutionFilename)
        {
            if (File.Exists(solutionFilename) == false)
            {
                throw new FileNotFoundException($"{solutionFilename} does not exist. Cannot remove references.");
            }

            var lines = File.ReadAllLines(solutionFilename).ToList();

            List<string> guidsToRemove = new();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("External"))
                {
                    guidsToRemove.Add(GetGuidString(lines[i], true));
                }
                else if (lines[i].Contains("..\\"))
                {
                    guidsToRemove.Add(GetGuidString(lines[i], true));
                }
            }

            int index = 0;
            bool foundGuid;

            while (index < lines.Count)
            {
                foundGuid = false;
                foreach (var guid in guidsToRemove)
                {
                    if (lines[index].Contains(guid))
                    {
                        lines.RemoveAt(index);

                        if (lines[index].Contains("EndProject"))
                        {
                            lines.RemoveAt(index);
                        }
                        foundGuid = true;
                    }
                }

                if (foundGuid == false)
                {
                    index++;
                }
            }

            File.WriteAllLines(solutionFilename, lines);
        }

        public static void RemoveExternalRefsOld(string solutionFilename)
        {
            if (File.Exists(solutionFilename) == false)
            {
                throw new FileNotFoundException($"{solutionFilename} does not exist. Cannot remove references.");
            }

            var lines = File.ReadAllLines(solutionFilename).ToList();

            var (Start, End) = GetExternalRefIndexes(lines);

            if (Start == -1)
            {
                Console.WriteLine($"No external references found in {solutionFilename}");
                return;
            }

            List<string> guidsToRemove = new()
            {
                GetGuidString(lines[Start + 1], true)
            };

            for (int i = Start + 1; i < End - 1; i++)
            {
                guidsToRemove.Add(GetGuidString(lines[i], false));
            }

            //first delete the global section entries
            lines.RemoveAt(End);
            lines.RemoveAt(Start);

            int index = 0;
            bool foundGuid;

            while (index < lines.Count)
            {
                foundGuid = false;
                foreach (var guid in guidsToRemove)
                {
                    if (lines[index].Contains(guid))
                    {
                        lines.RemoveAt(index);

                        if (lines[index].Contains("EndProject"))
                        {
                            lines.RemoveAt(index);
                        }
                        foundGuid = true;
                    }
                }

                if (foundGuid == false)
                {
                    index++;
                }

            }

            /*

            // Remove lines containing the specified GUIDs
            lines.RemoveAll(line => guidsToRemove.Any(guid => line.Contains(guid)));

            // Remove sequential lines if both lines contain "EndProject" from lines
            index = 0;

            while (index < lines.Count)
            {
                if (lines[index].Contains("EndProject") && lines[index + 1].Contains("EndProject"))
                {
                    lines.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }

            */

            File.WriteAllLines(solutionFilename, lines);
        }

        static string GetGuidString(string line, bool skipFirst = false)
        {
            var startIndex = line.IndexOf('{') + 1;

            if (skipFirst)
            {
                startIndex = line.IndexOf('{', startIndex) + 1;
            }

            var endIndex = line.IndexOf('}', startIndex);

            return line[startIndex..endIndex];
        }

        static (int Start, int End) GetExternalRefIndexes(List<string> lines)
        {
            int nestedProjectsIndex = -1;
            int endGlobalSectionIndex = -1;

            //Fine the line that contains "(NestedProjects)" in lines and it's EndGlobalSection
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("GlobalSection(NestedProjects)"))
                {
                    nestedProjectsIndex = i;
                }
                if (nestedProjectsIndex != -1 &&
                    lines[i].Contains("EndGlobalSection"))
                {
                    endGlobalSectionIndex = i;
                    break;
                }
            }

            return (nestedProjectsIndex, endGlobalSectionIndex);
        }
    }
}
