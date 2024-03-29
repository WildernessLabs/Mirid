﻿using CsvHelper;
using System.Collections.Generic;
using System.IO;
using Mirid.Models;
using System.Globalization;
using System.Linq;

namespace Mirid.Output
{
    public static class CsvOutput
    {
        public static void WritePackagesCSV(List<MFPackage> packages, string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(packages);
                }
            }
        }

        public static void WriteDriversCSV(List<MFPackage> packages, string filename)
        {
            var drivers = new List<MFDriver>();

            foreach(var package in packages)
            {
                foreach(var d in package.Drivers)
                {
                    drivers.Add(d);
                }
            }

            using (var writer = new StreamWriter(filename))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers);
                }
            }
        }


        /*
        public static void WriteCSVs(List<MFDriver> drivers)
        {
            using (var writer = new StreamWriter("AllDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers);
                }
            }

            using (var writer = new StreamWriter("InProgressDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers.Where(d => d.IsTested == false));
                }
            }

            using (var writer = new StreamWriter("TestedDrivers.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(drivers.Where(d => d.IsTested == true));
                }
            }
        }*/
    }
}
