using System;
using System.IO;
using System.Linq;

namespace Mirid.Models
{
    public class MFDriverAssets
    {
        public bool HasDataSheet { get; private set; }

        public int NumberOfSamples => (sampleDirectories == null)?0: sampleDirectories.GetDirectories().Count();

        DirectoryInfo sampleDirectories;

        public MFDriverAssets(DirectoryInfo directory)
        {
            var datasheetDir = directory.GetDirectories("Datasheet*").FirstOrDefault();

            if (datasheetDir != null)
            {
                if (datasheetDir.GetFiles().Count() > 0)
                {
                    HasDataSheet = true;
                }
            }

            sampleDirectories = directory.GetDirectories("Sample*").FirstOrDefault();
        }

        public MFDriverSample GetSampleForName(string name)
        {
            var folder = sampleDirectories?.GetDirectories(name);

            if (folder == null || folder.Length == 0)
            {
                return null;
            }

            return new MFDriverSample(folder[0]);
        }
    }
}