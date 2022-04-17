using System;
using System.Collections.Generic;
using System.Text;

namespace Mirid
{
    public static class Constants
    {
        public static string BlockedBadgeHtml => "<img src=\"https://img.shields.io/badge/Blocked-red\"/>";
        public static string InProgressBadgeHtml => "<img src=\"https://img.shields.io/badge/InProgress-yellow\"/>  ";
        public static string WorkingBadgeHtml => "<img src=\"https://img.shields.io/badge/Working-brightgreen\"/>";

        //yeah this is pretty lazy - ToDo
        public static string BlockedBadgeHtmlwStyle => "<img src=\"https://img.shields.io/badge/Blocked-red\" style=\"width: auto; height: -webkit-fill-available;\" />";
        public static string InProgressBadgeHtmlwStyle => "<img src=\"https://img.shields.io/badge/InProgress-yellow\" style=\"width: auto; height: -webkit-fill-available;\" />";
        public static string WorkingBadgeHtmlwStyle => "<img src=\"https://img.shields.io/badge/Working-brightgreen\" style=\"width: auto; height: -webkit-fill-available;\" />";

        public static string DocHeaderBadgeStyle => " style=\"width: auto; height: -webkit-fill-available;\"";
    }
}
