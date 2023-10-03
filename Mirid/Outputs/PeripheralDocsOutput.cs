using Mirid.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mirid.Outputs
{
    public static class PeripheralDocsOutput
    {
        public static void WritePeripheralTables(List<MFDriverSet> driverSets)
        {
            StringBuilder output = new StringBuilder();

            foreach (var driverSet in driverSets)
            {
                output.AppendLine($"## {driverSet.SetName}");

                WritePeripheralPackages(driverSet.DriverPackages, output);
            }

            File.WriteAllText("PeripheralTablesCombined.md", output.ToString());
        }


        public static void WritePeripheralTables(List<MFPackage> packages)
        {
            StringBuilder output = new StringBuilder();

            WritePeripheralPackages(packages, output);

            File.WriteAllText("PeripheralTables.md", output.ToString());
        }

        static void WritePeripheralPackages(List<MFPackage> packages, StringBuilder output)
        {
            string group = string.Empty;

            List<MFPackage> packagesWithMultipleDrivers = new List<MFPackage>();

            //put the packages in alphabetical order
            packages.Sort((a, b) => a.PackageName.CompareTo(b.PackageName));

            foreach (var package in packages)
            {
                if (group != GetPeripheralGroup(package.PackageName))
                {
                    if (packagesWithMultipleDrivers.Count > 0)
                    {
                        output.AppendLine();
                        WriteMultipleDriverTables(packagesWithMultipleDrivers, output);
                    }

                    packagesWithMultipleDrivers.Clear();
                    group = GetPeripheralGroup(package.PackageName);
                    output.AppendLine();
                    output.AppendLine($"### {group}");
                    output.AppendLine();
                    WriteTableHeader(output);
                }

                if (package.NumberOfDrivers > 1)
                {
                    packagesWithMultipleDrivers.Add(package);
                }

                WriteTableRow(GetStatusText(package.IsPublished),
                    GetPeripheralLink(package.PackageName, package.Drivers.Count > 1),
                    package.Description,
                    output);
            }

            //catch
            if (packagesWithMultipleDrivers.Count > 0)
            {
                output.AppendLine();
                WriteMultipleDriverTables(packagesWithMultipleDrivers, output);
            }
        }

        static void WriteMultipleDriverTables(List<MFPackage> nugets, StringBuilder builder)
        {
            string GetDriverType(MFPackage nuget)
            {
                var packageCategories = nuget.PackageName.Split('.');
                var type = packageCategories[packageCategories.Length - 2].TrimEnd('s');
                switch (type)
                {
                    case "Display":
                        return "display";
                    case "ADC":
                        return "analog digital converter";
                    case "Hid":
                        return "HID";
                    case "IOExpander":
                        return "IO expander";
                    default:
                        return type.ToLower();



                }
            }

            string GetDriverUrl(MFDriver driver)
            {
                return $"/docs/api/Meadow.Foundation/{driver.Namespace}.{driver.Name}.html";
            }

            foreach (var nuget in nugets)
            {
                var driverType = GetDriverType(nuget);

                builder.AppendLine();
                builder.AppendLine($"#### {GetDriverNameFromPackage(nuget.PackageName)}");
                builder.AppendLine();

                WriteTableHeader(builder);

                foreach (var driver in nuget.Drivers)
                {
                    var driverUrl = GetDriverUrl(driver);

                    WriteTableRow(GetStatusText(nuget.IsPublished),
                                        $"[{driver.Name}]({driverUrl})",
                                        $"{driver.SimpleName} {driverType} driver",
                                        builder);
                }
            }
        }

        public static string GetPeripheralGroup(string package)
        {
            var text = package.Split(".");

            if (text.Length < 3)
            {
                return package;
            }

            if (string.Compare(text[2], "Sensors") == 0)
            {
                return text[3];
            }
            return text[2];
        }

        static string GetPeripheralLink(string packageName, bool hasMultipleDrivers)
        {
            var name = GetDriverNameFromPackage(packageName);
            var postFix = hasMultipleDrivers ? "Base" : string.Empty;

            var url = $"/docs/api/Meadow.Foundation/{packageName}{postFix}.html";

            return $"[{name}]({url})";
        }

        public static void WritePeripheralTablesSimple(List<MFPackage> nugets)
        {
            StringBuilder output = new StringBuilder();

            WriteTableHeader(output);

            foreach (var nuget in nugets)
            {
                WriteTableRow(GetStatusText(nuget.IsPublished),
                    GetDriverNameFromPackage(nuget.PackageName),
                    nuget.Description,
                    output);
            }

            File.WriteAllText("SimpleTable.md", output.ToString());
        }

        static string GetDriverNameFromPackage(string package)
        {
            if (package == "Meadow.Foundation")
                return package;

            if (package.Contains("Meadow.Foundation"))
            {
                return package.Substring("Meadow.Foundation.".Length);
            }
            return package;
        }

        static string GetStatusText(bool isWorking)
            => isWorking ? Constants.WorkingBadgeHtml : Constants.InProgressBadgeHtml;
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