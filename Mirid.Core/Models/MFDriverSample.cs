namespace Mirid.Models
{
    public class MFDriverSample
    {
        public DirectoryInfo DirectoryInfo { get; protected set; }
        FileInfo meadowAppFileInfo;

        public const string SNIP = "//<!=SNIP=>";
        public const string SNOP = "//<!=SNOP=>";

        public string Name => DirectoryInfo.Name;

        public MFDriverSample(DirectoryInfo directoryInfo)
        {
            this.DirectoryInfo = directoryInfo;

            meadowAppFileInfo = directoryInfo.GetFiles("MeadowApp.cs").FirstOrDefault();

            if (meadowAppFileInfo == null)
            {   //for non-Meadow app samples
                meadowAppFileInfo = directoryInfo.GetFiles("Program.cs").FirstOrDefault();
            }
        }

        public static string ExtractRaw(string text)
        {
            int snipIndex = text.IndexOf(SNIP);
            int snopIndex = text.IndexOf(SNOP);

            if (snipIndex == -1 || snopIndex == -1)
                return string.Empty;

            snipIndex += SNIP.Length;

            if (snopIndex <= snipIndex)
                return string.Empty;

            return text.Substring(snipIndex, snopIndex - snipIndex);
        }

        public string GetSnipSnop()
        {
            if (meadowAppFileInfo == null || !File.Exists(meadowAppFileInfo.FullName))
                return string.Empty;

            return ExtractRaw(File.ReadAllText(meadowAppFileInfo.FullName));
        }
    }
}
