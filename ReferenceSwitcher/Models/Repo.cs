using System.Collections.Generic;
using System.IO;

namespace ReferenceSwitcher
{
    public class Repo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public IEnumerable<FileInfo> ProjectFiles { get; set; }
    }
}