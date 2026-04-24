using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mirid.Models
{
    public class MFDriverDocumentation
    {
        public string DocsFileName { get; private set; }
        public string FullPath { get; private set; }
        public string UID { get; private set; }

        public bool HasOverride => string.IsNullOrWhiteSpace(text) == false;
        public bool HasFritzing => text?.Contains("Fritzing.png") ?? false;
        public bool HasWiringExample => text?.Contains("### Wiring Example") ?? false;
        public bool HasCodeExample => text?.Contains("### Code Example") ?? false;
        public bool HasPurchasing => text?.Contains("# Purchasing") ?? false;

        readonly MFDriver driver;
        readonly string documentationPath;
        string text;


        public MFDriverDocumentation(MFDriver driver, string docsPath)
        {
            documentationPath = docsPath;
            this.driver = driver;

            ReadDocsFile();
        }

        public void ReadDocsFile()
        {
            var simpleName = driver.SimpleName;

            DocsFileName = driver.Namespace + "." + simpleName + ".md";
            FullPath = Path.Combine(documentationPath, DocsFileName);

            if (File.Exists(FullPath))
            {
                try
                {
                    text = File.ReadAllText(FullPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading docs file {FullPath}: {ex.Message}");
                    return;
                }

                if (text?.Length > 5)
                {
                    int uidStart = text.IndexOf("uid:") + 5;
                    int uidEnd = text.IndexOfAny(new char[] { '\r', '\n' }, uidStart);
                    if (uidStart > 4 && uidEnd > uidStart)
                    {
                        UID = text.Substring(uidStart, uidEnd - uidStart);
                    }
                }
            }
        }

        string GetRootDocsPath(MFDriver driver)
        {
            string rootPath;

            if (driver.FilePath.Contains(".CompositeDevices"))
            {
                rootPath = "/docs/api/Meadow.Foundation.CompositeDevices/";
            }
            else if (driver.Namespace.Contains(".Grove"))
            {
                rootPath = "/docs/api/Meadow.Foundation.Grove/";
            }
            else if (driver.Namespace.Contains(".FeatherWings"))
            {
                rootPath = "/docs/api/Meadow.Foundation.FeatherWings/";
            }
            else if (driver.Namespace.Contains(".mikroBUS"))
            {
                rootPath = "/docs/api/Meadow.Foundation.mikroBUS/";
            }
            else if (driver.Namespace.Contains(".MBus"))
            {
                rootPath = "/docs/api/Meadow.Foundation.MBus/";
            }
            else
            {
                rootPath = "/docs/api/Meadow.Foundation/";
            }
            return rootPath;
        }

        public void CreateOverride()
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"uid: {driver.Namespace}.{driver.Name}");
            sb.AppendLine($"slug: {GetRootDocsPath(driver)}{driver.Namespace}.{driver.Name}");
            sb.AppendLine("---");
            sb.AppendLine("");
            sb.AppendLine("");

            try { File.WriteAllText(FullPath, sb.ToString()); }
            catch (Exception ex) { Console.WriteLine($"Error writing docs file {FullPath}: {ex.Message}"); return; }

            ReadDocsFile();
        }

        public void UpdateSnipSnop(string snippet, string githubUrl)
        {
            int snipIndex;

            //Find the code snippet 
            if (HasCodeExample)
            {
                //delete old example 
                snipIndex = text.IndexOf("### Code Example");
                int snopIndex = text.IndexOf("###", snipIndex + 1);

                if (snopIndex == -1)
                {
                    snopIndex = text.Length;
                }

                text = text.Remove(snipIndex, snopIndex - snipIndex);
            }
            else
            {   //where do we inject the code snippet??
                snipIndex = text.IndexOf("###");
                if (snipIndex == -1)
                {
                    snipIndex = text.Length - 1;//EOF
                }
            }

            //now .... build up the new code snippet
            StringBuilder snipSnop = new StringBuilder();

            snipSnop.AppendLine("### Code Example");
            snipSnop.AppendLine();
            snipSnop.AppendLine("```csharp");
            snipSnop.Append(snippet);
            snipSnop.AppendLine("```");
            snipSnop.AppendLine();

            snipSnop.Append($"[Sample project(s) available on GitHub]({githubUrl})");
            //   snipSnop.Append(githubUrl);
            //   snipSnop.Append($"{driver.Name})");
            snipSnop.AppendLine();
            snipSnop.AppendLine();

            text = text.Insert(snipIndex, snipSnop.ToString());

            try { File.WriteAllText(FullPath, text); }
            catch (Exception ex) { Console.WriteLine($"Error writing docs file {FullPath}: {ex.Message}"); return; }

            ReadDocsFile();
        }

        public void UpdateDocHeader(string packageName, string githubCodeUrl, string githubDatasheetUrl = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            int tableLineStart = -1;
            int tableLineEnd = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 0 && lines[i][0] == '|')
                {
                    if (tableLineStart == -1) tableLineStart = i;
                    tableLineEnd = i;
                }
                else if (tableLineStart != -1)
                {
                    break;
                }
            }

            if (tableLineStart == -1)
            {
                // No existing table — insert after the front-matter block (first blank line after ---)
                tableLineStart = lines.FindIndex(l => l.TrimStart().StartsWith("---")) + 1;
                while (tableLineStart < lines.Count && string.IsNullOrWhiteSpace(lines[tableLineStart]))
                    tableLineStart++;
                tableLineEnd = tableLineStart - 1;
            }

            // Remove existing table rows
            int rowCount = tableLineEnd - tableLineStart + 1;
            for (int i = 0; i < rowCount; i++)
                lines.RemoveAt(tableLineStart);

            //create the table 
            var table = new List<string>
            {
                $"| {driver.SimpleName} | |",
                $"|--------|--------|",
                string.Format("| Status | {0} |", driver.IsPublished ? Constants.WorkingBadgeHtmlwStyle : Constants.InProgressBadgeHtmlwStyle),

                $"| Source code | [GitHub]({githubCodeUrl}) |"
            };

            if (!string.IsNullOrWhiteSpace(githubDatasheetUrl))
            {
                table.Add($"| Datasheet(s) | [GitHub]({githubDatasheetUrl}) |");
            }

            var nugetUrl = $"<a href=\"https://www.nuget.org/packages/{packageName}/\" target=\"_blank\"><img src=\"https://img.shields.io/nuget/v/{packageName}.svg?label={packageName}\" alt=\"NuGet Gallery for {packageName}\" /></a>";
            table.Add($"| NuGet package | {nugetUrl} |");

            //inject new rows at index 
            for (int i = 0; i < table.Count; i++)
            {
                lines.Insert(tableLineStart + i, table[i]);
            }

            //remove the trailing empty line
            lines.RemoveAt(lines.Count - 1);

            try { File.WriteAllLines(FullPath, lines); }
            catch (Exception ex) { Console.WriteLine($"Error writing docs file {FullPath}: {ex.Message}"); return; }

            ReadDocsFile();
        }
    }
}