using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace Mirid.Models
{
    public class MFDriver
    {
        public string Name => driverCode.Name;
        public string SimpleName => driverCode.Name.Split('.').LastOrDefault();

        public string Namespace => driverCode.Namespace;

        public string SnipSnop => driverSample?.GetSnipSnop();
        public bool HasSnipSnop => !string.IsNullOrWhiteSpace(SnipSnop);

        MFDriverCode driverCode;
        MFDriverSample driverSample;

        public MFDriver(string driverFileName, MFDriverSample driverSample)
        {
            driverCode = new MFDriverCode(driverFileName);

            this.driverSample = driverSample;
        }
    }
}