namespace Mirid
{
    public static class Constants
    {
        public static string GetDocsApiPrefix(string identifier)
        {
            if (identifier.Contains(".CompositeDevices")) return "/docs/api/Meadow.Foundation.CompositeDevices/";
            if (identifier.Contains(".Grove"))            return "/docs/api/Meadow.Foundation.Grove/";
            if (identifier.Contains(".FeatherWings"))     return "/docs/api/Meadow.Foundation.FeatherWings/";
            if (identifier.Contains(".MBus"))             return "/docs/api/Meadow.Foundation.MBus/";
            return "/docs/api/Meadow.Foundation/";
        }


        public static string BlockedBadgeHtml => $"<img alt=\"{blockedAltText}\" src=\"{blockedBadgeLink}\"/>";
        public static string InProgressBadgeHtml => $"<img alt=\"{inProgressAltText}\" src=\"{inProgressBadgeLink}\"/>";
        public static string WorkingBadgeHtml => $"<img alt=\"{workingAltText}\" src=\"{workingBadgeLink}\"/>";

        public static string BlockedBadgeHtmlwStyle => $"<img src=\"{blockedBadgeLink}\" style={headerStyle} alt=\"{blockedAltText}\" />";
        public static string InProgressBadgeHtmlwStyle => $"<img src=\"{inProgressBadgeLink}\" style={headerStyle} alt=\"{inProgressAltText}\" />";
        public static string WorkingBadgeHtmlwStyle => $"<img src=\"{workingBadgeLink}\" style={headerStyle} alt=\"{workingAltText}\" />";


        static readonly string headerStyle = "{{ width: \"auto\", height: \"-webkit-fill-available\" }}";

        static readonly string blockedBadgeLink = "https://img.shields.io/badge/Blocked-red";
        static readonly string inProgressBadgeLink = "https://img.shields.io/badge/InProgress-yellow";
        static readonly string workingBadgeLink = "https://img.shields.io/badge/Working-brightgreen";

        static readonly string blockedAltText = "Status badge: blocked";
        static readonly string inProgressAltText = "Status badge: in-progress";
        static readonly string workingAltText = "Status badge: working";
    }
}