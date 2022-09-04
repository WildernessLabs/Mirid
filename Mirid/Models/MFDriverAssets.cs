using System;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFDriverAssets
    {
        public int NumberOfDatasheets => (datasheetsDirectory == null) ? 0 : datasheetsDirectory.GetFiles().Count();

        public string DatasheetPath => $"{datasheetsDirectory}";

        public int NumberOfSamples => (samplesDirectory == null)?0: samplesDirectory.GetDirectories().Count();

        readonly DirectoryInfo samplesDirectory;
        readonly DirectoryInfo datasheetsDirectory;

        public MFDriverAssets(DirectoryInfo directory)
        {
            datasheetsDirectory = directory.GetDirectories("Datasheet*", SearchOption.TopDirectoryOnly).FirstOrDefault();

            samplesDirectory = directory.GetDirectories("Sample*").FirstOrDefault();
        }

        public MFDriverSample GetSampleForName(string name)
        {
            var folder = samplesDirectory?.GetDirectories(name);

            if (folder == null || folder.Length == 0)
            {
                return null;
            }

            return new MFDriverSample(folder[0]);
        }
    }
}