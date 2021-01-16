using System.IO;
using System.Linq;

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


        public MFDriverDocumentation(MFDriver driver, string docsPath)
        {
            documentationPath = docsPath;
            this.driver = driver;

            ReadDocsFile();
        }

        public void ReadDocsFile()
        {
            //  var override = Path.Combine()
            var simpleName = driver.PackageName.Split('.').LastOrDefault();

            DocsFileName = driver.Namespace + "." + simpleName + ".md";
            FullPath = Path.Combine(documentationPath, DocsFileName);

            if(File.Exists(FullPath))
            {
                text = File.ReadAllText(FullPath); //ready for processing
            }
        }
    }
}