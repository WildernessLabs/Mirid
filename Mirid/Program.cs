using Microsoft.Extensions.Configuration;
using Mirid.Configuration;
using Mirid.Models;
using Mirid.Output;
using Mirid.Outputs;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mirid;

class Program
{
    // Driver set name constants
    private const string CORE_PERIPHERALS = "Core Peripherals";
    private const string LIBRARIES_AND_FRAMEWORKS = "Libraries and Frameworks";
    private const string EXTERNAL_PERIPHERALS = "External Peripherals";
    private const string FEATHERWINGS = "FeatherWings";
    private const string SEEED_STUDIO_GROVE = "Seeed Studio Grove";
    private const string COMPOSITE_DEVICES = "Composite Devices";

    private static readonly Dictionary<string, MFDriverSet> driverSets = new();
    private static MiridConfiguration config = new();

    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Mirid - Meadow Foundation documentation and project management tool");

        var configOption = new Option<string?>("--config") { Description = "Path to a custom configuration file" };
        var updateDocsOption = new Option<bool?>("--update-docs") { Description = "Update documentation files" };
        var noUpdateDocsOption = new Option<bool>("--no-update-docs") { Description = "Skip updating documentation files" };
        var runReportOption = new Option<bool?>("--run-report") { Description = "Run driver report" };
        var noRunReportOption = new Option<bool>("--no-run-report") { Description = "Skip running driver report" };
        var updateMetadataOption = new Option<bool?>("--update-metadata") { Description = "Update project metadata" };
        var noUpdateMetadataOption = new Option<bool>("--no-update-metadata") { Description = "Skip updating project metadata" };
        var writeTablesOption = new Option<bool?>("--write-tables") { Description = "Write peripheral tables" };
        var noWriteTablesOption = new Option<bool>("--no-write-tables") { Description = "Skip writing peripheral tables" };
        var driverSetsOption = new Option<string[]?>("--driver-sets")
        {
            Description = "Specify which driver sets to process (comma-separated: CorePeripherals, LibrariesAndFrameworks, ExternalPeripherals, FeatherWings, SeeedStudioGrove, CompositeDevices)",
            AllowMultipleArgumentsPerToken = true
        };

        rootCommand.Add(configOption);
        rootCommand.Add(updateDocsOption);
        rootCommand.Add(noUpdateDocsOption);
        rootCommand.Add(runReportOption);
        rootCommand.Add(noRunReportOption);
        rootCommand.Add(updateMetadataOption);
        rootCommand.Add(noUpdateMetadataOption);
        rootCommand.Add(writeTablesOption);
        rootCommand.Add(noWriteTablesOption);
        rootCommand.Add(driverSetsOption);

        rootCommand.SetAction((parseResult) =>
        {
            var configPath = parseResult.GetValue(configOption);
            var updateDocs = parseResult.GetValue(updateDocsOption);
            var noUpdateDocs = parseResult.GetValue(noUpdateDocsOption);
            var runReport = parseResult.GetValue(runReportOption);
            var noRunReport = parseResult.GetValue(noRunReportOption);
            var updateMetadata = parseResult.GetValue(updateMetadataOption);
            var noUpdateMetadata = parseResult.GetValue(noUpdateMetadataOption);
            var writeTables = parseResult.GetValue(writeTablesOption);
            var noWriteTables = parseResult.GetValue(noWriteTablesOption);
            var driverSetsArg = parseResult.GetValue(driverSetsOption);

            Run(configPath, updateDocs, noUpdateDocs, runReport, noRunReport,
                updateMetadata, noUpdateMetadata, writeTables, noWriteTables, driverSetsArg);
        });

