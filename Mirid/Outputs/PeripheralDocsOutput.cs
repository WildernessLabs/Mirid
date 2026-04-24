using Mirid.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mirid.Outputs;

public static class PeripheralDocsOutput
{
    static readonly string[] driverIgnoreList = new string[] { "Animations", "Simulated" };

    public static void WritePeripheralTables(List<MFDriverSet> driverSets, string outputPath)
    {
        StringBuilder output = new();

        foreach (var driverSet in driverSets)
        {
            output.AppendLine();
            output.AppendLine($"## {driverSet.SetName}");

            WritePeripheralPackages(driverSet.DriverPackages, output, driverSet.DriverPackages.Count > 1);
        }

        var generated = output.ToString();

        if (File.Exists(outputPath))
        {
            var existing = File.ReadAllText(outputPath);
            var splitIndex = existing.IndexOf("\n## ");
            if (splitIndex != -1)
            {
                generated = existing.Substring(0, splitIndex + 1) + generated;
            }
        }

        File.WriteAllText(outputPath, generated);
        Console.WriteLine($"Peripheral tables written to {outputPath}");
    }


    public static void WritePeripheralTables(List<MFPackage> packages)
    {
        StringBuilder output = new();

        WritePeripheralPackages(packages, output);

        File.WriteAllText("PeripheralTables.md", output.ToString());
    }

    static void WritePeripheralPackages(List<MFPackage> packages, StringBuilder output, bool writeHeader = true)
    {
        string group = string.Empty;

        List<MFPackage> packagesWithMultipleDrivers = new();

        //put the packages in alphabetical order
        packages.Sort((a, b) => a.PackageName.CompareTo(b.PackageName));

        foreach (var package in packages)
        {
            if (group != GetPeripheralGroup(package.PackageName))
            {
                if (packagesWithMultipleDrivers.Count > 0)
                {
                    WriteMultipleDriverTables(packagesWithMultipleDrivers, output);
                }

                packagesWithMultipleDrivers.Clear();
                group = GetPeripheralGroup(package.PackageName);

                if (writeHeader)
                {
                    output.AppendLine();
                    output.AppendLine($"### {group}");
                    output.AppendLine();
                    WriteTableHeader(output);
                }
            }

            if (package.NumberOfDrivers > 1)
            {
                packagesWithMultipleDrivers.Add(package);
                continue;
            }

            WriteTableRow(GetStatusText(package.IsPublished),
                GetPeripheralLink(package, package.Drivers.Count > 1),
                package.Description,
                output);
        }

        if (packagesWithMultipleDrivers.Count > 0)
        {
            WriteMultipleDriverTables(packagesWithMultipleDrivers, output);
        }
    }

    static void WriteMultipleDriverTables(List<MFPackage> nugets, StringBuilder builder)
    {
        string GetDriverType(MFPackage nuget)
        {
            var packageCategories = nuget.PackageName.Split('.');
            var type = packageCategories[packageCategories.Length - 2].TrimEnd('s');

            return type switch
            {
                "Display" => "display",
                "ADC" => "analog digital converter",
                "Hid" => "HID",
                "IOExpander" => "IO expander",
                _ => type.ToLower(),
            };
        }

        string GetDriverUrl(MFDriver driver)
        {
            return $"/docs/api/Meadow.Foundation/{driver.Namespace}.{driver.Name}";
        }

        foreach (var nuget in nugets)
        {
            var driverType = GetDriverType(nuget);

            if (nuget.PackageName != "Meadow.Foundation") //lazy
            {
                builder.AppendLine();
                builder.AppendLine($"#### {GetDriverNameFromPackage(nuget.PackageName)}");
            }
            builder.AppendLine();

            WriteTableHeader(builder);

            //put the drivers in alphabetical order
            nuget.Drivers.Sort((a, b) => a.Name.CompareTo(b.Name));

            //remove drivers if any part of the simple name contains a word in the ignore list
            nuget.Drivers.RemoveAll(d => driverIgnoreList.Any(i => d.SimpleName.Contains(i)));

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

    static string GetPeripheralLink(MFPackage package, bool hasMultipleDrivers)
    {
        var name = GetDriverNameFromPackage(package.PackageName);
        var prefix = Constants.GetDocsApiPrefix(package.ProjectDirectory.FullName + package.PackageName);

        string modifiedPackageName = package.PackageName;

        var postFix = hasMultipleDrivers ? "Base" : string.Empty;

        var url = $"{prefix}{modifiedPackageName}{postFix}";

        return $"[{name}]({url})";
    }

    public static void WritePeripheralTablesSimple(List<MFPackage> nugets)
    {
        StringBuilder output = new();

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
            return package["Meadow.Foundation.".Length..];
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