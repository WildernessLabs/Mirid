using System;
using System.Collections.Generic;
using System.IO;
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
        [Ignore]
        public string SimpleName => driverCode.Name.Split('.').LastOrDefault();
        
        [Ignore]
        public string SnipSnop => driverSample?.GetSnipSnop();
        
        

        MFDriverCode driverCode;
        MFDriverSample driverSample;

        public MFDriver(string driverFileName, MFDriverSample driverSample)
        {
            driverCode = new MFDriverCode(driverFileName);

            this.driverSample = driverSample;
        }
    }
}