# Mirid

Internal Wilderness Labs toolkit for Meadow.Foundation release automation and documentation generation.

> Minimal support, but significantly cleaned up. See [Refactor history](#refactor-history) below.

## What it does

Mirid is a solution of eight tools that together handle the full Meadow.Foundation release and docs pipeline:

| Tool | Purpose |
|------|---------|
| **Mirid** | Main tool — crawls all driver packages, extracts metadata and SNIP/SNOP code samples, generates CSV reports and peripheral docs pages for the Documentation site |
| **Lectura** | Generates `Readme.md` for every driver package |
| **Contribuir** | Generates `Contributing.md` for every repo |
| **Lanzamiento** | Full release pipeline — clones repos, switches local refs → NuGet refs, builds, packs, publishes to NuGet.org, tags releases |
| **ActionGen** | Generates GitHub Actions workflow YAMLs for NuGet publishing, split by dependency level (level 1 = no local deps, level 2 = has deps) |
| **Metafire** | Bulk csproj property updates across all repos (LangVersion, Authors, readme refs, etc.) |
| **ReferenceSwitcher** | Library — switches csproj files between `ProjectReference` (dev mode) and `PackageReference` (publishing mode) |
| **ExternalRefReaper** | Strips external/relative project references from `.sln` files before publishing |

## Prerequisites

All repos must be cloned at the same level. Required repos:

- [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation)
- [Meadow.Foundation.Grove](https://github.com/WildernessLabs/Meadow.Foundation.Grove)
- [Meadow.Foundation.Featherwings](https://github.com/WildernessLabs/Meadow.Foundation.Featherwings)
- [Meadow.Foundation.mikroBUS](https://github.com/WildernessLabs/Meadow.Foundation.mikroBUS)
- [Meadow.Foundation.CompositeDevices](https://github.com/WildernessLabs/Meadow.Foundation.CompositeDevices)
- [Documentation](https://github.com/WildernessLabs/Documentation)

## Setup

### Mirid (main tool)

Configuration is in `mirid.config.json` next to the executable. On first run, if the file doesn't exist it will be created with defaults based on relative paths from the binary location:

```json
{
  "mfSourcePath": "../../../../../Meadow.Foundation/Source/",
  "mfCorePeripheralsPath": "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Core",
  "mfPeripheralsPath": "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals",
  "mfDocsOverridePath": "../../../../../Documentation/docs/api/Meadow.Foundation",
  ...
}
```

Edit the paths to match your local layout, then re-run.

### Lectura and Contribuir

Pass the WL root directory as a command-line argument, or set the `WL_ROOT` environment variable:

```
Lectura C:\WL
Contribuir C:\WL

# or
set WL_ROOT=C:\WL
Lectura
```

### Lanzamiento

Requires four command-line arguments:

```
Lanzamiento <version> <root-dev-directory> <nuget-output-directory> <nuget-api-token>

# Example:
Lanzamiento 2.5.0 G:\2500 G:\LocalNuget abc123token
```

### Metafire and ActionGen

Still have hardcoded paths at the top of `Program.cs` — edit before running. Candidates for the same config treatment as Lectura/Contribuir.

## Running

Open `Mirid.sln` and run the tool you need. Each is a standalone console app.

## Refactor history

The codebase was significantly cleaned up in 2026 (Phases 1–8):

| Phase | What changed |
|-------|-------------|
| 1 | Typos in identifiers and filenames |
| 2 | Error handling for file I/O and bounds checks |
| 3 | Hardcoded local paths in main Mirid tool externalized to `mirid.config.json` |
| 4A | String-based `.csproj` parsing replaced with XDocument |
| 4B | String-based namespace extraction replaced with Roslyn |
| 5 | Null-ref crashes and fragile header table logic fixed |
| 6 | Seven null-ref and bounds-check crash fixes across core models |
| 7 | Hardcoded paths in satellite tools (Lectura, Contribuir, Lanzamiento) replaced with CLI args/env vars; ActionGen XML parsing replaced with XDocument; Lectura CRLF split fixed |
| 8 | Remaining string XML parsing in RefSwitcher (4 methods) replaced with XDocument |
