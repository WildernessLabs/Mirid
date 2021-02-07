using System.Collections.Generic;
using System.IO;
using System.Text;
using Mirid.Models;

namespace Mirid.Outputs
{
    public static class PeripheralDocsOutput
    {
        public static void WritePeripheralTables(List<MFDriver> drivers)
        {
            StringBuilder output = new StringBuilder();
            //we can assume we're in order
            string group = string.Empty;

            List<MFDriver> packagesWithMultipleDrivers = new List<MFDriver>();

            foreach (var driver in drivers)
            {
                if(group != GetPeripheralGroup(driver.PackageName))
                {
                    if(packagesWithMultipleDrivers.Count > 0)
                    {
                        output.AppendLine();
                        WriteMultipleDriverTables(packagesWithMultipleDrivers, output);
                    }

                    packagesWithMultipleDrivers.Clear();
                    group = GetPeripheralGroup(driver.PackageName);
                    output.AppendLine();
                    output.AppendLine($"## {group}");
                    output.AppendLine();
                    WriteTableHeader(output);
                }

                if(driver.NumberOfDrivers > 1)
                {
                    packagesWithMultipleDrivers.Add(driver);
                }

                WriteTableRow(GetStatusText(driver.IsTested),
                    GetPeripheralLink(driver.PackageName),
                    driver.Description,
                    output);
            }

            File.WriteAllText("PeripheralTables.md", output.ToString());
        }

        static void WriteMultipleDriverTables(List<MFDriver> drivers, StringBuilder builder)
        {
            foreach(var driver in drivers)
            {
                builder.AppendLine();
                builder.AppendLine($"### {GetDriverNameFromPackage(driver.PackageName)}");

                WriteTableHeader(builder);

                foreach(var file in driver.CodeFiles)
                {
                    WriteTableRow(GetStatusText(driver.IsTested),
                                        file.Name,
                                        $"{driver.SimpleName} driver",
                                        builder);
                }
            }
        }

        public static string GetPeripheralGroup(string package)
        {
            var text = package.Split(".");

            if(string.Compare(text[2], "Sensors") == 0)
            {
                return text[3];
            }
            return text[2];
        }

        static string GetPeripheralLink(string packageName)
        {
            var name = GetDriverNameFromPackage(packageName);
            var url = $"/docs/api/Meadow.Foundation/{packageName}.html";

            return $"[{name}]({url})";
        }

        public static void WritePeripheralTablesSimple(List<MFDriver> drivers)
        {
            StringBuilder output = new StringBuilder();

            WriteTableHeader(output);

            foreach(var driver in drivers)
            {
                WriteTableRow(GetStatusText(driver.IsTested),
                    GetDriverNameFromPackage(driver.PackageName),
                    driver.Description,
                    output);
            }

            File.WriteAllText("SimpleTable.md", output.ToString());
        }

        static string GetDriverNameFromPackage(string package)
        {
            if(package.Contains("Meadow.Foundation"))
            {
                return package.Substring("Meadow.Foundation.".Length);
            }
            return package;
        }

        static string GetStatusText(bool isWorking)
        {
            if(isWorking)
            {
                return "<img src=\"https://img.shields.io/badge/Working-brightgreen\"/>";
            }
            return "<img src=\"https://img.shields.io/badge/InProgress-yellow\"/>";

     
           // return "<img src=\"https://img.shields.io/badge/Blocked-red\"/>";
        }

        static void WriteTableHeader(StringBuilder builder)
        {
            builder.AppendLine("| Status | Driver | Description |");
            builder.AppendLine("|--------|--------|-------------|");
        }

        static void WriteTableRow(string status, string driver, string description, StringBuilder builder)
        {
            builder.AppendLine($"| {status} | {driver} | {description} |");
        }
    }
}