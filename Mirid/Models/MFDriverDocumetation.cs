﻿using System;
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

        public bool HasOverride => string.IsNullOrWhiteSpace(text) == false;
        public bool HasFritzing => text?.Contains("Fritzing.png") ?? false;
        public bool HasWiringExample => text?.Contains("### Wiring Example") ?? false;
        public bool HasCodeExample => text?.Contains("### Code Example") ?? false;
        public bool HasPurchasing => text?.Contains("# Purchasing") ?? false;


        MFDriver driver;
        string documentationPath;
        string text;
        string simpleNamespace;


        public MFDriverDocumentation(MFDriver driver, string docsPath)
        {
            documentationPath = docsPath;
            this.driver = driver;

            ReadDocsFile();
        }

        public void ReadDocsFile()
        {
            //  var override = Path.Combine()
            var simpleName = driver.SimpleName;

            var index = "Meadow.Foundation.".Length;
            simpleNamespace = driver.Namespace.Substring(index) + "." + driver.SimpleName;

            DocsFileName = driver.Namespace + "." + simpleName + ".md";
            FullPath = Path.Combine(documentationPath, DocsFileName);

            if (File.Exists(FullPath))
            {
                text = File.ReadAllText(FullPath); //ready for processing
            }
        }

        public void UpdateSnipSnop(string snippet)
        {
            //Read the file - should aleady be done 
            if (text == null)
            {
                ReadDocsFile();

                if (text == null)
                {
                    Console.WriteLine($"No docs override for {driver.Name}");
                    return;
                }
            }

            int snipIndex;

            //Find the code snippet 
            if (HasCodeExample)
            {
                //delete old example 
                snipIndex = text.IndexOf("### Code Example");
                int snopIndex = text.IndexOf("###", snipIndex + 1);

                if (snopIndex == -1)
                {
                    snopIndex = text.Length - 1;
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

            snipSnop.Append("[Sample project(s) available on GitHub](");
            snipSnop.Append("https://github.com/WildernessLabs/Meadow.Foundation/tree/master/Source/Meadow.Foundation.Peripherals/");
            snipSnop.Append($"{simpleNamespace}");
            snipSnop.Append($"/Samples/");
            snipSnop.Append($"{simpleNamespace}");
            snipSnop.Append($"_Sample)");
            snipSnop.AppendLine();
            snipSnop.AppendLine();

            text = text.Insert(snipIndex, snipSnop.ToString());

            //now that everything is stored in memory .... we need to update the docs file
            File.WriteAllText(FullPath, text);
        }

        public void UpdateDocHeader(string packageName)
        {
            //Read the file - should aleady be done 
            if (text == null)
            {
                ReadDocsFile();

                if (text == null)
                {
                    Console.WriteLine($"No docs override for {driver.Name}");
                    return;
                }
            }

            //split by line
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            bool isTableStarted = false;
            int tableLineStart = 0;
            int tableLineEnd = 0;

            for(int i = 0; i < lines.Count; i++)
            {
                //find first | 
                if(isTableStarted == false && 
                    lines[i].Length > 0 && 
                    lines[i][0] == '|')
                {
                    isTableStarted = true;
                    tableLineStart = i;
                }
                else if(isTableStarted == true && 
                    lines[i].Length > 0 && 
                    lines[i][0] == '|')
                {
                    tableLineEnd = i;
                }
                else if(isTableStarted == true)
                {
                    break;
                }
            }

            //delete the header table
            for(int i = 0; i < tableLineEnd - tableLineStart + 1; i++)
            {
                lines.RemoveAt(tableLineStart);
            }

            //create the table 
            var table = new List<string>();

            table.Add($"| {driver.SimpleName} | |");
            table.Add($"|--------|--------|");
            table.Add(String.Format("| Status | {0} |", driver.IsPublished ? Constants.WorkingBadgeHtml : Constants.InProgressBadgeHtml));
            var gitUrl = $"https://github.com/WildernessLabs/Meadow.Foundation/tree/master/Source/Meadow.Foundation.Peripherals/{simpleNamespace}";
            table.Add($"| Source code | [GitHub]({gitUrl}) |");
            var nugetUrl = $"<a href=\"https://www.nuget.org/packages/{packageName}/\" target=\"_blank\"><img src=\"https://img.shields.io/nuget/v/{packageName}.svg?label={packageName}\" /></a>"; 
            table.Add($"| NuGet package | {nugetUrl} |");

            //inject new rows at index 
            for(int i = 0; i < table.Count; i++)
            {
                lines.Insert(tableLineStart + i, table[i]);
            }

            //now that everything is stored in memory .... we need to update the docs file
            File.WriteAllLines(FullPath, lines);
        }
    }
}