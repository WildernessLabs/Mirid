namespace Mirid.Configuration;

/// <summary>
/// Configuration for all driver sets.
/// </summary>
public class DriverSetsConfig
{
    public DriverSetConfig CorePeripherals { get; set; } = new();
    public DriverSetConfig LibrariesAndFrameworks { get; set; } = new() { Enabled = false };
    public DriverSetConfig ExternalPeripherals { get; set; } = new();
    public DriverSetConfig FeatherWings { get; set; } = new();
    public DriverSetConfig SeeedStudioGrove { get; set; } = new();
    public DriverSetConfig CompositeDevices { get; set; } = new();
}
