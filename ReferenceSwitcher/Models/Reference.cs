using System.Collections.Generic;

namespace ReferenceSwitcher
{
    public class Reference
    {
        public string SourceName { get; set; }

        public List<string> ReferenceNames { get; set; }

        public Reference(string name, List<string> refNames)
        {
            SourceName = name;
            ReferenceNames = refNames;
        }
    }
}