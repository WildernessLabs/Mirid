using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFDriverSample
    {
        DirectoryInfo directoryInfo;
        FileInfo meadowAppFileInfo;

        const string SNIP = "//<!—SNIP—>";
        const string SNOP = "//<!—SNOP—>";

        public string Name => directoryInfo.Name;

        public MFDriverSample(DirectoryInfo directoryInfo)
        {
            this.directoryInfo = directoryInfo;

            meadowAppFileInfo = directoryInfo.GetFiles("MeadowApp.cs").FirstOrDefault();
        }

        public string GetSnipSnop()
        {
            var text = File.ReadAllText(meadowAppFileInfo.FullName);

            int snipIndex = text.IndexOf(SNIP);
            int snopIndex = text.IndexOf(SNOP);

            if(snipIndex == -1 || snopIndex == -1)
            {
                return string.Empty;
            }

            snipIndex += SNIP.Length;

            return text.Substring(snipIndex, snopIndex - snipIndex);
        }

        string LoadFileText(string path)
        {
            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException($"Couldn't find file {path}");
            }

            return File.ReadAllText(path);
        }
    }
}
