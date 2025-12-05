namespace Mirid.Configuration;

/// <summary>
/// Root configuration class for Mirid.
/// </summary>
public class MiridConfiguration
{
    public DriverSetsConfig DriverSets { get; set; } = new();
    public ActionConfig Actions { get; set; } = new();
    public PathConfig Paths { get; set; } = new();
}
