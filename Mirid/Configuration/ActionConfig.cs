namespace Mirid.Configuration;

/// <summary>
/// Configuration for which actions to run.
/// </summary>
public class ActionConfig
{
    public bool UpdateProjectMetadata { get; set; } = true;
    public bool UpdateDocs { get; set; } = true;
    public bool WritePeripheralTables { get; set; } = true;
    public bool RunDriverReport { get; set; } = false;
}
