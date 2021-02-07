using System.Text;

namespace Mirid
{
    public static class MDWriter
    {
        public static string GetH3(string text)
        {
            return $"### {text}";
        }

        public static string GetH2(string text)
        {
            return $"## {text}";
        }

        public static string GetH1(string text)
        {
            return $"# {text}";
        }

        public static string GetTableRow(string[] text)
        {
            StringBuilder result = new StringBuilder();

            result.Append("|");

            return result.ToString();
        }
    }
}