        return await rootCommand.Parse(args).InvokeAsync();
    }

    private static void Run(string? configPath, bool? updateDocs, bool noUpdateDocs, bool? runReport, bool noRunReport,
        bool? updateMetadata, bool noUpdateMetadata, bool? writeTables, bool noWriteTables, string[]? driverSetsArg)
    {
        Console.Clear();
        Console.WriteLine("Hello Mirid!");

        // Load configuration
        LoadConfiguration(configPath);

        // Apply command-line overrides
        ApplyCommandLineOverrides(updateDocs, noUpdateDocs, runReport, noRunReport,
            updateMetadata, noUpdateMetadata, writeTables, noWriteTables, driverSetsArg);

        // Validate paths
        ValidatePaths();

        // Load driver sets based on configuration
        LoadDriverSets();

        // Execute actions based on configuration
        if (config.Actions.UpdateProjectMetadata)
        {
            UpdateProjectMetadata();
        }

        if (config.Actions.UpdateDocs)
        {
            UpdateDocs();
        }

        if (config.Actions.WritePeripheralTables)
        {
            WritePeripheralTables(driverSets.Values.ToList());
        }

        if (config.Actions.RunDriverReport && driverSets.Count > 0)
        {
            RunDriverReport(driverSets.Values.First());
        }
    }

    private static void LoadConfiguration(string? configPath)
    {
        var configFileName = configPath ?? "appsettings.json";

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(configFileName, optional: true, reloadOnChange: false);

        var configuration = configBuilder.Build();
        config = configuration.Get<MiridConfiguration>() ?? new MiridConfiguration();

        Console.WriteLine($"Configuration loaded from: {configFileName}");
    }

    private static void ApplyCommandLineOverrides(bool? updateDocs, bool noUpdateDocs, bool? runReport, bool noRunReport,
        bool? updateMetadata, bool noUpdateMetadata, bool? writeTables, bool noWriteTables, string[]? driverSetsArg)
    {
        // Apply action overrides (--no-* flags take precedence over --* flags)
        if (noUpdateDocs)
            config.Actions.UpdateDocs = false;
        else if (updateDocs.HasValue)
            config.Actions.UpdateDocs = updateDocs.Value;

        if (noRunReport)
            config.Actions.RunDriverReport = false;
        else if (runReport.HasValue)
            config.Actions.RunDriverReport = runReport.Value;

        if (noUpdateMetadata)
            config.Actions.UpdateProjectMetadata = false;
        else if (updateMetadata.HasValue)
            config.Actions.UpdateProjectMetadata = updateMetadata.Value;

        if (noWriteTables)
            config.Actions.WritePeripheralTables = false;
        else if (writeTables.HasValue)
            config.Actions.WritePeripheralTables = writeTables.Value;

        // Apply driver sets override
        if (driverSetsArg != null && driverSetsArg.Length > 0)
        {
            // Disable all driver sets first
            config.DriverSets.CorePeripherals.Enabled = false;
            config.DriverSets.LibrariesAndFrameworks.Enabled = false;
            config.DriverSets.ExternalPeripherals.Enabled = false;
            config.DriverSets.FeatherWings.Enabled = false;
            config.DriverSets.SeeedStudioGrove.Enabled = false;
            config.DriverSets.CompositeDevices.Enabled = false;

            // Enable only the specified driver sets
            foreach (var driverSet in driverSetsArg)
            {
                var normalizedName = driverSet.Trim().ToLowerInvariant();
                switch (normalizedName)
                {
                    case "coreperipherals":
                        config.DriverSets.CorePeripherals.Enabled = true;
                        break;
                    case "librariesandframeworks":
                        config.DriverSets.LibrariesAndFrameworks.Enabled = true;
                        break;
                    case "externalperipherals":
                        config.DriverSets.ExternalPeripherals.Enabled = true;
                        break;
                    case "featherwings":
                        config.DriverSets.FeatherWings.Enabled = true;
                        break;
                    case "seeedstudiogrove":
                        config.DriverSets.SeeedStudioGrove.Enabled = true;
                        break;
                    case "compositedevices":
                        config.DriverSets.CompositeDevices.Enabled = true;
                        break;
                    default:
                        Console.WriteLine($"Warning: Unknown driver set '{driverSet}' - ignoring");
                        break;
                }
            }
        }
    }

    private static void ValidatePaths()
    {
        var paths = config.Paths;
        var warnings = new List<string>();

        void CheckPath(string path, string name)
        {
            if (!Directory.Exists(path))
            {
                warnings.Add($"Path '{name}' does not exist: {path}");
            }
        }

        // Only validate paths for enabled driver sets
        if (config.DriverSets.CorePeripherals.Enabled)
        {
            CheckPath(paths.CorePeripheralsPath, nameof(paths.CorePeripheralsPath));
        }

        if (config.DriverSets.LibrariesAndFrameworks.Enabled)
        {
            CheckPath(paths.FrameworksPath, nameof(paths.FrameworksPath));
        }

        if (config.DriverSets.ExternalPeripherals.Enabled)
        {
            CheckPath(paths.ExternalPeripheralsPath, nameof(paths.ExternalPeripheralsPath));
        }

        if (config.DriverSets.SeeedStudioGrove.Enabled)
        {
            CheckPath(paths.GrovePath, nameof(paths.GrovePath));
        }

        if (config.DriverSets.FeatherWings.Enabled)
        {
            CheckPath(paths.FeatherwingPath, nameof(paths.FeatherwingPath));
        }

        if (config.DriverSets.CompositeDevices.Enabled)
        {
            CheckPath(paths.CompositePath, nameof(paths.CompositePath));
        }

        foreach (var warning in warnings)
        {
            Console.WriteLine($"Warning: {warning}");
        }
    }

    private static void UpdateProjectMetadata()
    {
        foreach (var driverSet in driverSets.Values)
        {
            driverSet.UpdateProjectMetadata();
        }
    }

    private static void UpdateDocs()
    {
        foreach (var driverSet in driverSets.Values)
        {
            UpdatePeripheralDocs(driverSet);
        }
    }

    private static void LoadDriverSets()
    {
        var paths = config.Paths;

        if (config.DriverSets.CorePeripherals.Enabled)
        {
            Console.WriteLine($"Load {CORE_PERIPHERALS} driver set");
            var coreDriverSet = new MFCoreDriverSet(
                name: CORE_PERIPHERALS,
                MFSourcePath: paths.MeadowFoundationSourcePath,
                driverSourcePath: paths.CorePeripheralsPath,
                docsOverridePath: paths.DocsOverridePath,
                githubUrl: paths.CoreGitHubUrl);
            Console.WriteLine($"Processed {coreDriverSet.DriverPackages.Count} packages with {GetDriverCount(coreDriverSet)} drivers");
            driverSets.Add(CORE_PERIPHERALS, coreDriverSet);
        }

        if (config.DriverSets.LibrariesAndFrameworks.Enabled)
        {
            Console.WriteLine($"Load {LIBRARIES_AND_FRAMEWORKS} driver set");
            var frameworksDriverSet = new MFDriverSet(LIBRARIES_AND_FRAMEWORKS,
                paths.MeadowFoundationSourcePath,
                paths.FrameworksPath,
                paths.DocsOverridePath,
                paths.FrameworksGitHubUrl);
            Console.WriteLine($"Processed {frameworksDriverSet.DriverPackages.Count} packages with {GetDriverCount(frameworksDriverSet)} drivers");
            driverSets.Add(LIBRARIES_AND_FRAMEWORKS, frameworksDriverSet);
        }

        if (config.DriverSets.ExternalPeripherals.Enabled)
        {
            Console.WriteLine($"Load {EXTERNAL_PERIPHERALS} driver set");
            var peripheralsDriverSet = new MFDriverSet(EXTERNAL_PERIPHERALS,
                paths.MeadowFoundationSourcePath,
                paths.ExternalPeripheralsPath,
                paths.DocsOverridePath,
                paths.ExternalPeripheralsGitHubUrl);
            Console.WriteLine($"Processed {peripheralsDriverSet.DriverPackages.Count} packages with {GetDriverCount(peripheralsDriverSet)} drivers");
            driverSets.Add(EXTERNAL_PERIPHERALS, peripheralsDriverSet);
        }

        if (config.DriverSets.SeeedStudioGrove.Enabled)
        {
            Console.WriteLine($"Load {SEEED_STUDIO_GROVE} driver set");
            var groveDriverSet = new MFDriverSet(SEEED_STUDIO_GROVE,
                paths.MeadowFoundationSourcePath,
                paths.GrovePath,
                paths.GroveDocsOverridePath,
                paths.GroveGitHubUrl);
            Console.WriteLine($"Processed {groveDriverSet.DriverPackages.Count} packages with {GetDriverCount(groveDriverSet)} drivers");
            driverSets.Add(SEEED_STUDIO_GROVE, groveDriverSet);
        }

        if (config.DriverSets.FeatherWings.Enabled)
        {
            Console.WriteLine($"Load {FEATHERWINGS} driver set");
            var featherDriverSet = new MFDriverSet(FEATHERWINGS,
                paths.MeadowFoundationSourcePath,
                paths.FeatherwingPath,
                paths.FeatherwingDocsOverridePath,
                paths.FeatherwingGitHubUrl);
            Console.WriteLine($"Processed {featherDriverSet.DriverPackages.Count} packages with {GetDriverCount(featherDriverSet)} drivers");
            driverSets.Add(FEATHERWINGS, featherDriverSet);
        }

        if (config.DriverSets.CompositeDevices.Enabled)
        {
            Console.WriteLine($"Load {COMPOSITE_DEVICES} driver set");
            var compositeDriverSet = new MFDriverSet(COMPOSITE_DEVICES,
                paths.MeadowFoundationSourcePath,
                paths.CompositePath,
                paths.CompositeDocsOverridePath,
                paths.CompositeGitHubUrl);
            Console.WriteLine($"Processed {compositeDriverSet.DriverPackages.Count} packages with {GetDriverCount(compositeDriverSet)} drivers");
            driverSets.Add(COMPOSITE_DEVICES, compositeDriverSet);
        }

        int total = 0;
        foreach (var d in driverSets.Values)
        {
            total += GetDriverCount(d);
        }

        Console.WriteLine($"Found {total} drivers");
    }

    private static int GetDriverCount(MFDriverSet driverSet)
    {
        int count = 0;

        foreach (var p in driverSet.DriverPackages)
        {
            count += p.NumberOfDrivers;
        }
        return count;
    }

    private static void WritePeripheralTables(List<MFDriverSet> driverSets)
    {
        Console.WriteLine("Write Peripheral Tables");
        PeripheralDocsOutput.WritePeripheralTables(driverSets);
    }

    private static void RunDriverReport(MFDriverSet docSet)
    {
        Console.Clear();
        Console.WriteLine("Driver Report");

        CsvOutput.WritePackagesCSV(docSet.DriverPackages, "AllPeripherals.csv");
        CsvOutput.WriteDriversCSV(docSet.DriverPackages, "AllDrivers.csv");

        CsvOutput.WritePackagesCSV(docSet.DriverPackages.Where(d => d.IsPublished == false).ToList(), "InProgressPeripherals.csv");
    }

    private static void UpdatePeripheralDocs(MFDriverSet driverSet)
    {
        int count = 0;

        foreach (var package in driverSet.DriverPackages)
        {
            foreach (var driver in package.Drivers)
            {
                count++;

                if (driver.HasDocOverride == false)
                {
                    Console.WriteLine($"No docs override for {driver.Name} .... creating file ....");
                    driver.CreateDocsOverride();
                }

                var driverDir = Path.GetDirectoryName(driver.FilePath);
                if (string.IsNullOrEmpty(driverDir))
                {
                    Console.WriteLine($"Warning: Could not get directory for {driver.Name}");
                    continue;
                }
                
                var relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, driverDir);
                // Driver folder hack
                if (Path.GetFileName(relativePath) == "Drivers")
                {
                    var parentDir = Path.GetDirectoryName(relativePath);
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        relativePath = parentDir;
                    }
                }
                if (Path.GetFileName(relativePath) == "Driver")
                {
                    var parentDir = Path.GetDirectoryName(relativePath);
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        relativePath = parentDir;
                    }
                }

                var uri = new Uri(driverSet.GitHubUrl);
                var githubCodeUri = new Uri(uri, relativePath);

                string githubDatasheetUrl = string.Empty;
                if (package.HasDataSheet &&
                   !string.IsNullOrWhiteSpace(package.Assets.DatasheetPath))
                {
                    relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, package.Assets.DatasheetPath);
                    uri = new Uri(driverSet.GitHubUrl);
                    var githubDatasheetUri = new Uri(uri, relativePath);
                    githubDatasheetUrl = $"{githubDatasheetUri}";
                }

                driver.UpdateDocHeader($"{githubCodeUri}", githubDatasheetUrl);

                if (driver.HasSample)
                {
                    relativePath = Path.GetRelativePath(driverSet.DriverSetSourcePath, driver.SamplePath);
                    uri = new Uri(driverSet.GitHubUrl);
                    var sampleUri = new Uri(uri, relativePath);

                    driver.UpdateSnipSnop($"{sampleUri}");
                }
                else
                {
                    Console.WriteLine($"No sample found for {driver.Name}");
                }
            }
        }

        Console.WriteLine($"Found {count} drivers in {driverSet.SetName}");
    }
}
