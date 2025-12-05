namespace Mirid.Configuration;

/// <summary>
/// Configuration for all file paths used by Mirid.
/// </summary>
public class PathConfig
{
    // Meadow Core paths
    public string MeadowCorePath { get; set; } = "../../../../../Meadow.Core/source/";

    // Meadow Foundation paths
    public string MeadowFoundationSourcePath { get; set; } = "../../../../../Meadow.Foundation/Source/";
    public string CorePeripheralsPath { get; set; } = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Core";
    public string CoreGitHubUrl { get; set; } = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Core/";

    public string ExternalPeripheralsPath { get; set; } = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Peripherals";
    public string DocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation";
    public string ExternalPeripheralsGitHubUrl { get; set; } = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Peripherals/";

    public string FrameworksPath { get; set; } = "../../../../../Meadow.Foundation/Source/Meadow.Foundation.Libraries_and_Frameworks";
    public string FrameworksGitHubUrl { get; set; } = "https://github.com/WildernessLabs/Meadow.Foundation/tree/main/Source/Meadow.Foundation.Libraries_and_Frameworks/";

    // Grove paths
    public string GrovePath { get; set; } = "../../../../../Meadow.Foundation.Grove/Source/";
    public string GroveDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.Grove";
    public string GroveGitHubUrl { get; set; } = "https://github.com/WildernessLabs/Meadow.Foundation.Grove/tree/main/Source/";

    // Featherwing paths
    public string FeatherwingPath { get; set; } = "../../../../../Meadow.Foundation.Featherwings/Source/";
    public string FeatherwingDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.Featherwings";
    public string FeatherwingGitHubUrl { get; set; } = "https://github.com/WildernessLabs/Meadow.Foundation.FeatherWings/tree/main/Source/";

    // MikroBus paths
    public string MikroBusPath { get; set; } = "../../../../../Meadow.Foundation.mikroBUS/Source/";
    public string MikroBusDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.MikroBus";
    public string MikroBusGitHubUrl { get; set; } = "https://github.com/WildernessLabs/Meadow.Foundation.MikroBus/tree/main/Source/";

    // Composite devices paths
    public string CompositePath { get; set; } = "../../../../../Meadow.Foundation.CompositeDevices/Source/";
    public string CompositeDocsOverridePath { get; set; } = "../../../../../Documentation/docs/api/Meadow.Foundation.CompositeDevices";
    public string CompositeGitHubUrl { get; set; } = "https://github.com/wildernesslabs/meadow.foundation.compositedevices/tree/main/Source/";
}
