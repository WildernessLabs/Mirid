using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace Mirid.Models
{
    public class MFDriver
    {
        //indexes for writing CSV
        [Index(0)]
        public string Namespace => driverCode.Namespace;
        [Index(1)]
        public string Name => driverCode.Name;
        [Index(2)]
        public bool HasSample => driverSample != null;
        [Index(3)]
        public bool HasSnipSnop => !string.IsNullOrWhiteSpace(SnipSnop);
        [Index(4)]
        public bool HasDocOverride => documentation?.HasOverride ?? false;
        [Index(5)]
        public bool HasFritzing => documentation?.HasFritzing ?? false;
        [Index(6)]
        public bool HasCodeExample => documentation?.HasCodeExample ?? false;
        [Index(7)]
        public bool HasWiringExample => documentation?.HasWiringExample ?? false;
        [Index(8)]
        public bool HasPurchasing => documentation?.HasPurchasing ?? false;
        [Index(9)]
        public bool IsPublished => isPublished;



        [Ignore]
        public string SimpleName => driverCode.Name.Split('.').LastOrDefault();
        
        [Ignore]
        public string SnipSnop => driverSample?.GetSnipSnop();
        
        MFDriverCode driverCode;
        MFDriverSample driverSample;
        MFDriverDocumentation documentation;

        string packageName;
        bool isPublished = false;

        public MFDriver(MFPackage package, string driverFileName, MFDriverSample driverSample)
        {
            driverCode = new MFDriverCode(driverFileName);

            packageName = package.PackageName;
            isPublished = package.IsPublished;

            this.driverSample = driverSample;

            //Load documentation
            documentation = new MFDriverDocumentation(this, Program.MFDocsOverridePath);
        }

        public void UpdateDocHeader()
        {
            documentation.UpdateDocHeader(packageName);
        }

        public void UpdateSnipSnop()
        {
            //check if we have a valid SnipSnop
            if (HasSnipSnop == false) return;

            //clean up the snip
            var snip = CleanSnipSnop(SnipSnop);

            //update the driver override file
            documentation.UpdateSnipSnop(snip);
        }

        string CleanSnipSnop(string snipSnop)
        {
            if (string.IsNullOrWhiteSpace(snipSnop)) return string.Empty;

            //split by line
            var lines = snipSnop.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            //remove any lead in blank lines
            while(true)
            {
                if(string.IsNullOrWhiteSpace(lines[0]) == true)
                {
                    lines.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            //find the index of the first non-space character
            int textStart = 0;
            for(int i = 0; i < lines[0].Length; i++)
            {
                if(lines[0][i] == ' ')
                {
                    textStart++;
                }
                else
                {
                    break;
                }
            }

            //remove leading spaces uniformly from all lines 
            for(int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length < textStart) continue;

                lines[i] = lines[i].Substring(textStart);
            }

            return string.Join("\r\n", lines);
        }
    }
}